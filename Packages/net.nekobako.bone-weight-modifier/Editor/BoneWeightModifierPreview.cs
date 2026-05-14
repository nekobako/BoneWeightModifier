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
                .Where(x => context.Observe(x, y => y.Renderer) && context.GetSharedMesh(x.Renderer))
                .GroupBy(x => x.Renderer)
                .Select(x => RenderGroup.For(x.Key).WithData(x.ToArray(), Enumerable.SequenceEqual))
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

            private readonly ComputeContext m_MeshContext = null;
            private readonly Transform[] m_MeshContextBones = null;
            private readonly Transform m_MeshContextRootBone = null;
            private readonly BoneWeightModifier[] m_Modifiers = null;
            private readonly Mesh m_Mesh = null;
            private readonly Transform[] m_Bones = null;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Mesh;

            public Node(IEnumerable<(Renderer, Renderer)> pairs, BoneWeightModifier[] modifiers, ComputeContext context)
            {
                var (original, proxy) = pairs.Single();
                m_MeshContext = new(k_MeshContextDescription);
                m_MeshContextBones = RendererUtils.GetBones(proxy);
                m_MeshContextRootBone = RendererUtils.GetRootBone(proxy);
                BoneWeightModifierProcessor.Process(original, proxy, modifiers, m_MeshContext);

                m_Modifiers = modifiers;
                m_Mesh = RendererUtils.GetSharedMesh(proxy);
                m_Bones = RendererUtils.GetBones(proxy);

                m_MeshContext.Invalidates(context);
            }

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> pairs, ComputeContext context, RenderAspects aspects)
            {
                var (_, proxy) = pairs.Single();
                if (m_MeshContext.IsInvalidated ||
                    aspects.HasFlag(RenderAspects.Mesh) ||
                    aspects.HasFlag(RenderAspects.Shapes) && RendererUtils.GetRootBone(proxy) != m_MeshContextRootBone ||
                    aspects.HasFlag(RenderAspects.Shapes) && !RendererUtils.GetBones(proxy).SequenceEqual(m_MeshContextBones))
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
                RendererUtils.SetSharedMesh(proxy, m_Mesh);
                RendererUtils.SetBones(proxy, m_Bones);
            }

            public void Dispose()
            {
                m_MeshContext.Invalidate();

                Object.DestroyImmediate(m_Mesh);
            }
        }
    }
}
