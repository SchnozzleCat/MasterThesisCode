using Schnozzle.AI.Data;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.UseCase.Inputs
{
    public struct AmmoCountInputBlob : IInput<GameContext, BasicAIInputType>
    {
        public InputHeader<BasicAIInputType> Header { get; set; }

        public float Bookend;

        public float GetValue(in GameContext context)
        {
            return context.Aspect.CombatData.ValueRO.Ammo / 10f;
        }
    }

    public class AmmoCountInput : InputSettings<AmmoCountInputBlob, GameContext, BasicAIInputType>
    {
        [SerializeField]
        private float _bookend;

        public override BasicAIInputType Type => BasicAIInputType.AmmoCount;

        public override void PopulateBlob(ref AmmoCountInputBlob blob, ref BlobBuilder builder)
        {
            blob.Bookend = _bookend;
        }
    }
}
