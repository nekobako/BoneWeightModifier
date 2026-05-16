using System;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    internal static class RendererUtils
    {
        public static Mesh GetSharedMesh(Renderer renderer)
        {
            if (!renderer)
            {
                return null;
            }

            return renderer switch
            {
                MeshRenderer when renderer.TryGetComponent<MeshFilter>(out var meshFilter) => meshFilter.sharedMesh,
                SkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.sharedMesh,
                _ => null,
            };
        }

        public static void SetSharedMesh(Renderer renderer, Mesh mesh)
        {
            if (!renderer)
            {
                return;
            }

            switch (renderer)
            {
                case MeshRenderer when renderer.TryGetComponent<MeshFilter>(out var meshFilter):
                    meshFilter.sharedMesh = mesh;
                    return;
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.sharedMesh = mesh;
                    return;
            }
        }

        public static Transform[] GetBones(Renderer renderer)
        {
            if (!renderer)
            {
                return Array.Empty<Transform>();
            }

            return renderer switch
            {
                SkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.bones,
                _ => Array.Empty<Transform>(),
            };
        }

        public static void SetBones(Renderer renderer, Transform[] bones)
        {
            if (!renderer)
            {
                return;
            }

            switch (renderer)
            {
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.bones = bones;
                    return;
            }
        }

        public static Transform GetRootBone(Renderer renderer)
        {
            if (!renderer)
            {
                return null;
            }

            return renderer switch
            {
                SkinnedMeshRenderer skinnedMeshRenderer => skinnedMeshRenderer.rootBone,
                _ => null,
            };
        }

        public static void SetRootBone(Renderer renderer, Transform bone)
        {
            if (!renderer)
            {
                return;
            }

            switch (renderer)
            {
                case SkinnedMeshRenderer skinnedMeshRenderer:
                    skinnedMeshRenderer.rootBone = bone;
                    return;
            }
        }
    }
}
