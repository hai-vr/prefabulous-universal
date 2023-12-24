using System;
using System.Linq;
using UnityEngine;

namespace Prefabulous.VRC.Editor
{
    public static class PrefabulousUtil
    {
        public static string[] GetAllBlendshapeNames(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null) return Array.Empty<string>();
            
            var sharedMesh = smr.sharedMesh;

            return Enumerable.Range(0, sharedMesh.blendShapeCount)
                .Select(i => sharedMesh.GetBlendShapeName(i))
                .ToArray();
        }
    }
}