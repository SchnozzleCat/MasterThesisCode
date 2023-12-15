using System;
using System.Collections.Generic;
using Schnozzle.GameSpace.Components;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Schnozzle.GameSpace.Authoring
{
    /// <summary>
    ///     Authoring component for GameSpace Nodes.
    /// </summary>
    public class GameSpaceNodeAuthoring : MonoBehaviour
    {
        /// <summary>
        ///     The parent of the game space node.
        /// </summary>
        [SerializeField]
        private GameSpaceNodeAuthoring _parent;

        public GameSpaceNodeAuthoring Parent
        {
            get => _parent;
            set => _parent = value;
        }

        /// <summary>
        ///     The spatial connections this game space node has to other node.
        /// </summary>
        [SerializeField]
        private List<SpatialConnectionAuthoring> _connections = new();

        [Serializable]
        private struct SpatialConnectionAuthoring
        {
            public GameSpaceNodeAuthoring Node;
            public float Cost;
        }

        /// <summary>
        ///     Baked parent data, to avoid deep transform trees.
        ///     TODO: Figure out a better was to do this, as it currently makes it impossible to have moving gamespaces, which is a
        ///     requirement for e.g. a car or a space ship. Maybe make it optional to bake using parents below this hierarchy?
        /// </summary>
        public struct BakedParentData : IComponentData
        {
            public Entity Parent;
        }

        public class AuthoringBaker : Baker<GameSpaceNodeAuthoring>
        {
            private static ushort GetDepth(GameSpaceNodeAuthoring node)
            {
                ushort depth = 0;
                var parent = node._parent;

                var safeCounter = 0;

                while (parent != null)
                {
                    if (parent == node)
                        throw new Exception("Node cannot have itself as parent!");

                    depth++;
                    parent = parent._parent;
                    safeCounter++;
                    if (safeCounter > 100)
                    {
                        node._parent = null;
                        throw new Exception("Circular dependency in parent references detected.");
                    }
                }

                return depth;
            }

            public override void Bake(GameSpaceNodeAuthoring authoring)
            {
                var flags = TransformUsageFlags.WorldSpace;
                
                var entity = GetEntity(authoring, flags);

#if UNITY_EDITOR
                AddComponent(entity, new GameSpaceNodeName { Value = authoring.name });
                var iconContent = EditorGUIUtility.IconContent("tranp");
                EditorGUIUtility.SetIconForObject(authoring.gameObject, (Texture2D)iconContent.image);
#endif

                var bakedParent = new BakedParentData();

                var parent = authoring._parent;
                if (parent != null)
                    bakedParent.Parent = GetEntity(parent, flags);
                AddComponent(entity, bakedParent);

                AddComponent(entity, new GameSpaceNode { Depth = GetDepth(authoring) });
                AddBuffer<GameSpaceChild>(entity);

                var connections = AddBuffer<SpatialConnection>(entity);
                foreach (var connection in authoring._connections)
                {
                    var connectedEntity = GetEntity(connection.Node, flags);
                    if (connectedEntity == Entity.Null)
                        continue;

                    connections.Add(new SpatialConnection { Entity = connectedEntity, Cost = connection.Cost});
                }
            }
        }
    }
}
