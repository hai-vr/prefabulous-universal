using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Universal.Common.Runtime;
using Prefabulous.Universal.Shared.Editor;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(PrefabulousReplaceTexturesPlugin))]
namespace Prefabulous.Universal.Shared.Editor
{
#if PREFABULOUS_UNIVERSAL_NDMF_CROSSAPP_INTEGRATION_SUPPORTED
    [RunsOnAllPlatforms]
#endif
    public class PrefabulousReplaceTexturesPlugin : Plugin<PrefabulousReplaceTexturesPlugin>
    {
        public override string QualifiedName => "dev.hai-vr.prefabulous.universal.ReplaceTextures";
        public override string DisplayName => "Prefabulous Universal - Replace Textures";
        
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .Run("Replace Textures", ReplaceTextures);
        }

        private static void ReplaceTextures(BuildContext ctx)
        {
            var comps = ctx.AvatarRootTransform.GetComponentsInChildren<PrefabulousReplaceTextures>(true);
            if (comps.Length == 0) return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                comps = comps.Where(textures => textures.executeInPlayMode).ToArray();
            }

            if (comps.Length == 0)
            {
                PrefabulousUtil.DestroyAllAfterBake<PrefabulousReplaceTextures>(ctx);
                return;
            }

            var substitutions = new Dictionary<Texture, Texture>();
            foreach (var comp in comps)
            {
                if (comp.replacements == null) continue;
                foreach (var replacement in comp.replacements)
                {
                    if (replacement.source == null || replacement.target == null) continue;

                    substitutions[replacement.source] = replacement.target;
                }
            }

            var visited = new HashSet<Material>();
            var materialNeedsChanging = new Dictionary<Material, Material>();
            
            var smrs = ctx.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                if (VisitMaterials(smr.sharedMaterials, substitutions, visited, materialNeedsChanging))
                {
                    Debug.Log($"Changing {smr.name}");
                    smr.sharedMaterials = MakeMaterialReplacements(smr.sharedMaterials, materialNeedsChanging);
                }
            }
            
            var mrs = ctx.AvatarRootTransform.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in mrs)
            {
                if (VisitMaterials(mr.sharedMaterials, substitutions, visited, materialNeedsChanging))
                {
                    mr.sharedMaterials = MakeMaterialReplacements(mr.sharedMaterials, materialNeedsChanging);
                }
            }
            
            var trs = ctx.AvatarRootTransform.GetComponentsInChildren<TrailRenderer>(true);
            foreach (var tr in trs)
            {
                if (VisitMaterials(tr.sharedMaterials, substitutions, visited, materialNeedsChanging))
                {
                    tr.sharedMaterials = MakeMaterialReplacements(tr.sharedMaterials, materialNeedsChanging);
                }
            }
            
            var psrs = ctx.AvatarRootTransform.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var psr in psrs)
            {
                if (VisitMaterials(psr.sharedMaterials, substitutions, visited, materialNeedsChanging))
                {
                    psr.sharedMaterials = MakeMaterialReplacements(psr.sharedMaterials, materialNeedsChanging);
                }
                if (VisitMaterial(psr.trailMaterial, substitutions, visited, materialNeedsChanging))
                {
                    psr.trailMaterial = psr.trailMaterial != null && materialNeedsChanging.TryGetValue(psr.trailMaterial, out var replacement) ? replacement : psr.trailMaterial;
                }
            }
            
            PrefabulousUtil.DestroyAllAfterBake<PrefabulousReplaceTextures>(ctx);
        }

        private static Material[] MakeMaterialReplacements(Material[] materials, Dictionary<Material, Material> materialNeedsChanging)
        {
            return materials
                .Select(original => original != null && materialNeedsChanging.TryGetValue(original, out var replacement) ? replacement : original)
                .ToArray();
        }

        private static bool VisitMaterials(Material[] materials, Dictionary<Texture, Texture> substitutions, HashSet<Material> visited, Dictionary<Material, Material> materialNeedsChanging)
        {
            var requiresChanges = false;
            foreach (var original in materials)
            {
                var visitMaterial = VisitMaterial(original, substitutions, visited, materialNeedsChanging);
                requiresChanges = requiresChanges || visitMaterial;
            }

            return requiresChanges;
        }

        private static bool VisitMaterial(Material original, Dictionary<Texture, Texture> substitutions, HashSet<Material> visited, Dictionary<Material, Material> materialNeedsChanging)
        {
            if (original == null) return false;
            if (visited.Contains(original)) return materialNeedsChanging.ContainsKey(original);

            visited.Add(original);
            
            Material copy = null;
            var ids = original.GetTexturePropertyNameIDs();
            foreach (var id in ids)
            {
                var texture = original.GetTexture(id);
                if (texture != null && substitutions.TryGetValue(texture, out var substitution))
                {
                    if (copy == null)
                    {
                        copy = Object.Instantiate(original);
                        copy.name = $"(NOT ORIGINAL) Generated by Prefabulous Replace Textures - {original.name}";
                    }
                    copy.SetTexture(id, substitution);
                }
            }

            if (copy != null)
            {
                materialNeedsChanging[original] = copy;
                return true;
            }

            return false;
        }
    }
}