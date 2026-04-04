using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
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

        public static void Process(SkinnedMeshRenderer renderer, (Transform, IBoneWeight)[] weights)
        {
            var baked = new Mesh();
            renderer.BakeMesh(baked, true);

            var vertexCount = baked.vertexCount;
            var subMeshCount = baked.subMeshCount;
            using var vertexPositions = new NativeArray<Vector3>(baked.vertices, Allocator.Temp);
            using var vertexUvs = new NativeArray<Vector2>(baked.uv, Allocator.Temp);
            using var subMeshMasks = new NativeBitArray(vertexCount * subMeshCount, Allocator.Temp);
            for (var i = 0; i < subMeshCount; i++)
            {
                foreach (var index in baked.GetIndices(i))
                {
                    subMeshMasks.Set(subMeshCount * i + index, true);
                }
            }

            Object.DestroyImmediate(baked);

            var context = new Context
            {
                Renderer = renderer,
                VertexCount = vertexCount,
                SubMeshCount = subMeshCount,
                VertexPositions = vertexPositions.AsReadOnly(),
                VertexUvs = vertexUvs.AsReadOnly(),
                SubMeshMasks = subMeshMasks.AsReadOnly(),
            };

            var mesh = Object.Instantiate(renderer.sharedMesh);
            var bones = new List<Transform>(renderer.bones);
            var bindposes = new List<Matrix4x4>(mesh.bindposes);

            foreach (var (bone, _) in weights)
            {
                if (bones.Contains(bone))
                {
                    continue;
                }

                bones.Add(bone);
                bindposes.Add(bone.worldToLocalMatrix * renderer.transform.localToWorldMatrix);
            }

            using var boneWeights = mesh.GetAllBoneWeights();
            using var boneWeightCounts = mesh.GetBonesPerVertex();
            using var boneWeightsList = new NativeList<BoneWeight1>(boneWeights.Length, Allocator.Temp);
            using var boneWeightCountsList = new NativeList<byte>(boneWeightCounts.Length, Allocator.Temp);

            var boneWeightProcessors = weights
                .ToDictionary(x => x, x => BoneWeightProcessor.Create(x.Item1, x.Item2, context));
            var boneIndices = bones
                .Select((x, i) => (bone: x, index: i))
                .ToDictionary(x => x.bone, x => x.index);

            var boneWeightIndex = 0;
            var boneWeightBuffer = new Dictionary<Transform, BoneWeight1>(byte.MaxValue);
            var boneWeightBufferList = new List<BoneWeight1>(byte.MaxValue);
            for (var i = 0; i < boneWeightCounts.Length; i++)
            {
                foreach (var boneWeight in boneWeights.Slice(boneWeightIndex, boneWeightCounts[i]))
                {
                    boneWeightBuffer[bones[boneWeight.boneIndex]] = boneWeight;
                }

                foreach (var (bone, weight) in weights)
                {
                    var boneWeight = boneWeightBuffer.TryGetValue(bone, out var x) ? x : new()
                    {
                        boneIndex = boneIndices[bone],
                        weight = 0.0f,
                    };
                    boneWeightProcessors[(bone, weight)].Process(i, ref boneWeight);
                    boneWeightBuffer[bone] = boneWeight;
                }

                boneWeightBufferList.AddRange(boneWeightBuffer.Values.Where(x => x.weight > 0.0f));
                boneWeightBufferList.Sort((a, b) => b.weight.CompareTo(a.weight));

                if (boneWeightBufferList.Count == 0)
                {
                    if (bones[^1] != renderer.transform)
                    {
                        bones.Add(renderer.transform);
                        bindposes.Add(Matrix4x4.identity);
                    }

                    boneWeightBufferList.Add(new()
                    {
                        boneIndex = bones.Count - 1,
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

                boneWeightIndex += boneWeightCounts[i];
                boneWeightBuffer.Clear();
                boneWeightBufferList.Clear();
            }

            foreach (var boneWeightProcessor in boneWeightProcessors.Values)
            {
                boneWeightProcessor.Dispose();
            }

            using var boneWeightsArray = boneWeightsList.ToArray(Allocator.Temp);
            using var boneWeightCountsArray = boneWeightCountsList.ToArray(Allocator.Temp);
            mesh.SetBoneWeights(boneWeightCountsArray, boneWeightsArray);

            mesh.bindposes = bindposes.ToArray();
            renderer.bones = bones.ToArray();
            renderer.sharedMesh = mesh;
        }
    }
}
