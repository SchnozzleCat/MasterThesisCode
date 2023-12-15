using Schnozzle.AI.Data;
using Unity.Entities;
using Unity.Mathematics;

namespace Schnozzle.UseCase.Inputs
{
    public struct IsTargetSelfInputBlob : IInput<GameContext, BasicAIInputType>
    {
        public InputHeader<BasicAIInputType> Header { get; set; }

        public float GetValue(in GameContext context) =>
            math.select(0, 1, context.Aspect.Entity == context.Target.Value);
    }

    public class IsTargetSelfInput : InputSettings<IsTargetSelfInputBlob, GameContext, BasicAIInputType>
    {
        public override void PopulateBlob(ref IsTargetSelfInputBlob blob, ref BlobBuilder builder) { }

        public override BasicAIInputType Type => BasicAIInputType.IsTargetSelf;
    }
}
