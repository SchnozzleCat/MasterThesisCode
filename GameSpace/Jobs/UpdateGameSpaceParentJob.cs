using Schnozzle.GameSpace.Authoring;
using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Utilities;
using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;

namespace Schnozzle.GameSpace.Jobs
{
    [BurstCompile]
    [WithChangeFilter(typeof(GameSpaceParent), typeof(PreviousGameSpaceParent))]
    public partial struct UpdateGameSpaceParentJob : IJobEntity
    {
        public BufferLookup<GameSpaceChild> ChildLookup;

        public void Execute(Entity entity, in GameSpaceParent parent, ref PreviousGameSpaceParent previous)
        {
            if (parent.Entity == previous.Entity)
                return;

            // TODO: Replace this with with a hashset remove instead of list iteration.
            if (ChildLookup.TryGetBuffer(previous.Entity, out var previousChildren))
                Assert.IsTrue(previousChildren.Remove(new GameSpaceChild { Entity = entity }));

            if (ChildLookup.TryGetBuffer(parent.Entity, out var newChildren))
                newChildren.Add(new GameSpaceChild { Entity = entity });

            previous.Entity = parent.Entity;
        }
    }
}
