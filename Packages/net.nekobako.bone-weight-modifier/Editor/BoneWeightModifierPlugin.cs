using nadena.dev.ndmf;
using net.nekobako.BoneWeightModifier.Editor;

[assembly: ExportsPlugin(typeof(BoneWeightModifierPlugin))]

namespace net.nekobako.BoneWeightModifier.Editor
{
    [RunsOnAllPlatforms]
    internal class BoneWeightModifierPlugin : Plugin<BoneWeightModifierPlugin>
    {
        public override string QualifiedName => "net.nekobako.bone-weight-modifier";
        public override string DisplayName => "Bone Weight Modifier";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(BoneWeightModifierPass.Instance)
                .PreviewingWith(new BoneWeightModifierPreview());
        }
    }
}
