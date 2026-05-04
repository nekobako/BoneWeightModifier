using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightModifierPreview : IRenderFilter
    {
        public static readonly TogglablePreviewNode PreviewNode = TogglablePreviewNode.Create(() => "Bone Weight Modifier", "net.nekobako.bone-weight-modifier", false);

        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(PreviewNode.IsEnabled);
        }

        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return PreviewNode;
        }

        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            return context.GetComponentsByType<BoneWeightModifier>()
                .Where(x =>
                    context.Observe(x, y => y.Renderer) && x.Renderer is MeshRenderer && context.TryGetComponent<MeshFilter>(x.Renderer, out var meshFilter) && context.Observe(meshFilter, y => y.sharedMesh) ||
                    context.Observe(x, y => y.Renderer) && x.Renderer is SkinnedMeshRenderer skinnedMeshRenderer && context.Observe(skinnedMeshRenderer, y => y.sharedMesh))
                .GroupBy(x => x.Renderer)
                .Select(x => RenderGroup.For(x.Key).WithData(x.ToArray()))
                .ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context)
        {
            var modifiers = group.GetData<BoneWeightModifier[]>();
            var node = new Node(pairs, modifiers, context);
            return Task.FromResult<IRenderFilterNode>(node);
        }

        private class Node : IRenderFilterNode
        {
            private const string k_MeshContextDescription = "BoneWeightModifierPreview.Node.MeshContext";

            private readonly BoneWeightModifier[] m_Modifiers = null;
            private readonly Mesh m_Mesh = null;
            private readonly Transform[] m_Bones = null;
            private readonly ComputeContext m_MeshContext = null;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Mesh;

            public Node(IEnumerable<(Renderer, Renderer)> pairs, BoneWeightModifier[] modifiers, ComputeContext context)
            {
                var (original, proxy) = pairs.Single();
                m_MeshContext = new(k_MeshContextDescription);
                BoneWeightModifierProcessor.Process(original, proxy, modifiers, m_MeshContext);

                m_Modifiers = modifiers;
                switch (proxy)
                {
                    case MeshRenderer when proxy.TryGetComponent<MeshFilter>(out var meshFilter):
                        m_Mesh = meshFilter.sharedMesh;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        m_Mesh = skinnedMeshRenderer.sharedMesh;
                        m_Bones = skinnedMeshRenderer.bones;
                        break;
                }

                m_MeshContext.Invalidates(context);
            }

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context, RenderAspects aspects)
            {
                if (aspects.HasFlag(RenderAspects.Mesh) || m_MeshContext.IsInvalidated)
                {
                    // Returning null here forcibly passes RenderAspects.Everything to Refresh() of downstream nodes
                    // return Task.FromResult<IRenderFilterNode>(null);

                    var node = new Node(pairs, m_Modifiers, context);
                    return Task.FromResult<IRenderFilterNode>(node);
                }

                WhatChanged = 0;

                m_MeshContext.Invalidates(context);

                return Task.FromResult<IRenderFilterNode>(this);
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                switch (proxy)
                {
                    case MeshRenderer when proxy.TryGetComponent<MeshFilter>(out var meshFilter):
                        meshFilter.sharedMesh = m_Mesh;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        skinnedMeshRenderer.sharedMesh = m_Mesh;
                        skinnedMeshRenderer.bones = m_Bones;
                        break;
                }
            }

            public void Dispose()
            {
                m_MeshContext.Invalidate();

                Object.DestroyImmediate(m_Mesh);
            }
        }
    }
}
