using System;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace net.nekobako.BoneWeightModifier.Editor
{
    internal static class PreviewUtils
    {
        public static bool TryGetComponent<T>(this ComputeContext context, GameObject gameObject, out T result) where T : Component
        {
            if (!gameObject)
            {
                result = null;
                return false;
            }

            context.GetComponent<T>(gameObject);
            return gameObject.TryGetComponent(out result);
        }

        public static bool TryGetComponent<T>(this ComputeContext context, Component component, out T result) where T : Component
        {
            if (!component)
            {
                result = null;
                return false;
            }

            context.GetComponent<T>(component.gameObject);
            return component.TryGetComponent(out result);
        }

        public static Mesh GetSharedMesh(this ComputeContext context, Renderer renderer)
        {
            if (!renderer)
            {
                return null;
            }

            return renderer switch
            {
                MeshRenderer when context.TryGetComponent<MeshFilter>(renderer, out var meshFilter) => context.Observe(meshFilter, x => x.sharedMesh),
                SkinnedMeshRenderer skinnedMeshRenderer => context.Observe(skinnedMeshRenderer, y => y.sharedMesh),
                _ => null,
            };
        }

        public static Transform[] GetBones(this ComputeContext context, Renderer renderer)
        {
            if (!renderer)
            {
                return Array.Empty<Transform>();
            }

            return renderer switch
            {
                SkinnedMeshRenderer skinnedMeshRenderer => context.Observe(skinnedMeshRenderer, y => y.bones),
                _ => Array.Empty<Transform>(),
            };
        }

        public static Transform GetRootBone(this ComputeContext context, Renderer renderer)
        {
            if (!renderer)
            {
                return null;
            }

            return renderer switch
            {
                SkinnedMeshRenderer skinnedMeshRenderer => context.Observe(skinnedMeshRenderer, y => y.rootBone),
                _ => null,
            };
        }
    }
}
