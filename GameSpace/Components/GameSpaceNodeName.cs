#if UNITY_EDITOR
using Unity.Collections;
using Unity.Entities;

namespace Schnozzle.GameSpace.Components
{
    public struct GameSpaceNodeName : IComponentData
    {
        public FixedString32Bytes Value;
    }
}
#endif
