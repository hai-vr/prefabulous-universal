using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Edit All Mesh Anchor Override")]
    public class PrefabulousEditAllMeshAnchorOverride : MonoBehaviour, IPrefabulousEditorOnly
    {
        public Transform anchorOverride;
    }
}
