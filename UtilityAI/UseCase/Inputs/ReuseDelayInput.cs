using Schnozzle.AI.Data;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.UseCase.Inputs
{
    public struct ReuseDelayInputBlob : IInput<GameContext, BasicAIInputType>
    {
        public InputHeader<BasicAIInputType> Header { get; set; }

        public float Bookend;

        public float GetValue(in GameContext context)
        {
            return context.Aspect.CombatData.ValueRO.ReuseDelay / Bookend;
        }
    }

    public class ReuseDelayInput : InputSettings<ReuseDelayInputBlob, GameContext, BasicAIInputType>
    {
        [SerializeField]
        private float _bookend;

        public override BasicAIInputType Type => BasicAIInputType.ReuseDelay;

        public override void PopulateBlob(ref ReuseDelayInputBlob blob, ref BlobBuilder builder)
        {
            blob.Bookend = _bookend;
        }
    }
}
