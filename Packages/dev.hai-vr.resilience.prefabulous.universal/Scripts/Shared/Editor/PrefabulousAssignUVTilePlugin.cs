using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Universal.Common.Runtime;
using Prefabulous.Universal.Shared.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(PrefabulousAssignUVTilePlugin))]
namespace Prefabulous.Universal.Shared.Editor
{
#if PREFABULOUS_UNIVERSAL_NDMF_CROSSAPP_INTEGRATION_SUPPORTED
    [RunsOnAllPlatforms]
#endif
    public class PrefabulousAssignUVTilePlugin : Plugin<PrefabulousAssignUVTilePlugin>
    {
        public override string QualifiedName => "dev.hai-vr.prefabulous.universal.AssignUVTile";
        public override string DisplayName => "Prefabulous Universal - Assign UV Tile";
        
        private const float Offset = 0.5f;

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming)
                .AfterPlugin<PrefabulousDeletePolygonsPlugin>()
                .BeforePlugin("com.anatawa12.avatar-optimizer");

            seq.Run("Assign UV Tiles", AssignUVTIles);
        }

        private void AssignUVTIles(BuildContext context)
        {
            var assignUVTiles = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousAssignUVTile>(true);
            if (assignUVTiles.Length == 0) return;

            // Order matters: Process all components of type "EntireMesh", in case additional "BlendShapes" components will further split that assigned UV tile into more tiles.
            ProcessEntireMeshMode(assignUVTiles.Where(tile => tile.mode == PrefabulousAssignUVTile.AssignMode.EntireMesh).ToArray());
            ProcessBlendshapesMode(context, assignUVTiles.Where(tile => tile.mode == PrefabulousAssignUVTile.AssignMode.BlendShapes).ToArray());
            
            PrefabulousUtil.DestroyAllAfterBake<PrefabulousAssignUVTile>(context);
        }

        private void ProcessEntireMeshMode(PrefabulousAssignUVTile[] assignUVTiles_onlyEntireMeshMode)
        {
            foreach (var assignUVTile in assignUVTiles_onlyEntireMeshMode)
            {
                foreach (var renderer in assignUVTile.entireMeshes)
                {
                    // Caution: "renderer is Type" bypasses Unity object lifetime check
                    if (renderer == null) continue;
                    
                    if (renderer is SkinnedMeshRenderer smr)
                    {
                        var originalMesh = smr.sharedMesh;
                        if (originalMesh == null) continue;

                        smr.sharedMesh = CopyAndAssignEntirely(originalMesh, assignUVTile);

                    }
                    else if (renderer is MeshRenderer mr)
                    {
                        var mf = mr.GetComponent<MeshFilter>();
                        if (mf == null) continue;
                        
                        var originalMesh = mf.sharedMesh;
                        if (originalMesh == null) continue;

                        mf.sharedMesh = CopyAndAssignEntirely(originalMesh, assignUVTile);
                    }
                }
            }
        }

        private Mesh CopyAndAssignEntirely(Mesh originalMesh, PrefabulousAssignUVTile assignUVTile)
        {
            var mesh = Object.Instantiate(originalMesh);
            
            var channelNumber = AsChannelNumber(assignUVTile.uvChannel);
            var existingData = assignUVTile.existingData;
            var u = assignUVTile.u;
            var v = assignUVTile.v;
            
            var vertexCount = originalMesh.vertexCount;

            var existingUvs = PrefabulousUtil.GetUVsDefensively(originalMesh, channelNumber);
            var uvs = existingData == PrefabulousAssignUVTile.ExistingData.DoNotClear || existingData == PrefabulousAssignUVTile.ExistingData.Shift
                ? existingUvs
                : Enumerable.Repeat(
                    existingData == PrefabulousAssignUVTile.ExistingData.SetToZero
                        ? new Vector4(0 + Offset, 0 + Offset, 0f, 0f)
                        : new Vector4(-1 + Offset, -1 + Offset, 0f, 0f),
                    vertexCount).ToArray();

            var middleOfTile = new Vector4(u + Offset, v + Offset, 0f, 0f);
            var displacement = new Vector4(u, v, 0f, 0f);
            for (var index = 0; index < vertexCount; index++)
            {
                uvs[index] = existingData == PrefabulousAssignUVTile.ExistingData.Shift
                    ? (existingUvs[index] + displacement)
                    : middleOfTile;
            }

            Fromd4rk.SetUV(mesh, channelNumber, uvs);
            
            return mesh;
        }

        private void ProcessBlendshapesMode(BuildContext context, PrefabulousAssignUVTile[] assignUVTiles_onlyBlendshapeMode)
        {
            var smrs = context.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null) continue;

                var thatSmrBlendShapes = Enumerable.Range(0, mesh.blendShapeCount)
                    .Select(i => mesh.GetBlendShapeName(i))
                    .ToList();
                var applicableBlendShapes = assignUVTiles_onlyBlendshapeMode
                    .Where(recalculate => !recalculate.limitToSpecificMeshes || recalculate.renderers.Contains(smr))
                    .SelectMany(uvTile =>
                    {
                        return uvTile.blendShapes
                            .Where(blendShape => thatSmrBlendShapes.Contains(blendShape))
                            .Distinct()
                            .Select(blendShape => new AssignUVTileIntermediate
                            {
                                blendShape = blendShape,
                                channel = uvTile.uvChannel,
                                u = uvTile.u,
                                v = uvTile.v,
                                existingData = uvTile.existingData
                            });
                    })
                    .ToList();
                var keepPartialBlendshapes = assignUVTiles_onlyBlendshapeMode
                    .Where(recalculate => recalculate.keepPartialPolygons)
                    .Where(recalculate => !recalculate.limitToSpecificMeshes || recalculate.renderers.Contains(smr))
                    .SelectMany(normals => normals.blendShapes)
                    .Where(blendShape => thatSmrBlendShapes.Contains(blendShape))
                    .Distinct()
                    .ToList();
                if (applicableBlendShapes.Count > 0)
                {
                    AssignBlendshapeUVTileOf(smr, thatSmrBlendShapes, applicableBlendShapes, keepPartialBlendshapes);
                }
            }
        }

        internal struct AssignUVTileIntermediate
        {
            public string blendShape;
            public PrefabulousAssignUVTile.UVChannel channel;
            public int u;
            public int v;
            public PrefabulousAssignUVTile.ExistingData existingData;
        }
        //
        // internal struct AssignUVTileWork
        // {
        //     public string blendShape;
        //     public Dictionary<PrefabulousHaiAssignUVTile.UVChannel, AssignUVTileChannels> channels;
        // }
        //
        // internal struct AssignUVTileChannels
        // {
        //     public int u;
        //     public int v;
        // }

        private void AssignBlendshapeUVTileOf(SkinnedMeshRenderer smr, List<string> thatSmrBlendShapes,
            List<AssignUVTileIntermediate> applicableBlendShapes, List<string> keepPartialBlendshapes)
        {
            // TODO: If multiple SMRs share the same sharedMesh, it may not be necessary to do this op on all of them
            // However, it's rare for a single avatar to be referencing the same SMR mesh mutliple times.
            var originalMesh = smr.sharedMesh;

            var mesh = Object.Instantiate(originalMesh);

            var channelToBlendShapes = applicableBlendShapes.GroupBy(intermediate => intermediate.channel).ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());
            foreach (var channelToBlendShape in channelToBlendShapes)
            {
                var uvChannel = channelToBlendShape.Key;
                var intermediatesOfThisChannel = channelToBlendShape.Value;
                
                var channelNumber = AsChannelNumber(uvChannel);
                var existingData = GetExistingData(intermediatesOfThisChannel);

                var existingUvs = PrefabulousUtil.GetUVsDefensively(originalMesh, channelNumber);
                var uvs = existingData == PrefabulousAssignUVTile.ExistingData.DoNotClear || existingData == PrefabulousAssignUVTile.ExistingData.Shift
                    ? existingUvs
                    : Enumerable.Repeat(
                        existingData == PrefabulousAssignUVTile.ExistingData.SetToZero
                            ? new Vector4(0 + Offset, 0 + Offset, 0f, 0f)
                            : new Vector4(-1 + Offset, -1 + Offset, 0f, 0f),
                        originalMesh.vertexCount).ToArray();
                
                
                foreach (var intermediate in intermediatesOfThisChannel)
                {
                    var middleOfTile = new Vector4(intermediate.u + Offset, intermediate.v + Offset, 0f, 0f);
                    var displacement = new Vector4(intermediate.u, intermediate.v, 0f, 0f);
                    PrefabulousUtil.FigureOutAffectedVertices(out var verticesToDelete, out var partialVertices, thatSmrBlendShapes, new[] { intermediate.blendShape }.ToList(), keepPartialBlendshapes, originalMesh);
                    for (var index = 0; index < verticesToDelete.Length; index++)
                    {
                        var shouldAssign = verticesToDelete[index];
                        if (shouldAssign)
                        {
                            uvs[index] = existingData == PrefabulousAssignUVTile.ExistingData.Shift
                                ? (existingUvs[index] + displacement)
                                : middleOfTile;
                        }
                    }
                }

                Fromd4rk.SetUV(mesh, channelNumber, uvs);
            }
            
            smr.sharedMesh = mesh;
        }

        private PrefabulousAssignUVTile.ExistingData GetExistingData(AssignUVTileIntermediate[] intermediatesOfThisChannel)
        {
            var datas = intermediatesOfThisChannel
                .Select(intermediate => intermediate.existingData)
                .Distinct()
                .ToArray();
            if (datas.Contains(PrefabulousAssignUVTile.ExistingData.SetToMinusOne)) return PrefabulousAssignUVTile.ExistingData.SetToMinusOne;
            if (datas.Contains(PrefabulousAssignUVTile.ExistingData.SetToZero)) return PrefabulousAssignUVTile.ExistingData.SetToZero;
            if (datas.Contains(PrefabulousAssignUVTile.ExistingData.Shift)) return PrefabulousAssignUVTile.ExistingData.Shift;
            return PrefabulousAssignUVTile.ExistingData.DoNotClear;
        }

        private static int AsChannelNumber(PrefabulousAssignUVTile.UVChannel uvChannel)
        {
            return (int)uvChannel;
        }
    }
}