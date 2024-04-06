using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
    [AddComponentMenu("Prefabulous/PA Non-Linear Blendshapes")]
    public class PrefabulousNonLinearBlendshapes : MonoBehaviour, IPrefabulousEditorOnly
    {
        public string[] blendShapes;
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;

        public AnimationCurve distribution = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve maxCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve zeroCurve = new AnimationCurve
        {
            keys = new []
            {
                new Keyframe
                {
                    inTangent = 0,
                    outTangent = 0,
                    inWeight = 0,
                    outWeight = 0,
                    time = 0,
                    value = 0,
                    weightedMode = WeightedMode.None
                },
                new Keyframe
                {
                    inTangent = 2,
                    outTangent = 2,
                    inWeight = 0,
                    outWeight = 0,
                    time = 1,
                    value = 1,
                    weightedMode = WeightedMode.None
                }
            }
        };
    }
}