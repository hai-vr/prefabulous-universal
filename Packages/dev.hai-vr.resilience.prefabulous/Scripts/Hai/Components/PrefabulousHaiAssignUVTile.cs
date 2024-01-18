using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.Hai.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA-H Assign UV Tile")]
    public class PrefabulousHaiAssignUVTile : MonoBehaviour, IEditorOnly
    {
        public AssignMode mode;
        
        // BlendShape method
        public string[] blendShapes;
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;
        
        public bool keepPartialPolygons;
        
        // Mesh method
        public Renderer[] entireMeshes;
        
        // Common
        public UVChannel uvChannel = UVChannel.UV1;
        public int u;
        public int v;
        
        public ExistingData existingData;
        
        public enum UVChannel {
            UV0, UV1, UV2, UV3
        }
        
        public enum ExistingData {
            DoNotClear,
            SetToMinusOne,
            SetToZero,
            Shift,
        }
        
        public enum AssignMode {
            BlendShapes,
            EntireMesh
        }
    }
}