using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.VRC.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA Edit All Mesh Bounds")]
    public class PrefabulousEditAllMeshBounds : MonoBehaviour, IEditorOnly
    {
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2);
    }
}
