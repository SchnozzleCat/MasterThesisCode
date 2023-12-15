using System.Collections.Generic;
using Schnozzle.GameSpace.Components;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Schnozzle.GameSpace.Jobs
{
    [BurstCompile]
    internal struct HeapNode<T>
        where T : unmanaged
    {
        public T NodeKey;
        public float HCost;
        public float FCost;

        public HeapNode(T node, float hCost, float fCost)
        {
            NodeKey = node;
            HCost = hCost;
            FCost = fCost;
        }
    }

    [BurstCompile]
    internal struct HeapNodeComparer<T> : IComparer<HeapNode<T>>
        where T : unmanaged
    {
        public int Compare(HeapNode<T> x, HeapNode<T> y)
        {
            var fCostComparison = x.FCost.CompareTo(y.FCost);
            if (fCostComparison != 0)
                return fCostComparison;

            return x.HCost.CompareTo(y.HCost);
        }
    }

    [BurstCompile]
    internal struct PathfindingNode<T>
        where T : unmanaged
    {
        public float HCost { get; private set; }
        public float GCost { get; private set; }
        public float FCost { get; private set; }
        public T ParentNode;
        public NativeHeapIndex HeapIndex;

        public PathfindingNode(T parentKey, float hCost, float gCost, NativeHeapIndex heapIndex)
        {
            ParentNode = parentKey;
            HCost = hCost;
            GCost = gCost;
            FCost = hCost + gCost;
            HeapIndex = heapIndex;
        }
    }

    internal struct PathfinderState
    {
        public NativeList<Entity> PathList;
        public NativeParallelHashMap<Entity, PathfindingNode<Entity>> Nodes;
        public NativeHeap<HeapNode<Entity>, HeapNodeComparer<Entity>> OpenSet;
        public NativeParallelHashSet<Entity> ClosedSet;

        public void EnsureCreation()
        {
            if (!PathList.IsCreated)
                PathList = new NativeList<Entity>(Allocator.Temp);
            if (!Nodes.IsCreated)
                Nodes = new NativeParallelHashMap<Entity, PathfindingNode<Entity>>(100, Allocator.Temp);
            if (!OpenSet.IsCreated)
                OpenSet = new NativeHeap<HeapNode<Entity>, HeapNodeComparer<Entity>>(Allocator.Temp);
            if (!ClosedSet.IsCreated)
                ClosedSet = new NativeParallelHashSet<Entity>(100, Allocator.Temp);
        }

        public void Reset()
        {
            PathList.Clear();
            Nodes.Clear();
            OpenSet.Clear();
            ClosedSet.Clear();
        }
    }

    public interface IPathfindingProcessor
    {
        public void OnCreate(ref SystemState state);
        public void OnUpdate(ref SystemState state);
        public void OnDestroy(ref SystemState state);
        public bool ValidateNode(in Entity entity);
        public float3 NodePosition(in Entity entity);
        public float CalculateHCost(in Entity from, in Entity to);
    }

    [BurstCompile]
    [WithChangeFilter(typeof(Pathfinder))]
    [WithAll(typeof(Simulate))]
    public struct PathfindingJob<T> : IJobChunk
        where T : unmanaged, IPathfindingProcessor
    {
        [ReadOnly]
        public BufferLookup<SpatialConnection> SpatialConnectionLookup;

        public EntityTypeHandle EntityHandle;

        public ComponentTypeHandle<Pathfinder> PathfinderHandle;
        public BufferTypeHandle<Path> PathBufferHandle;

        public T Processor;

        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [NativeDisableContainerSafetyRestriction]
        private PathfinderState _state;

        public unsafe void Execute(
            in ArchetypeChunk chunk,
            int unfilteredChunkIndex,
            bool useEnabledMask,
            in v128 chunkEnabledMask
        )
        {
            Assert.IsFalse(useEnabledMask);

            _state.EnsureCreation();

            var entities = chunk.GetNativeArray(EntityHandle);
            var pathfinders = chunk.GetNativeArray(ref PathfinderHandle).GetUnsafePtr();
            var paths = chunk.GetBufferAccessor(ref PathBufferHandle);

            var isTraveler = chunk.Has<TravelSpeed>();
            var isTraveling = chunk.Has<Traveling>();

            for (int i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
                ref var pathfinder = ref UnsafeUtility.ArrayElementAsRef<Pathfinder>(pathfinders, i);
                var path = paths[i];

                ExecuteEntity(entity, unfilteredChunkIndex, ref pathfinder, ref path, isTraveler, isTraveling);
            }
        }

        public void ExecuteEntity(
            Entity entity,
            int unfilteredChunkIndex,
            ref Pathfinder pathfinder,
            ref DynamicBuffer<Path> path,
            bool isTraveler,
            bool isTraveling
        )
        {
            if (pathfinder.CurrentState != Pathfinder.State.Request)
                return;

            // Execute the pathfinding request and update the node's state.
            pathfinder.CurrentState = Pathfind(in pathfinder);

            ref var S = ref _state;

            // On success, reverse the path and write it to the entity's path buffer.
            if (pathfinder.CurrentState == Pathfinder.State.Success)
            {
                path.ResizeUninitialized(S.PathList.Length);
                var newPathArray = path.AsNativeArray();
                var foundPathArray = S.PathList.AsArray().Reinterpret<Path>();
                for (int j = S.PathList.Length - 1, index = 0; j >= 0; j--, index++)
                    newPathArray[index] = foundPathArray[j];
                if (isTraveler && S.PathList.Length > 0)
                {
                    if (isTraveling)
                        CommandBuffer.SetComponent(unfilteredChunkIndex, entity, Traveling.Uninitialized);
                    else
                        CommandBuffer.AddComponent(unfilteredChunkIndex, entity, Traveling.Uninitialized);
                }
            }
        }

        private bool ReconstructPath(in Entity start, in Entity destination)
        {
            ref var S = ref _state;

            var currentNode = destination;
            while (currentNode != start)
            {
                S.PathList.Add(currentNode);
                var nodeData = S.Nodes[currentNode];
                currentNode = nodeData.ParentNode;
            }

            //Add start node at beginning
            S.PathList.Add(start);
            return true;
        }

        private Pathfinder.State Pathfind(in Pathfinder pathfinder)
        {
            ref var S = ref _state;

            S.Reset();

            if (pathfinder.CurrentState == Pathfinder.State.None)
                return Pathfinder.State.None;
            if (pathfinder.Start == pathfinder.End)
                return Pathfinder.State.Success;
            if (!Processor.ValidateNode(pathfinder.Start))
                return Pathfinder.State.FailureStartNodeNotValid;
            if (!Processor.ValidateNode(pathfinder.End))
                return Pathfinder.State.FailureEndNodeNotValid;

            var hCost = Processor.CalculateHCost(pathfinder.Start, pathfinder.End);
            var heapIndex = S.OpenSet.Insert(new HeapNode<Entity>(pathfinder.Start, hCost, hCost));
            S.Nodes.Add(pathfinder.Start, new PathfindingNode<Entity>(pathfinder.Start, hCost, 0, heapIndex));

            while (S.OpenSet.Count > 0)
            {
                var currentNode = S.OpenSet.Pop();
                var currentNodeData = S.Nodes[currentNode.NodeKey];

                //We have arrived at the destination, reconstruct the path and finish
                if (currentNode.NodeKey == pathfinder.End)
                {
                    ReconstructPath(in pathfinder.Start, in pathfinder.End);
                    return Pathfinder.State.Success;
                }

                var neighbours = SpatialConnectionLookup[currentNode.NodeKey].AsNativeArray();

                for (int i = 0; i < neighbours.Length; i++)
                {
                    var neighbour = neighbours[i].Entity;

                    if (S.ClosedSet.Contains(neighbour))
                        continue;

                    var hasSeenPreviously = S.Nodes.TryGetValue(
                        neighbour,
                        out PathfindingNode<Entity> neighbourNodeData
                    );

                    if (!hasSeenPreviously && !Processor.ValidateNode(neighbour))
                    {
                        S.ClosedSet.Add(neighbour);
                        continue;
                    }

                    hCost = Processor.CalculateHCost(neighbour, pathfinder.End);
                    var gCost = (int)(
                        currentNodeData.GCost
                        + math.distance(Processor.NodePosition(currentNode.NodeKey), Processor.NodePosition(neighbour))
                    );
                    var fCost = gCost + hCost;

                    //We have not seen this node previously, add it to the open set and add it to the node lookup
                    if (!hasSeenPreviously)
                    {
                        heapIndex = S.OpenSet.Insert(new HeapNode<Entity>(neighbour, hCost, fCost));
                        S.Nodes.Add(
                            neighbour,
                            new PathfindingNode<Entity>(currentNode.NodeKey, hCost, gCost, heapIndex)
                        );
                    }
                    //We have seen this node previously, check if the current path is shorter than the path that this node was last visited with
                    else if (gCost < neighbourNodeData.GCost)
                    {
                        S.OpenSet.Remove(neighbourNodeData.HeapIndex);
                        heapIndex = S.OpenSet.Insert(new HeapNode<Entity>(neighbour, hCost, fCost));
                        S.Nodes[neighbour] = new PathfindingNode<Entity>(currentNode.NodeKey, hCost, gCost, heapIndex);
                    }
                }

                S.ClosedSet.Add(currentNode.NodeKey);
            }

            return Pathfinder.State.FailureEmptySet;
        }
    }
}
