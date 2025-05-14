using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Universal.Common.Runtime;
using Prefabulous.Universal.Shared.Editor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(PrefabulousEditAllMeshAnchorOverridePlugin))]
namespace Prefabulous.Universal.Shared.Editor
{
#if PREFABULOUS_UNIVERSAL_NDMF_CROSSAPP_INTEGRATION_SUPPORTED
    [RunsOnAllPlatforms]
#endif
    public class PrefabulousEditAllMeshAnchorOverridePlugin : Plugin<PrefabulousEditAllMeshAnchorOverridePlugin>
    {
        public override string QualifiedName => "dev.hai-vr.prefabulous.universal.EditAllMeshAnchorOverride";
        public override string DisplayName => "Prefabulous Universal - Edit All Mesh Anchor Override";
        
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .Run("Edit Mesh Anchor Override", context =>
                {
                    var prefabulousComps = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousEditAllMeshAnchorOverride>(true);
                    if (prefabulousComps.Length == 0) return;

                    var my = prefabulousComps.Last();

                    var smrs = context.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var smr in smrs)
                    {
                        smr.probeAnchor = my.anchorOverride;
                    }

                    var mrs = context.AvatarRootTransform.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var mr in mrs)
                    {
                        mr.probeAnchor = my.anchorOverride;
                    }
                    
                    PrefabulousUtil.DestroyAllAfterBake<PrefabulousEditAllMeshAnchorOverride>(context);
                });
        }
    }
}