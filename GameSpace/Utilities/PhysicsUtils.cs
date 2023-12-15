using Schnozzle.GameSpace.Components;
using Unity.Assertions;
using Unity.Entities;
using Unity.Physics;

namespace Schnozzle.GameSpace.Utilities
{
    /// <summary>
    /// Custom distance hit collector to find the node with the deepest depth in the query.
    /// </summary>
    public struct GameSpaceNodeCollector : ICollector<DistanceHit>
    {
        public GameSpaceNodeCollector(ref ComponentLookup<GameSpaceNode> gameSpaceNodeLookup)
        {
            MaxFraction = 0.0001f;
            GameSpaceNodeLookup = gameSpaceNodeLookup;
            NumHits = 0;
            BestDepth = 0;
            BestEntity = Entity.Null;
        }

        public readonly ComponentLookup<GameSpaceNode> GameSpaceNodeLookup;

        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        public ushort BestDepth { get; private set; }
        public Entity BestEntity { get; private set; }

        public bool AddHit(DistanceHit hit)
        {
            Assert.IsTrue(hit.Fraction <= MaxFraction);

            if (!GameSpaceNodeLookup.TryGetComponent(hit.Entity, out var nodeData))
                return true;

            if (nodeData.Depth >= BestDepth)
            {
                BestDepth = nodeData.Depth;
                BestEntity = hit.Entity;
                NumHits = 1;
            }

            return true;
        }
    }
}
