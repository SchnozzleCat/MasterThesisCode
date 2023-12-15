using Newtonsoft.Json;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.ECS.SchnozzleObject;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.SchnozzleObjects
{
    public struct DamageTypeBlob : ISchnozzleBlob
    {
        public int DamageIncreasingStatIndex;
        public int DamageReducingStatIndex;

        public void Dispose() { }
    }

    [CreateAssetMenu(menuName = "Gameplay/Damage Type")]
    public class DamageTypeSettings : SchnozzleObject<DamageTypeSettings, DamageTypeBlob>
    {
        [SerializeField, JsonProperty]
        private SchnozzleObjectReference<StatSettings> _damageIncreasingStat = new();

        [SerializeField, JsonProperty]
        private SchnozzleObjectReference<StatSettings> _damageReducingStat = new();

        [SerializeField, JsonProperty]
        private Color _floatingTextColor = Color.white;

        [SerializeField, JsonProperty]
        private string _floatingTextSpriteName;

        public Color FloatingTextColor => _floatingTextColor;
        public string FloatingTextSpriteName => _floatingTextSpriteName;

        public override void PopulateBlob(ref DamageTypeBlob blob, ref BlobBuilder builder, World world)
        {
            blob.DamageIncreasingStatIndex = _damageIncreasingStat?.TryGet()?.Index ?? -1;
            blob.DamageReducingStatIndex = _damageReducingStat?.TryGet()?.Index ?? -1;
        }
    }
}
