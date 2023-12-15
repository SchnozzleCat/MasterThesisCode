using Schnozzle.Core.SchnozzleObject;
using Sirenix.OdinInspector;
using Spark.Status.Data;
using Spark.Status.SchnozzleObjects;

namespace Spark.Status.Authoring
{
    [InlineProperty, HideReferenceObjectPicker]
    public class IntrinsicCostContainer
    {
        public SchnozzleObjectReference<IntrinsicSettings> Intrinsic = new();
        public float Value;

        public IntrinsicCost AsCost => new() { IntrinsicIndex = Intrinsic.Get().Index, Value = Value };
    }
}
