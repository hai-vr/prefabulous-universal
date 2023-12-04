using System;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using Prefabulous.VRC.Editor;
using Prefabulous.VRC.Runtime;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(PrefabulousEditMeshBoundsPlugin))]
namespace Prefabulous.VRC.Editor
{
    public class PrefabulousEditMeshBoundsPlugin : Plugin<PrefabulousEditMeshBoundsPlugin>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .Run("Edit Mesh Bounds", context =>
                {
                    var prefabulousComps = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousEditMeshBounds>(true);
                    if (prefabulousComps.Length == 0) return;

                    var my = prefabulousComps.Last();

                    var smrs = context.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var smr in smrs)
                    {
                        smr.localBounds = my.bounds;
                    }
                });
        }
    }
}