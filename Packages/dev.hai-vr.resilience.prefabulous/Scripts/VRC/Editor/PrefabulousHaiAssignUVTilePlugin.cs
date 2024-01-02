using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Hai.Runtime;
using Prefabulous.VRC.Editor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(PrefabulousHaiAssignUVTilePlugin))]
namespace Prefabulous.VRC.Editor
{
    public class PrefabulousHaiAssignUVTilePlugin : Plugin<PrefabulousHaiAssignUVTilePlugin>
    {
        private const float Offset = 0.5f;

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Optimizing)
                .AfterPlugin<PrefabulousHaiDeletePolygonsPlugin>();

            seq.Run("Assign UV Tiles", AssignUVTIles);
        }

        private void AssignUVTIles(BuildContext context)
        {
            var assignUVTiles = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousHaiAssignUVTile>(true);
            if (assignUVTiles.Length == 0) return;

            var smrs = context.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null) continue;
                
                var thatSmrBlendShapes = Enumerable.Range(0, mesh.blendShapeCount)
                    .Select(i => mesh.GetBlendShapeName(i))
                    .ToList();
                var applicableBlendShapes = assignUVTiles
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
                var keepPartialBlendshapes = assignUVTiles
                    .Where(recalculate => recalculate.keepPartialPolygons)
                    .Where(recalculate => !recalculate.limitToSpecificMeshes || recalculate.renderers.Contains(smr))
                    .SelectMany(normals => normals.blendShapes)
                    .Where(blendShape => thatSmrBlendShapes.Contains(blendShape))
                    .Distinct()
                    .ToList();
                if (applicableBlendShapes.Count > 0)
                {
                    AssignUVTileOf(smr, thatSmrBlendShapes, applicableBlendShapes, keepPartialBlendshapes);
                }
            }
        }

        internal struct AssignUVTileIntermediate
        {
            public string blendShape;
            public PrefabulousHaiAssignUVTile.UVChannel channel;
            public int u;
            public int v;
            public PrefabulousHaiAssignUVTile.ExistingData existingData;
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

        private void AssignUVTileOf(SkinnedMeshRenderer smr, List<string> thatSmrBlendShapes,
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
                var uvs = existingData == PrefabulousHaiAssignUVTile.ExistingData.DoNotClear || existingData == PrefabulousHaiAssignUVTile.ExistingData.Shift
                    ? existingUvs
                    : Enumerable.Repeat(
                        existingData == PrefabulousHaiAssignUVTile.ExistingData.SetToZero
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
                            uvs[index] = existingData == PrefabulousHaiAssignUVTile.ExistingData.Shift
                                ? (existingUvs[index] + displacement)
                                : middleOfTile;
                        }
                    }
                }

                Fromd4rk.SetUV(mesh, channelNumber, uvs);
            }
            
            smr.sharedMesh = mesh;
        }

        private PrefabulousHaiAssignUVTile.ExistingData GetExistingData(AssignUVTileIntermediate[] intermediatesOfThisChannel)
        {
            var datas = intermediatesOfThisChannel
                .Select(intermediate => intermediate.existingData)
                .Distinct()
                .ToArray();
            if (datas.Contains(PrefabulousHaiAssignUVTile.ExistingData.SetToMinusOne)) return PrefabulousHaiAssignUVTile.ExistingData.SetToMinusOne;
            if (datas.Contains(PrefabulousHaiAssignUVTile.ExistingData.SetToZero)) return PrefabulousHaiAssignUVTile.ExistingData.SetToZero;
            if (datas.Contains(PrefabulousHaiAssignUVTile.ExistingData.Shift)) return PrefabulousHaiAssignUVTile.ExistingData.Shift;
            return PrefabulousHaiAssignUVTile.ExistingData.DoNotClear;
        }

        private static int AsChannelNumber(PrefabulousHaiAssignUVTile.UVChannel uvChannel)
        {
            return (int)uvChannel;
        }

        private Mesh InstantiateNewMeshAndDeleteVerticesFromMesh(Mesh originalMesh, bool[] verticesToDelete, bool[] partialVertices, Dictionary<int, int> oldIndexToNewIndex, string smrName)
        {
            var mesh = new Mesh();
            
            var verts = new Vector3[originalMesh.vertexCount];
            var norms = new Vector3[originalMesh.vertexCount];
            var tans = new Vector3[originalMesh.vertexCount];

            mesh.bindposes = originalMesh.bindposes;
            mesh.name = originalMesh.name;
            mesh.bounds = originalMesh.bounds;

            var newVertexCount = verticesToDelete.Count(b => !b);
            Debug.Log($"({GetType().Name}) Deleting {originalMesh.vertexCount - newVertexCount} vertices (out of {originalMesh.vertexCount}) from {smrName}");

            Fromd4rk.SetMeshIndexFormat(mesh, newVertexCount);
            mesh.SetVertices(NewArrayDeleteVerts(originalMesh.vertices, verticesToDelete));
            mesh.SetNormals(NewArrayDeleteVerts(originalMesh.normals, verticesToDelete));
            mesh.SetTangents(NewArrayDeleteVerts(originalMesh.tangents, verticesToDelete));
            
            mesh.boneWeights = NewArrayDeleteVerts(originalMesh.boneWeights, verticesToDelete);

            var channels = Enumerable.Range(0, 8)
                .Select(uvChannel => PrefabulousUtil.GetUVsDefensively(originalMesh, uvChannel))
                .Select(uvs => NewArrayDeleteVerts(uvs, verticesToDelete))
                .ToArray();

            Fromd4rk.SetUVs(mesh, channels);

            if (originalMesh.HasVertexAttribute(VertexAttribute.Color))
            {
                if (originalMesh.GetVertexAttributeFormat(VertexAttribute.Color) == VertexAttributeFormat.UNorm8)
                {
                    mesh.colors32 = NewArrayDeleteVerts(originalMesh.colors32, verticesToDelete);
                }
                else
                {
                    mesh.colors = NewArrayDeleteVerts(originalMesh.colors, verticesToDelete);
                }
            }

            mesh.subMeshCount = originalMesh.subMeshCount;
            
            for (var subMeshIndex = 0; subMeshIndex < originalMesh.subMeshCount; subMeshIndex++)
            {
                var indices = originalMesh.GetIndices(subMeshIndex);
                var newIndices = NewTrianglesDeleteVerts(indices, verticesToDelete, partialVertices, oldIndexToNewIndex);
                mesh.SetIndices(newIndices, MeshTopology.Triangles, subMeshIndex);
            }

            var originalTriangleCount = originalMesh.triangles.Length;
            Debug.Log($"({GetType().Name}) Deleting {(originalTriangleCount - mesh.triangles.Length) / 3} triangles (out of {originalTriangleCount / 3}) from {smrName}");

            for (var shapeIndex = 0; shapeIndex < originalMesh.blendShapeCount; shapeIndex++)
            {
                var name = originalMesh.GetBlendShapeName(shapeIndex);
                var frames = originalMesh.GetBlendShapeFrameCount(shapeIndex);
                for (var frameIndex = 0; frameIndex < frames; frameIndex++)
                {
                    var weight = originalMesh.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    originalMesh.GetBlendShapeFrameVertices(shapeIndex, frameIndex, verts, norms, tans);

                    var newDeltaVerts = NewArrayDeleteVerts(verts, verticesToDelete);
                    var newDeltaNorms = NewArrayDeleteVerts(norms, verticesToDelete);
                    var newDeltaTans = NewArrayDeleteVerts(tans, verticesToDelete);
                    mesh.AddBlendShapeFrame(name, weight, newDeltaVerts, newDeltaNorms, newDeltaTans);
                }
            }

            return mesh;
        }

        private static T[] NewArrayDeleteVerts<T>(T[] originalVertices, bool[] verticesToDelete)
        {
            return originalVertices
                .Where((_, vertexIndex) => !verticesToDelete[vertexIndex])
                .ToArray();
        }

        private int[] NewTrianglesDeleteVerts(int[] triangles, bool[] verticesToDelete, bool[] partialVertices,
            Dictionary<int, int> oldIndexToNewIndex)
        {
            var originalTriangleTriads = triangles;
            var triangleCount = originalTriangleTriads.Length / 3;
            var newTriangles = Enumerable.Range(0, triangleCount)
                .SelectMany(triangleIndex =>
                {
                    var a = originalTriangleTriads[3 * triangleIndex];
                    var b = originalTriangleTriads[3 * triangleIndex + 1];
                    var c = originalTriangleTriads[3 * triangleIndex + 2];

                    var atLeastOneVertexDoesNotExist = verticesToDelete[a] || verticesToDelete[b] || verticesToDelete[c];
                    bool AreAllPartialVertices() => partialVertices[a] || partialVertices[b] || partialVertices[c];

                    var shouldDeleteTriangle = atLeastOneVertexDoesNotExist || AreAllPartialVertices();
                    if (shouldDeleteTriangle)
                    {
                        return Array.Empty<int>();
                    }

                    return new[] { oldIndexToNewIndex[a], oldIndexToNewIndex[b], oldIndexToNewIndex[c] };
                })
                .ToArray();

            return newTriangles;
        }
    }
}