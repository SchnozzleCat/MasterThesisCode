using System.Collections;
using System.Collections.Generic;
using Schnozzle.GameSpace.Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Schnozzle.GameSpace.Aspects
{
    public readonly partial struct AtomicAgentAspect : IAspect
    {
        public readonly RefRO<LocalTransform> LocalTransform;
        public readonly RefRO<GameSpaceParent> GameSpaceParent;
        public readonly RefRO<SuperAgentParent> SuperAgentParent;
    }
}
