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
            var binders = context.AvatarRootObject.GetComponentsInChildren<BoneWeightBinder>(true);

            foreach (var grouping in modifiers
                .Where(x => x.Renderer && RendererUtils.GetSharedMesh(x.Renderer))
                .GroupBy(x => x.Renderer))
            {
                var renderer = grouping.Key;
                if (renderer is not SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    var gameObject = renderer.gameObject;
                    var meshFilter = renderer.GetComponent<MeshFilter>();

                    var allowOcclusionWhenDynamic           = renderer.allowOcclusionWhenDynamic;
                    // var bounds                           : Not Serialized
                    var enabled                             = renderer.enabled;
                    // var forceRenderingOff                : Not Serialized
                    // var isPartOfStaticBatch              : Read Only
                    // var isVisible                        : Read Only
                    // var lightmapIndex                    : Not Serialized
                    // var lightmapScaleOffset              : Not Serialized
                    var lightProbeProxyVolumeOverride       = renderer.lightProbeProxyVolumeOverride;
                    var lightProbeUsage                     = renderer.lightProbeUsage;
                    var localBounds                         = renderer.localBounds;
                    // var localToWorldMatrix               : Read Only
                    // var material                         : Set by sharedMaterials
                    // var materials                        : Set by sharedMaterials
                    var motionVectorGenerationMode          = renderer.motionVectorGenerationMode;
                    var probeAnchor                         = renderer.probeAnchor;
                    var rayTracingMode                      = renderer.rayTracingMode;
                    // var realtimeLightmapIndex            : Not Serialized
                    // var realtimeLightmapScaleOffset      : Not Serialized
                    var receiveShadows                      = renderer.receiveShadows;
                    var reflectionProbeUsage                = renderer.reflectionProbeUsage;
                    var rendererPriority                    = renderer.rendererPriority;
                    var renderingLayerMask                  = renderer.renderingLayerMask;
                    var shadowCastingMode                   = renderer.shadowCastingMode;
                    // var sharedMaterial                   : Set by sharedMaterials
                    var sharedMaterials                     = renderer.sharedMaterials;
                    var sortingLayerID                      = renderer.sortingLayerID;
                    // var sortingLayerID                   : Set by sortingLayerName
                    var sortingOrder                        = renderer.sortingOrder;
                    var staticShadowCaster                  = renderer.staticShadowCaster;
                    // var worldToLocalMatrix               : Read Only
                    // var bones                            : Not Provided
                    // var forceMatrixRecalculationPerRender: Not Serialized
                    // var quality                          : Not Provided
                    var sharedMesh                          = meshFilter.sharedMesh;
                    // var skinnedMotionVectors             : Not Provided
                    // var updateWhenOffscreen              : Not Provided
                    // var vertexBufferTarget               : Not Serialized

                    Object.DestroyImmediate(meshFilter);
                    Object.DestroyImmediate(renderer);
                    skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();

                    skinnedMeshRenderer.allowOcclusionWhenDynamic           = allowOcclusionWhenDynamic;
                    // skinnedMeshRenderer.bounds                           : Not Serialized
                    skinnedMeshRenderer.enabled                             = enabled;
                    // skinnedMeshRenderer.forceRenderingOff                : Not Serialized
                    // skinnedMeshRenderer.isPartOfStaticBatch              : Read Only
                    // skinnedMeshRenderer.isVisible                        : Read Only
                    // skinnedMeshRenderer.lightmapIndex                    : Not Serialized
                    // skinnedMeshRenderer.lightmapScaleOffset              : Not Serialized
                    skinnedMeshRenderer.lightProbeProxyVolumeOverride       = lightProbeProxyVolumeOverride;
                    skinnedMeshRenderer.lightProbeUsage                     = lightProbeUsage;
                    skinnedMeshRenderer.localBounds                         = localBounds;
                    // skinnedMeshRenderer.localToWorldMatrix               : Read Only
                    // skinnedMeshRenderer.material                         : Set by sharedMaterials
                    // skinnedMeshRenderer.materials                        : Set by sharedMaterials
                    skinnedMeshRenderer.motionVectorGenerationMode          = motionVectorGenerationMode;
                    skinnedMeshRenderer.probeAnchor                         = probeAnchor;
                    skinnedMeshRenderer.rayTracingMode                      = rayTracingMode;
                    // skinnedMeshRenderer.realtimeLightmapIndex            : Not Serialized
                    // skinnedMeshRenderer.realtimeLightmapScaleOffset      : Not Serialized
                    skinnedMeshRenderer.receiveShadows                      = receiveShadows;
                    skinnedMeshRenderer.reflectionProbeUsage                = reflectionProbeUsage;
                    skinnedMeshRenderer.rendererPriority                    = rendererPriority;
                    skinnedMeshRenderer.renderingLayerMask                  = renderingLayerMask;
                    skinnedMeshRenderer.shadowCastingMode                   = shadowCastingMode;
                    // skinnedMeshRenderer.sharedMaterial                   : Set by sharedMaterials
                    skinnedMeshRenderer.sharedMaterials                     = sharedMaterials;
                    skinnedMeshRenderer.sortingLayerID                      = sortingLayerID;
                    // skinnedMeshRenderer.sortingLayerID                   : Set by sortingLayerName
                    skinnedMeshRenderer.sortingOrder                        = sortingOrder;
                    skinnedMeshRenderer.staticShadowCaster                  = staticShadowCaster;
                    // skinnedMeshRenderer.worldToLocalMatrix               : Read Only
                    // skinnedMeshRenderer.bones                            : Not Provided
                    // skinnedMeshRenderer.forceMatrixRecalculationPerRender: Not Serialized
                    // skinnedMeshRenderer.quality                          : Not Provided
                    skinnedMeshRenderer.sharedMesh                          = sharedMesh;
                    // skinnedMeshRenderer.skinnedMotionVectors             : Not Provided
                    // skinnedMeshRenderer.updateWhenOffscreen              : Not Provided
                    // skinnedMeshRenderer.vertexBufferTarget               : Not Serialized
                }

                BoneWeightModifierProcessor.Process(skinnedMeshRenderer, grouping.ToArray());
            }

            foreach (var modifier in modifiers)
            {
                Object.DestroyImmediate(modifier);
            }
            foreach (var binder in binders)
            {
                Object.DestroyImmediate(binder);
            }
        }
    }
}
