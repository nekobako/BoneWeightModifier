using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal static class BoneWeightModifierProcessor
    {
        public readonly ref struct Context
        {
            public Renderer OriginalRenderer { get; init; }
            public Renderer ProxyRenderer { get; init; }
            public ComputeContext ComputeContext { get; init; }
            public int VertexCount { get; init; }
            public int SubMeshCount { get; init; }
            public NativeArray<Vector3>.ReadOnly VertexPositions { get; init; }
            public NativeArray<Vector2>.ReadOnly VertexUvs { get; init; }
            public NativeBitArray.ReadOnly SubMeshMasks { get; init; }
        }

        public static void Process(Renderer renderer, BoneWeightModifier[] modifiers)
        {
            Process(renderer, renderer, modifiers, ComputeContext.NullContext);
        }

        public static void Process(Renderer original, Renderer proxy, BoneWeightModifier[] modifiers, ComputeContext context)
        {
            var baked = default(Mesh);
            switch (original)
            {
                case MeshRenderer when original.TryGetComponent<MeshFilter>(out var meshFilter):
                    baked = Object.Instantiate(meshFilter.sharedMesh);
                    break;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.BakeMesh(baked = new(), true);
                    break;
                default:
                    return;
            }
            context.ObserveTransformPosition(original.transform);

            var vertexCount = baked.vertexCount;
            var subMeshCount = baked.subMeshCount;
            using var vertexPositions = new NativeArray<Vector3>(baked.vertices, Allocator.Temp);
            using var vertexUvs = new NativeArray<Vector2>(baked.uv, Allocator.Temp);
            using var subMeshMasks = new NativeBitArray(vertexCount * subMeshCount, Allocator.Temp);
            var readonlyVertexPositions = vertexPositions.AsReadOnly();
            var readonlyVertexUvs = vertexUvs.AsReadOnly();
            var readonlySubMeshMasks = subMeshMasks.AsReadOnly();
            for (var i = 0; i < subMeshCount; i++)
            {
                foreach (var index in baked.GetIndices(i))
                {
                    subMeshMasks.Set(vertexCount * i + index, true);
                }
            }

            Object.DestroyImmediate(baked);

            var weights = modifiers
                .SelectMany(
                    x => context.Observe(x, y => y.Weights.Select(z => z.Clone()).ToArray(), Enumerable.SequenceEqual),
                    (x, y) => (bone: context.Observe(x, z => z.Bone) ? x.Bone : x.transform, weight: y))
                .ToArray();

            var mesh = Object.Instantiate(RendererUtils.GetSharedMesh(proxy));

            var bones = new List<Transform>(RendererUtils.GetBones(proxy));
            var existingBoneCount = bones.Count;
            bones.AddRange(weights
                .Select(x => x.bone)
                .Where(x => !bones.Contains(x))
                .Distinct());
            var extendedBoneCount = bones.Count;
            var boneIndices = bones
                .Select((x, i) => (bone: x, index: i))
                .Where(x => x.bone)
                .GroupBy(x => x.bone)
                .ToDictionary(x => x.Key, x => x.First().index);

            var rootBone = RendererUtils.GetRootBone(proxy);
            var fallbackBone = rootBone ? rootBone : original.transform;
            var fallbackBoneIndex = boneIndices.GetValueOrDefault(fallbackBone, extendedBoneCount);

            var bindposes = new List<Matrix4x4>();
            for (var i = 0; i < extendedBoneCount; i++)
            {
                var bone = bones[i];
                if (bone && context.TryGetComponent<BoneWeightBinder>(bone, out var binder))
                {
                    if (context.Observe(binder, x => x.IsBound))
                    {
                        var root = context.GetAvatarRoot(bone.gameObject);
                        if (root)
                        {
                            if (original is MeshRenderer)
                            {
                                context.ObserveTransformPosition(bone);
                            }
                            var origin = mesh.bindposeCount == 0 && rootBone ? rootBone : original.transform;
                            var originToWorld = context.ObserveTransformPosition(origin).localToWorldMatrix;
                            var worldToRoot = context.ObserveTransformPosition(root.transform).worldToLocalMatrix;
                            var rootToBone = context.Observe(binder, x => x.Bindpose);
                            bindposes.Add(rootToBone * worldToRoot * originToWorld);
                            continue;
                        }
                    }
                }
                else
                {
                    if (i < existingBoneCount && i < mesh.bindposes.Length)
                    {
                        bindposes.Add(mesh.bindposes[i]);
                        continue;
                    }
                }

                if (bone)
                {
                    var origin = mesh.bindposeCount == 0 && rootBone ? rootBone : original.transform;
                    var originToWorld = context.ObserveTransformPosition(origin).localToWorldMatrix;
                    var worldToBone = context.ObserveTransformPosition(bone).worldToLocalMatrix;
                    bindposes.Add(worldToBone * originToWorld);
                }
                else
                {
                    bindposes.Add(Matrix4x4.identity);
                }
            }

            var processors = weights
                .Select(x => BoneWeightProcessor.Create(x.bone, x.weight, bindposes[boneIndices[x.bone]]))
                .ToArray();
            foreach (var processor in processors)
            {
                processor.Prepare(new()
                {
                    OriginalRenderer = original,
                    ProxyRenderer = proxy,
                    ComputeContext = context,
                    VertexCount = vertexCount,
                    SubMeshCount = subMeshCount,
                    VertexPositions = readonlyVertexPositions,
                    VertexUvs = readonlyVertexUvs,
                    SubMeshMasks = readonlySubMeshMasks,
                });
            }

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
                    var bone = boneWeight.boneIndex < existingBoneCount ? bones[boneWeight.boneIndex] : null;
                    if (bone)
                    {
                        boneWeightBuffer[bone] = boneWeight;
                    }
                }

                foreach (var processor in processors)
                {
                    var boneWeight = boneWeightBuffer.TryGetValue(processor.Bone, out var x) ? x : new()
                    {
                        boneIndex = boneIndices[processor.Bone],
                        weight = 0.0f,
                    };
                    processor.Process(new()
                    {
                        OriginalRenderer = original,
                        ProxyRenderer = proxy,
                        ComputeContext = context,
                        VertexCount = vertexCount,
                        SubMeshCount = subMeshCount,
                        VertexPositions = readonlyVertexPositions,
                        VertexUvs = readonlyVertexUvs,
                        SubMeshMasks = readonlySubMeshMasks,
                    }, i, ref boneWeight);
                    boneWeightBuffer[processor.Bone] = boneWeight;
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

            foreach (var processor in processors)
            {
                processor.Dispose();
            }

            if (fallbackBoneIndex == extendedBoneCount)
            {
                bones.Add(context.ObserveTransformPosition(fallbackBone));
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

            switch (proxy)
            {
                case MeshRenderer when proxy.TryGetComponent<MeshFilter>(out var meshFilter):
                    var temp = new GameObject().AddComponent<SkinnedMeshRenderer>();
                    temp.transform.SetParent(proxy.transform, false);
                    temp.sharedMesh = mesh;
                    temp.bones = bones.ToArray();
                    temp.BakeMesh(baked = new(), true);
                    meshFilter.sharedMesh = baked;
                    Object.DestroyImmediate(mesh);
                    Object.DestroyImmediate(temp.gameObject);
                    break;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.sharedMesh = mesh;
                    skinnedMeshRenderer.bones = bones.ToArray();
                    break;
            }
        }
    }
}
