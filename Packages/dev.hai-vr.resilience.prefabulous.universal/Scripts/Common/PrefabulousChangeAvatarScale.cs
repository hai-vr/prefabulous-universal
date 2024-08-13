using UnityEngine;
#if PREFABULOUS_UNIVERSAL_VRCHAT_IS_INSTALLED
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
