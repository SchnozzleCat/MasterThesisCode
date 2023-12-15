using Schnozzle.Core.SchnozzleObject;
using Spark.Status.Components;
using Spark.Status.SchnozzleObjects;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.Authoring
{
    public class SpecificStatSingletonAuthoring : MonoBehaviour
    {
        public SchnozzleObjectReference<StatSettings> ReachStat;
        public SchnozzleObjectReference<StatSettings> MoveSpeedStat;
        public SchnozzleObjectReference<IntrinsicSettings> RecoveryIntrinsic;

        public class Baker : Baker<SpecificStatSingletonAuthoring>
        {
            public override void Bake(SpecificStatSingletonAuthoring authoring)
            {
                AddComponent(
                    new SpecificStatusSingleton
                    {
                        ReachStat = authoring.ReachStat.AsRef,
                        RecoveryIntrinsic = authoring.RecoveryIntrinsic.AsRef,
                        MoveSpeedStat = authoring.MoveSpeedStat.AsRef
                    }
                );
            }
        }
    }
}
