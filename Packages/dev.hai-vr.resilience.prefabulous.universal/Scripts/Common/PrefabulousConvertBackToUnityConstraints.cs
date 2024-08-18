using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Convert back to Unity Constraints")]
    public class PrefabulousConvertBackToUnityConstraints : MonoBehaviour, IPrefabulousEditorOnly
    {
    }
}