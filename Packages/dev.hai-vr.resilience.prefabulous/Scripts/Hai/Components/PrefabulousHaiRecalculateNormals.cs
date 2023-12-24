using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.Hai.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA-H Recalculate Normals")]
    public class PrefabulousHaiRecalculateNormals : MonoBehaviour, IEditorOnly
    {
        public string[] blendShapes;
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;
        
        public bool eraseCustomSplitNormals;
    }
}