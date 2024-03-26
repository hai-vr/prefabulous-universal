using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Hai.Runtime;
using Prefabulous.VRC.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(PrefabulousHaiConvertBlendshapeConventionsPlugin))]
namespace Prefabulous.VRC.Editor
{
    public class PrefabulousHaiConvertBlendshapeConventionsPlugin : Plugin<PrefabulousHaiConvertBlendshapeConventionsPlugin>
    {
        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Transforming)
                .AfterPlugin("Hai.FaceTraShape.Editor.HFTSCPlugin");
            
            seq.Run("Convert Blendshape Conventions", GenerateBlendshapes);
        }

        private void GenerateBlendshapes(BuildContext context)
        {
            var converts = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousHaiConvertBlendshapeConventions>(true);
            if (converts.Length == 0) return;

            var smrs = context.AvatarRootTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null) continue;
                
                var thatSmrBlendShapes = PrefabulousUtil.GetAllBlendshapeNames(smr).ToList();
                var applicablePairs = converts
                    .Where(convert => !convert.limitToSpecificMeshes || convert.renderers.Contains(smr))
                    .SelectMany(convert => convert.ParseMapping())
                    .Where(pairs => thatSmrBlendShapes.Contains(pairs.Key) && !thatSmrBlendShapes.Contains(pairs.Value))
                    .Distinct()
                    .ToList();

                if (applicablePairs.Count > 0)
                {
                    GenerateConversion(smr, applicablePairs, thatSmrBlendShapes);
                }
            }
            
            PrefabulousUtil.DestroyAllAfterBake<PrefabulousHaiConvertBlendshapeConventions>(context);
        }

        private void GenerateConversion(SkinnedMeshRenderer smr, List<KeyValuePair<string, string>> applicablePairs, List<string> blendshapeList)
        {
            // TODO: We're starting to have several blendshape processors that duplicate the mesh. This should be only done once to avoid unnecessary waste.
            var mesh = Object.Instantiate(smr.sharedMesh);
            
            var vertexCount = mesh.vertexCount;
            var storagePosition = new Vector3[vertexCount];
            var storageNormal = new Vector3[vertexCount];
            var storageTangent = new Vector3[vertexCount];
            
            foreach (var keyValuePair in applicablePairs)
            {
                var source = keyValuePair.Key;
                var destination = keyValuePair.Value;
                
                var sourceIndex = blendshapeList.IndexOf(source);
                var frameCount = mesh.GetBlendShapeFrameCount(sourceIndex);
                for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    var frameWeight = mesh.GetBlendShapeFrameWeight(sourceIndex, frameIndex);
                    mesh.GetBlendShapeFrameVertices(sourceIndex, frameIndex, storagePosition, storageNormal, storageTangent);
                    mesh.AddBlendShapeFrame(destination, frameWeight, storagePosition, storageNormal, storageTangent);
                }
            }

            smr.sharedMesh = mesh;
        }
    }
}