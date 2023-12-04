using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.VRC.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA Replace Action Animator")]
    public class PrefabulousReplaceActionAnimator : MonoBehaviour, IEditorOnly
    {
        public RuntimeAnimatorController controller;
    }
}