using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA HaiXT Generate Blendshapes for Face Tracking Extensions")]
    public class PrefabulousGenerateBlendshapesFTE : MonoBehaviour, IPrefabulousEditorOnly
    {
        public string EyeClosedInverse_Smile_EyeLeft;
        public string EyeClosedInverse_Smile_EyeRight;
    }
}