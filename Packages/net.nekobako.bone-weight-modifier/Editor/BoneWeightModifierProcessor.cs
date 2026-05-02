using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using nadena.dev.ndmf.runtime;
using Object = UnityEngine.Object;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal static class BoneWeightModifierProcessor
    {
        public readonly struct Context
        {
            public SkinnedMeshRenderer Renderer { get; init; }
            public int VertexCount { get; init; }
            public int SubMeshCount { get; init; }
            public NativeArray<Vector3>.ReadOnly VertexPositions { get; init; }
            public NativeArray<Vector2>.ReadOnly VertexUvs { get; init; }
            public NativeBitArray.ReadOnly SubMeshMasks { get; init; }
        }

        public static void Process(Renderer renderer, (Transform, IBoneWeight)[] weights)
        {
            if (renderer is not SkinnedMeshRenderer skinnedMeshRenderer)
            {
                var gameObject = renderer.gameObject;
                var meshFilter = renderer.GetComponent<MeshFilter>();

                var allowOcclusionWhenDynamic     = renderer.allowOcclusionWhenDynamic;
                // var bounds                     : Not Serialized
                var enabled                       = renderer.enabled;
                // var forceRenderingOff          : Not Serialized
                // var isPartOfStaticBatch        : Read Only
                // var isVisible                  : Read Only
                // var lightmapIndex              : Not Serialized
                // var lightmapScaleOffset        : Not Serialized
                var lightProbeProxyVolumeOverride = renderer.lightProbeProxyVolumeOverride;
                var lightProbeUsage               = renderer.lightProbeUsage;
                var localBounds                   = renderer.localBounds;
                // var localToWorldMatrix         : Read Only
                // var material                   : Set by sharedMaterials
                // var materials                  : Set by sharedMaterials
                var motionVectorGenerationMode    = renderer.motionVectorGenerationMode;
                var probeAnchor                   = renderer.probeAnchor;
                var rayTracingMode                = renderer.rayTracingMode;
                // var realtimeLightmapIndex      : Not Serialized
                // var realtimeLightmapScaleOffset: Not Serialized
                var receiveShadows                = renderer.receiveShadows;
                var reflectionProbeUsage          = renderer.reflectionProbeUsage;
                var rendererPriority              = renderer.rendererPriority;
                var renderingLayerMask            = renderer.renderingLayerMask;
                var shadowCastingMode             = renderer.shadowCastingMode;
                // var sharedMaterial             : Set by sharedMaterials
                var sharedMaterials               = renderer.sharedMaterials;
                var sortingLayerID                = renderer.sortingLayerID;
                // var sortingLayerID             : Set by sortingLayerName
                var sortingOrder                  = renderer.sortingOrder;
                var staticShadowCaster            = renderer.staticShadowCaster;
                // var worldToLocalMatrix         : Read Only
                // var bones                      : Not Provided
                // var quality                    : Not Provided
                var sharedMesh                    = meshFilter.sharedMesh;
                // var skinnedMotionVectors       : Not Provided
                // var updateWhenOffscreen        : Not Provided

                Object.DestroyImmediate(meshFilter);
                Object.DestroyImmediate(renderer);
                skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();

                skinnedMeshRenderer.allowOcclusionWhenDynamic     = allowOcclusionWhenDynamic;
                // skinnedMeshRenderer.bounds                     : Not Serialized
                skinnedMeshRenderer.enabled                       = enabled;
                // skinnedMeshRenderer.forceRenderingOff          : Not Serialized
                // skinnedMeshRenderer.isPartOfStaticBatch        : Read Only
                // skinnedMeshRenderer.isVisible                  : Read Only
                // skinnedMeshRenderer.lightmapIndex              : Not Serialized
                // skinnedMeshRenderer.lightmapScaleOffset        : Not Serialized
                skinnedMeshRenderer.lightProbeProxyVolumeOverride = lightProbeProxyVolumeOverride;
                skinnedMeshRenderer.lightProbeUsage               = lightProbeUsage;
                skinnedMeshRenderer.localBounds                   = localBounds;
                // skinnedMeshRenderer.localToWorldMatrix         : Read Only
                // skinnedMeshRenderer.material                   : Set by sharedMaterials
                // skinnedMeshRenderer.materials                  : Set by sharedMaterials
                skinnedMeshRenderer.motionVectorGenerationMode    = motionVectorGenerationMode;
                skinnedMeshRenderer.probeAnchor                   = probeAnchor;
                skinnedMeshRenderer.rayTracingMode                = rayTracingMode;
                // skinnedMeshRenderer.realtimeLightmapIndex      : Not Serialized
                // skinnedMeshRenderer.realtimeLightmapScaleOffset: Not Serialized
                skinnedMeshRenderer.receiveShadows                = receiveShadows;
                skinnedMeshRenderer.reflectionProbeUsage          = reflectionProbeUsage;
                skinnedMeshRenderer.rendererPriority              = rendererPriority;
                skinnedMeshRenderer.renderingLayerMask            = renderingLayerMask;
                skinnedMeshRenderer.shadowCastingMode             = shadowCastingMode;
                // skinnedMeshRenderer.sharedMaterial             : Set by sharedMaterials
                skinnedMeshRenderer.sharedMaterials               = sharedMaterials;
                skinnedMeshRenderer.sortingLayerID                = sortingLayerID;
                // skinnedMeshRenderer.sortingLayerID             : Set by sortingLayerName
                skinnedMeshRenderer.sortingOrder                  = sortingOrder;
                skinnedMeshRenderer.staticShadowCaster            = staticShadowCaster;
                // skinnedMeshRenderer.worldToLocalMatrix         : Read Only
                // skinnedMeshRenderer.bones                      : Not Provided
                // skinnedMeshRenderer.quality                    : Not Provided
                skinnedMeshRenderer.sharedMesh                    = sharedMesh;
                // skinnedMeshRenderer.skinnedMotionVectors       : Not Provided
                // skinnedMeshRenderer.updateWhenOffscreen        : Not Provided
            }

            var baked = new Mesh();
            skinnedMeshRenderer.BakeMesh(baked, true);

            var vertexCount = baked.vertexCount;
            var subMeshCount = baked.subMeshCount;
            using var vertexPositions = new NativeArray<Vector3>(baked.vertices, Allocator.Temp);
            using var vertexUvs = new NativeArray<Vector2>(baked.uv, Allocator.Temp);
            using var subMeshMasks = new NativeBitArray(vertexCount * subMeshCount, Allocator.Temp);
            for (var i = 0; i < subMeshCount; i++)
            {
                foreach (var index in baked.GetIndices(i))
                {
                    subMeshMasks.Set(vertexCount * i + index, true);
                }
            }

            Object.DestroyImmediate(baked);

            var context = new Context
            {
                Renderer = skinnedMeshRenderer,
                VertexCount = vertexCount,
                SubMeshCount = subMeshCount,
                VertexPositions = vertexPositions.AsReadOnly(),
                VertexUvs = vertexUvs.AsReadOnly(),
                SubMeshMasks = subMeshMasks.AsReadOnly(),
            };

            var mesh = Object.Instantiate(skinnedMeshRenderer.sharedMesh);
            var bones = skinnedMeshRenderer.bones
                .Concat(weights
                    .Select(x => x.Item1)
                    .Where(x => !skinnedMeshRenderer.bones.Contains(x))
                    .Distinct())
                .ToList();
            var boneIndices = bones
                .Select((x, i) => (bone: x, index: i))
                .Where(x => x.bone)
                .GroupBy(x => x.bone)
                .ToDictionary(x => x.Key, x => x.First().index);
            var fallbackBoneIndex = boneIndices.GetValueOrDefault(skinnedMeshRenderer.transform, bones.Count);

            var bindposes = new List<Matrix4x4>();
            for (var i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                if (bone && bone.TryGetComponent<BoneWeightBinder>(out var binder))
                {
                    if (binder.IsBound)
                    {
                        var root = RuntimeUtil.FindAvatarInParents(bone);
                        if (root)
                        {
                            bindposes.Add(binder.Bindpose * root.worldToLocalMatrix * skinnedMeshRenderer.transform.localToWorldMatrix);
                            continue;
                        }
                    }
                }
                else
                {
                    if (i < skinnedMeshRenderer.bones.Length && i < mesh.bindposes.Length)
                    {
                        bindposes.Add(mesh.bindposes[i]);
                        continue;
                    }
                }

                if (bone)
                {
                    bindposes.Add(bone.worldToLocalMatrix * skinnedMeshRenderer.transform.localToWorldMatrix);
                }
                else
                {
                    bindposes.Add(Matrix4x4.identity);
                }
            }

            var processors = weights
                .Distinct()
                .ToDictionary(x => x, x => BoneWeightProcessor.Create(x.Item1, x.Item2, bindposes[boneIndices[x.Item1]], context));

            using var boneWeights = mesh.GetAllBoneWeights();
            using var boneWeightCounts = mesh.GetBonesPerVertex();
            using var boneWeightsList = new NativeList<BoneWeight1>(boneWeights.Length, Allocator.Temp);
            using var boneWeightCountsList = new NativeList<byte>(boneWeightCounts.Length, Allocator.Temp);
            var boneWeightIndex = 0;
            var boneWeightBuffer = new Dictionary<Transform, BoneWeight1>(byte.MaxValue);
            var boneWeightBufferList = new List<BoneWeight1>(byte.MaxValue);
            for (var i = 0; i < vertexCount; i++)
            {
                var boneWeightCount = i < boneWeightCounts.Length ? boneWeightCounts[i] : 0;
                foreach (var boneWeight in boneWeights.Slice(boneWeightIndex, boneWeightCount))
                {
                    var bone = boneWeight.boneIndex < skinnedMeshRenderer.bones.Length ? bones[boneWeight.boneIndex] : null;
                    if (bone)
                    {
                        boneWeightBuffer[bone] = boneWeight;
                    }
                }

                foreach (var (bone, weight) in weights)
                {
                    var boneWeight = boneWeightBuffer.TryGetValue(bone, out var x) ? x : new()
                    {
                        boneIndex = boneIndices[bone],
                        weight = 0.0f,
                    };
                    processors[(bone, weight)].Process(i, ref boneWeight);
                    boneWeightBuffer[bone] = boneWeight;
                }

                boneWeightBufferList.AddRange(boneWeightBuffer.Values.Where(x => x.weight > 0.0f));
                boneWeightBufferList.Sort((a, b) => b.weight.CompareTo(a.weight));

                if (boneWeightBufferList.Count == 0)
                {
                    boneWeightBufferList.Add(new()
                    {
                        boneIndex = fallbackBoneIndex,
                        weight = 1.0f,
                    });
                }

                var totalWeight = boneWeightBufferList.Sum(x => x.weight);
                foreach (var boneWeight in boneWeightBufferList)
                {
                    boneWeightsList.Add(new()
                    {
                        boneIndex = boneWeight.boneIndex,
                        weight = boneWeight.weight / totalWeight,
                    });
                }
                boneWeightCountsList.Add((byte)boneWeightBufferList.Count);

                boneWeightIndex += boneWeightCount;
                boneWeightBuffer.Clear();
                boneWeightBufferList.Clear();
            }

            foreach (var processor in processors.Values)
            {
                processor.Dispose();
            }

            if (fallbackBoneIndex == bones.Count)
            {
                bones.Add(skinnedMeshRenderer.transform);
                bindposes.Add(Matrix4x4.identity);
            }

            // Workaround for errors such as "Unsupported conversion of vertex data (format 0 to 4, dimensions 4 to 4)"
            using var emptyBoneWeightsArray = new NativeArray<BoneWeight1>(0, Allocator.Temp);
            using var emptyBoneWeightCountsArray = new NativeArray<byte>(0, Allocator.Temp);
            mesh.SetBoneWeights(emptyBoneWeightCountsArray, emptyBoneWeightsArray);

            using var boneWeightsArray = boneWeightsList.ToArray(Allocator.Temp);
            using var boneWeightCountsArray = boneWeightCountsList.ToArray(Allocator.Temp);
            mesh.SetBoneWeights(boneWeightCountsArray, boneWeightsArray);

            mesh.bindposes = bindposes.ToArray();
            skinnedMeshRenderer.bones = bones.ToArray();
            skinnedMeshRenderer.sharedMesh = mesh;
        }
    }
}
