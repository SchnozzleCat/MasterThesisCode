using Schnozzle.Core.SchnozzleObject;
using Sirenix.OdinInspector;
using Spark.Status.Components.Stats;
using Spark.Status.SchnozzleObjects;

namespace Spark.Status.Authoring
{
    [InlineProperty, HideReferenceObjectPicker]
    public class BaseStatContainer
    {
        public SchnozzleObjectReference<StatSettings> Stat = new();
        public float Value;

        public BaseStat ToBaseStat => new() { Stat = Stat.AsRef, Value = Value };
    }
}
