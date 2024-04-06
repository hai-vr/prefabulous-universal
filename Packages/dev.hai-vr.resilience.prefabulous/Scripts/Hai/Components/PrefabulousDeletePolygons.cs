using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Native.Shared.Runtime;
#endif

namespace Prefabulous.Native.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Delete Polygons")]
    public class PrefabulousDeletePolygons : MonoBehaviour, IPrefabulousEditorOnly
    {
        public string[] blendShapes;
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;
        
        public bool keepPartialPolygons;
    }
}