using System;
using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Replace Textures")]
    public class PrefabulousReplaceTextures : MonoBehaviour, IPrefabulousEditorOnly
    {
        public bool executeInPlayMode = false;
        public PrefabulousTextureSubstitution[] replacements;
    }

    [Serializable]
    public struct PrefabulousTextureSubstitution
    {
        public Texture2D source;
        public Texture2D target;
    }
}