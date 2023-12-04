using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.VRC.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA Edit Mesh Bounds")]
    public class PrefabulousEditMeshBounds : MonoBehaviour, IEditorOnly
    {
        public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2);
    }
}
