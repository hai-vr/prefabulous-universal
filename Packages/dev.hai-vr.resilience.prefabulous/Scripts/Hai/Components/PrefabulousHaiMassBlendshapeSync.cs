using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.Hai.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA-H Mass Blendshape Sync")]
    public class PrefabulousHaiMassBlendshapeSync : MonoBehaviour, IEditorOnly
    {
        public SkinnedMeshRenderer source;
        public SkinnedMeshRenderer[] targets;
    }
}