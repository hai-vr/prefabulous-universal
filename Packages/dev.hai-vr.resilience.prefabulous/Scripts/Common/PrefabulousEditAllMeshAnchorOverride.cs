using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.VRC.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA Edit All Mesh Anchor Override")]
    public class PrefabulousEditAllMeshAnchorOverride : MonoBehaviour, IEditorOnly
    {
        public Transform anchorOverride;
    }
}
