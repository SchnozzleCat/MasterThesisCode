using System.Linq;
using Newtonsoft.Json;
using Schnozzle.Core.SchnozzleObject;
using Schnozzle.Core.SchnozzleObject.Interfaces;
using Schnozzle.ECS.SchnozzleObject;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Spark.Status.SchnozzleObjects
{
    public struct IntrinsicBlob : ISchnozzleBlob
    {
        public int MaximumValueStatIndex;
        public int RegenerationPerSecondStatIndex;
        public int RegenerationDelayStatIndex;
        public int Index;
        public bool SendEvents;

        public void Dispose() { }
    }

    [CreateAssetMenu(menuName = "Gameplay/Intrinsic")]
    public class IntrinsicSettings : SchnozzleObject<IntrinsicSettings, IntrinsicBlob>, IOrderable
    {
        [ShowInInspector, PropertyOrder(-1)]
        public string Name => name;
        
        [SerializeField, JsonProperty]
        private SchnozzleObjectReference<StatSettings> _maximumValueStat;

        [SerializeField, JsonProperty]
        private SchnozzleObjectReference<StatSettings> _regenerationPerSecondStat;

        [SerializeField, JsonProperty]
        private SchnozzleObjectReference<StatSettings> _regenerationDelayStat;

        [SerializeField, JsonProperty]
        private int _order;

        [SerializeField, JsonProperty]
        private bool _isVisible = true;

        [SerializeField, JsonProperty]
        private bool _sendEvents = false;

        [SerializeField, JsonProperty, ShowIf(nameof(_isVisible))]
        private Color _intrinsicColor = Color.white;

        [SerializeField, JsonProperty, ShowIf(nameof(_isVisible))]
        private Vector2 _baseSizeMultiplier = Vector2.one;

        public int Order => _order;
        public Color Color => _intrinsicColor;
        public Vector2 BaseSizeMultiplier => _baseSizeMultiplier;
        public bool IsVisible => _isVisible;

        public int Index => GetAll().Ordered().ToList().IndexOf(this);

        public override void PopulateBlob(ref IntrinsicBlob blob, ref BlobBuilder builder, World world)
        {
            blob.MaximumValueStatIndex = _maximumValueStat?.TryGet()?.Index ?? -1;
            blob.RegenerationPerSecondStatIndex = _regenerationPerSecondStat?.TryGet()?.Index ?? -1;
            blob.RegenerationDelayStatIndex = _regenerationDelayStat?.TryGet()?.Index ?? -1;
            blob.Index = Index;
            blob.SendEvents = _sendEvents;
        }
    }
}
