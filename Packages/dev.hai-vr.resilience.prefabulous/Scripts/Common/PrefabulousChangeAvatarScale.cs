using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Native.Shared.Runtime;
#endif

namespace Prefabulous.Native.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Change Avatar Scale")]
    public class PrefabulousChangeAvatarScale : MonoBehaviour, IPrefabulousEditorOnly
    {
        public bool customSourceSize;
        public float sourceSizeInMeters = 1f;
        public float desiredSizeInMeters = 1f;
    }
}
