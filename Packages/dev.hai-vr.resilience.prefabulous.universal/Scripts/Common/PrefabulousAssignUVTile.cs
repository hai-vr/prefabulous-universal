using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Assign UV Tile")]
    public class PrefabulousAssignUVTile : MonoBehaviour, IPrefabulousEditorOnly
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