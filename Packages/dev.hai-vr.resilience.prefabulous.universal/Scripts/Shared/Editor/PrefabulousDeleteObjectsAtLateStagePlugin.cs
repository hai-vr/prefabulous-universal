#if PREFABULOUS_INTERNAL
using System.Linq;
using nadena.dev.ndmf;
using Prefabulous.Universal.Common.Runtime;
using Prefabulous.Universal.Shared.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: ExportsPlugin(typeof(PrefabulousDeleteObjectsAtLateStagePlugin))]
namespace Prefabulous.Universal.Shared.Editor
{
    public class PrefabulousDeleteObjectsAtLateStagePlugin : Plugin<PrefabulousDeleteObjectsAtLateStagePlugin>
    {
        public override string QualifiedName => "dev.hai-vr.prefabulous.universal.DeleteObjectsAtLateStage";
        public override string DisplayName => "Prefabulous Universal - Delete Objects At Late Stage";

        protected override void Configure()
        {
            var seq = InPhase(BuildPhase.Optimizing);

            seq
                .AfterPlugin("com.anatawa12.avatar-optimizer")
                .Run("Delete Objects At Late Stage", DeleteObjects);
        }

        private void DeleteObjects(BuildContext context)
        {
            var delete = context.AvatarRootTransform.GetComponentsInChildren<PrefabulousDeleteObjectsAtLateStage>(true);
            if (delete.Length == 0) return;

            var objectsToDelete = delete
                .SelectMany(comp =>
                {
                    var objs = comp.objects != null ? comp.objects.Where(o => o != null) : Enumerable.Empty<GameObject>();
                    if (comp.deleteThisObject)
                    {
                        return objs.Concat(new[] { comp.gameObject });
                    }

                    return objs;
                })
                .Distinct()
                .ToArray();
            
            foreach (var gameObject in objectsToDelete)
            {
                if (gameObject != null) // Could happen in hierarchy deletions
                {
                    Object.DestroyImmediate(gameObject);
                }
            }

            PrefabulousUtil.DestroyAllAfterBake<PrefabulousDeleteObjectsAtLateStage>(context);
        }
    }
}
#endif