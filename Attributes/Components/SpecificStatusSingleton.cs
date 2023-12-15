using Schnozzle.Core.SchnozzleObject;
using Spark.Status.SchnozzleObjects;
using Unity.Entities;

namespace Spark.Status.Components
{
    public struct SpecificStatusSingleton : IComponentData
    {
        public SchnozzleReference<StatSettings> ReachStat;
        public SchnozzleReference<StatSettings> MoveSpeedStat;
        public SchnozzleReference<IntrinsicSettings> RecoveryIntrinsic;
    }
}
