using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Edit All Mesh Anchor Override")]
    [HelpURL("https://docs.hai-vr.dev/redirect/components/PrefabulousEditAllMeshAnchorOverride")]
    public class PrefabulousEditAllMeshAnchorOverride : MonoBehaviour, IPrefabulousEditorOnly
    {
        public Transform anchorOverride;
    }
}
