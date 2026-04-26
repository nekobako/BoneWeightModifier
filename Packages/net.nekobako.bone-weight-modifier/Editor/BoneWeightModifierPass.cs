using System.Linq;
using UnityEngine;
using nadena.dev.ndmf;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightModifierPass : Pass<BoneWeightModifierPass>
    {
        public override string QualifiedName => "net.nekobako.bone-weight-modifier";
        public override string DisplayName => "Bone Weight Modifier";

        protected override void Execute(BuildContext context)
        {
            var modifiers = context.AvatarRootObject.GetComponentsInChildren<BoneWeightModifier>(true);

            foreach (var grouping in modifiers
                .Where(x =>
                    x.Renderer is MeshRenderer && x.Renderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh ||
                    x.Renderer is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
                .GroupBy(x => x.Renderer))
            {
                BoneWeightModifierProcessor.Process(grouping.Key, grouping
                    .SelectMany(x => x.Weights, (x, y) => (x.Bone ? x.Bone : x.transform, y))
                    .ToArray());
            }

            foreach (var modifier in modifiers)
            {
                Object.DestroyImmediate(modifier);
            }
        }
    }
}
