using Schnozzle.AI.Data;
using Unity.Entities;
using UnityEngine;

namespace Schnozzle.UseCase.Inputs
{
    public struct MyHealthInputBlob : IInput<GameContext, BasicAIInputType>
    {
        public InputHeader<BasicAIInputType> Header { get; set; }

        public float GetValue(in GameContext context)
        {
            float currentHealth = context.Aspect.Health.ValueRO.Value;
            float maxHealth = context.Aspect.MaxHealth.ValueRO.Value;

            return currentHealth / maxHealth;
        }
    }

    public class MyHealthInput : InputSettings<MyHealthInputBlob, GameContext, BasicAIInputType>
    {
        public override void PopulateBlob(ref MyHealthInputBlob blob, ref BlobBuilder builder) { }

        public override BasicAIInputType Type => BasicAIInputType.MyHealth;
    }
}
