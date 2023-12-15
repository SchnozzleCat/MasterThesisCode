using Schnozzle.GameSpace.Components;
using Schnozzle.GameSpace.Utilities;
using Unity.Assertions;
using Unity.Entities;

namespace Schnozzle.GameSpace.Jobs
{
    [WithNone(typeof(GameSpaceParent))]
    public partial struct CleanupGameSpaceParentJob : IJobEntity
    {
        public BufferLookup<GameSpaceChild> ChildLookup;

        public void Execute(Entity entity, in PreviousGameSpaceParent previous)
        {
            if (ChildLookup.TryGetBuffer(previous.Entity, out var previousChildren))
                Assert.IsTrue(previousChildren.Remove(new GameSpaceChild { Entity = entity }));
        }
    }
}
