﻿using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Recalculate Normals")]
    public class PrefabulousRecalculateNormals : MonoBehaviour, IPrefabulousEditorOnly
    {
        public string[] blendShapes;
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;
        
        public bool eraseCustomSplitNormals;
    }
}