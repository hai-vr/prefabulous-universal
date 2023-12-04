using AnimatorAsCode.V1.ModularAvatar;
using AnimatorAsCode.V1.NDMFProcessor;
using nadena.dev.ndmf;
using Prefabulous.VRC.Editor;
using Prefabulous.VRC.Runtime;

[assembly: ExportsPlugin(typeof(PrefabulousImportExpressionParametersPlugin))]
namespace Prefabulous.VRC.Editor
{
    public class PrefabulousImportExpressionParametersPlugin : AacPlugin<PrefabulousImportExpressionParameters>
    {
        protected override AacPluginOutput Execute()
        {
            if (my.parameters == null) return AacPluginOutput.Regular();
            
            var ma = MaAc.Create(my.gameObject);
            ma.ImportParameters(my.parameters);
            
            return AacPluginOutput.Regular();
        }
    }
}
