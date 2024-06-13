using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Universal.Common.Runtime;
using Prefabulous.Universal.Shared.Editor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(PrefabulousEditAllMeshBoundsPlugin))]
namespace Prefabulous.Universal.Shared.Editor
{
    public class PrefabulousEditAllMeshBoundsPlugin : Plugin<PrefabulousEditAllMeshBoundsPlugin>
    {
        public override string QualifiedName => "dev.hai-vr.prefabulous.universal.EditAllMeshBounds";
        public override string DisplayName => "Prefabulous Universal - Edit All Mesh Bounds";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .Run("Edit Mesh Bounds", context =>
                {
                    var prefabulousComps = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousEditAllMeshBounds>(true);
                    if (prefabulousComps.Length == 0) return;

                    var my = prefabulousComps.Last();

                    var smrs = context.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var smr in smrs)
                    {
                        smr.localBounds = my.bounds;
                    }
                    
                    PrefabulousUtil.DestroyAllAfterBake<PrefabulousEditAllMeshBounds>(context);
                });
        }
    }
}