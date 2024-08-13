using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Edit All Mesh Bounds")]
    public class PrefabulousEditAllMeshBounds : MonoBehaviour, IPrefabulousEditorOnly
    {
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2);
    }
}
