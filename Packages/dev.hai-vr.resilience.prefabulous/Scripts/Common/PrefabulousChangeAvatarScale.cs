using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.VRC.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA Change Avatar Scale")]
    public class PrefabulousChangeAvatarScale : MonoBehaviour, IEditorOnly
    {
        public bool customSourceSize;
        public float sourceSizeInMeters = 1f;
        public float desiredSizeInMeters = 1f;
    }
}
