using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.Hai.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA-H Lock Locomotion Menu Item")]
    public class PrefabulousHaiLockLocomotionMenuItem : MonoBehaviour, IEditorOnly
    {
        public Texture2D icon;

        private void OnDestroy()
        {
            if (Application.isPlaying) return;
            
            var menu = GetComponent<ModularAvatarMenuItem>();
            if (menu != null && menu.hideFlags == HideFlags.NotEditable)
            {
                menu.hideFlags = HideFlags.None;
                DestroyImmediate(menu);
            }
        }
    }
}
