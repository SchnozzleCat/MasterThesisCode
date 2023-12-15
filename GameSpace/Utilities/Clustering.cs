using Schnozzle.GameSpace.Aspects;
using Schnozzle.GameSpace.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Schnozzle.GameSpace.Utilities
{
    public static class Clustering
    {
        public struct ClusterEntry
        {
            public Entity Entity;
            public bool Collapsed;
        }

        public struct ClusterMetaData
        {
            public bool HasCollapsedEntities;
            public float3 AveragePosition;
            public uint EntityCount;
            public Entity CollapsedParent;
        }

        /// <summary>
        /// Grid clustering method to quickly but coarsely identify approximate neighbours.
        /// The max distance parameter is used to decide the granularity of the resulting buckets.
        /// </summary>
        /// <param name="entities">The entities to cluster.</param>
        /// <param name="transformLookup">The transform lookup.</param>
        /// <param name="maxDistance">The maximum distance two entities can be apart.</param>
        public static (
            NativeParallelMultiHashMap<uint, ClusterEntry> ClusterSet,
            NativeHashMap<uint, ClusterMetaData> MetaData
        ) GridClustering(
            in NativeArray<Entity> entities,
            in AtomicAgentAspect.Lookup atomicAgentLookup,
            in ComponentLookup<CollapsedSpace> collapsedSpaceLookup,
            float maxDistance
        )
        {
            var clusterSet = new NativeParallelMultiHashMap<uint, ClusterEntry>(entities.Length / 2, Allocator.Temp);
            var clusterMetaData = new NativeHashMap<uint, ClusterMetaData>(entities.Length / 2, Allocator.Temp);

            var count = entities.Length;

            for (int i = 0; i < count; i++)
            {
                // Add Cluster entry.
                var entity = entities[i];
                var atomicAgent = atomicAgentLookup[entity];
                var position = atomicAgent.LocalTransform.ValueRO.Position;
                var parent = atomicAgent.GameSpaceParent.ValueRO.Entity;
                var quantizedPosition = math.floor(position / maxDistance);
                var hash = math.hash(quantizedPosition);
                var isCollapsed = collapsedSpaceLookup.HasComponent(parent);
                clusterSet.Add(hash, new ClusterEntry { Entity = entity, Collapsed = isCollapsed });

                // Write Metadata.
                var hasValue = clusterMetaData.TryGetValue(hash, out var meta);
                meta.EntityCount++;
                meta.AveragePosition += position;
                meta.HasCollapsedEntities |= isCollapsed;
                if (isCollapsed)
                    meta.CollapsedParent = parent;
                if (hasValue)
                    clusterMetaData[hash] = meta;
                else
                    clusterMetaData.Add(hash, meta);
            }

            foreach (var key in clusterMetaData.GetKeyArray(Allocator.Temp))
            {
                var value = clusterMetaData[key];
                value.AveragePosition /= value.EntityCount;
                clusterMetaData[key] = value;
            }

            return (clusterSet, clusterMetaData);
        }
    }
}
