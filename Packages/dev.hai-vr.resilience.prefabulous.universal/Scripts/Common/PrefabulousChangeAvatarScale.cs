using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Change Avatar Scale")]
    public class PrefabulousChangeAvatarScale : MonoBehaviour, IPrefabulousEditorOnly
    {
        public bool customSourceSize;
        public float sourceSizeInMeters = 1f;
        public float desiredSizeInMeters = 1f;
    }
}
