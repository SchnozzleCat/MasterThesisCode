using Schnozzle.ECS.Extensions;
using Spark.Status.Components;
using Unity.Burst;
using Unity.Entities;

namespace Spark.Status.Jobs.Intrinsics
{
    [BurstCompile]
    internal partial struct UpdateIntrinsicModificationEventsJob : IJobEntity
    {
        public int TickRate;

        public uint Tick;

        public void Execute(ref DynamicBuffer<ModifyIntrinsicEvent> statusEvents)
        {
            for (int i = 0; i < statusEvents.Length; i++)
            {
                ref var element = ref statusEvents.GetAsRef(i);

                //We have not seen this event before, apply status modification.
                if (!element.HasInitialized)
                {
                    element.Tick = Tick;
                }

                //If the element is older than 1 second, remove it.
                if (Tick - element.Tick > TickRate)
                {
                    statusEvents.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }
    }
}
