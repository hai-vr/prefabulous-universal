using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Native.Shared.Runtime;
#endif

namespace Prefabulous.Native.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Edit All Mesh Bounds")]
    public class PrefabulousEditAllMeshBounds : MonoBehaviour, IPrefabulousEditorOnly
    {
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2);
    }
}
