using System;
using UnityEngine;
#if VRC_SDK_VRCSDK3
using IPrefabulousEditorOnly = VRC.SDKBase.IEditorOnly;
#else
using Prefabulous.Universal.Shared.Runtime;
#endif

namespace Prefabulous.Universal.Common.Runtime
{
#if PREFABULOUS_INTERNAL
    [AddComponentMenu("Prefabulous/PA Delete Objects At Late Stage")]
#else
    [AddComponentMenu("")]
#endif
    public class PrefabulousDeleteObjectsAtLateStage : MonoBehaviour, IPrefabulousEditorOnly
    {
        public GameObject[] objects = Array.Empty<GameObject>();
        public bool deleteThisObject = true;
    }
}