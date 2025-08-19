using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Recalculate Normals")]
    [HelpURL("https://docs.hai-vr.dev/redirect/components/PrefabulousRecalculateNormals")]
    public class PrefabulousRecalculateNormals : MonoBehaviour, IPrefabulousEditorOnly
    {
        public string[] blendShapes;
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;
        
        public bool eraseCustomSplitNormals;
    }
}