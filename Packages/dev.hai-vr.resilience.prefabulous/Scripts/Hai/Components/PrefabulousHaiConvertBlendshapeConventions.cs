using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDKBase;

namespace Prefabulous.Hai.Runtime
{
    [AddComponentMenu("Prefabulous Avatar/PA-H Convert Blendshape Conventions")]
    public class PrefabulousHaiConvertBlendshapeConventions : MonoBehaviour, IEditorOnly
    {
        public bool limitToSpecificMeshes;
        public SkinnedMeshRenderer[] renderers;

        [TextArea]
        public string keyValueMapping;
        public string keyValueSeparator = "=";
        public bool reverse;

        public Dictionary<string, string> ParseMapping()
        {
            var store = keyValueMapping ?? "";

            var mapping = new Dictionary<string, string>();
            var lines = store.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var keyValue in lines)
            {
                var equalSplit = keyValue.Split(keyValueSeparator);
                if (equalSplit.Length == 2)
                {
                    var key = equalSplit[reverse ? 1 : 0];
                    var value = equalSplit[reverse ? 0 : 1];
                    mapping[key] = value;
                }
            }

            return mapping;
        }
    }
}