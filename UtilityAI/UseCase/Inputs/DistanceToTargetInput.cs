using Schnozzle.AI.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Schnozzle.UseCase.Inputs
{
    public struct DistanceToTargetInputBlob : IInput<GameContext, BasicAIInputType>
    {
        public InputHeader<BasicAIInputType> Header { get; set; }

        public float MaxRangeBookend;

        public float GetValue(in GameContext context)
        {
            var target = context.Target.Value;

            if (!context.EntityStorage.Exists(target))
                return 1;

            var aspect = context.Lookup[target];

            var myPosition = context.Aspect.LocalToWorld.ValueRO.Position;
            var targetPosition = aspect.LocalToWorld.ValueRO.Position;

            return math.distance(myPosition, targetPosition) / MaxRangeBookend;
        }
    }

    public class DistanceToTargetInput : InputSettings<DistanceToTargetInputBlob, GameContext, BasicAIInputType>
    {
        [SerializeField]
        private float MaxRangeBookend = 10;

        public override void PopulateBlob(ref DistanceToTargetInputBlob blob, ref BlobBuilder builder)
        {
            blob.MaxRangeBookend = MaxRangeBookend;
        }

        public override BasicAIInputType Type => BasicAIInputType.DistanceToTarget;
    }
}
