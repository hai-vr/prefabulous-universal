using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.VRC.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA Edit Mesh Anchor Override")]
    public class PrefabulousEditMeshAnchorOverride : MonoBehaviour, IEditorOnly
    {
        public Transform anchorOverride;
    }
}
