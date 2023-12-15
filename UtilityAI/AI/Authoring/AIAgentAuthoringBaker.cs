using System;
using System.Collections.Generic;
using Schnozzle.AI.Components;
using Schnozzle.AI.Data;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Schnozzle.AI.Authoring
{
    public abstract class AIAgentAuthoring<TExecutionContext, TEnum, TCustomData> : SerializedMonoBehaviour
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool _debug;

        public bool Debug => _debug;
#endif

        [SerializeField]
        private List<DecisionSetSettings<TExecutionContext, TEnum, TCustomData>> _decisionSets = new();

        public IReadOnlyList<DecisionSetSettings<TExecutionContext, TEnum, TCustomData>> Sets => _decisionSets;
    }

    public abstract class AIAgentAuthoringBaker<TAuthoringType, TExecutionContext, TEnum, TCustomData> : Baker<TAuthoringType>
        where TAuthoringType : AIAgentAuthoring<TExecutionContext, TEnum, TCustomData>
        where TExecutionContext : unmanaged, IExecutionContext<TEnum>
        where TEnum : unmanaged, Enum
        where TCustomData : unmanaged
    {
        public override void Bake(TAuthoringType authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent<ActiveDecision<TExecutionContext, TEnum, TCustomData>>(entity);
            AddComponent<AITargetBuffer>(entity);
            // #if UNITY_EDITOR
            //             if (authoring.Debug)
            //                 AddBuffer<RuntimeDebuggingBuffer<TExecutionContext, TEnum, TCustomData>>(entity);
            // #endif
            var setBuffer = AddBuffer<DecisionSetBuffer<TExecutionContext, TEnum, TCustomData>>(entity);

            foreach (var set in authoring.Sets)
            {
                DependsOn(set);

                if (set == null)
                    continue;

                var hash = new Hash128(set.Guid);

                if (
                    !TryGetBlobAssetReference(hash, out BlobAssetReference<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>> blobAssetReference)
                )
                {
                    var blobBuilder = new BlobBuilder(Allocator.Temp);

                    ref var root = ref blobBuilder.ConstructRoot<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>>();

                    set.PopulateBlob(ref root, ref blobBuilder);

                    blobAssetReference = blobBuilder.CreateBlobAssetReference<DecisionSetBlob<TExecutionContext, TEnum, TCustomData>>(
                        Allocator.Persistent
                    );

                    AddBlobAssetWithCustomHash(ref blobAssetReference, hash);

                    Debug.Log("Built blob.");
                }

                setBuffer.Add(new() { Value = blobAssetReference });
            }
        }
    }
}
