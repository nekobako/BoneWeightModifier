using UnityEditor;
using UnityEngine;
using nadena.dev.ndmf.preview;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightByMaskProcessor : BoneWeightProcessor<BoneWeightByMask>
    {
        private int m_Slot = 0;
        private Texture2D m_Mask = null;
        private Texture2D m_Temp = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register((bone, weight, bindpose) => new BoneWeightByMaskProcessor(bone, weight, bindpose));
        }

        private BoneWeightByMaskProcessor(Transform bone, BoneWeightByMask weight, Matrix4x4 bindpose) : base(bone, weight, bindpose)
        {
        }

        public override void Prepare(BoneWeightModifierProcessor.Context context)
        {
            m_Mask = Weight.Mask;
            m_Slot = Mathf.Min(Weight.Slot, context.SubMeshCount - 1);

            if (!m_Mask)
            {
                return;
            }

            context.ComputeContext.Observe(m_Mask, x => x.imageContentsHash);

#if BWM_MASK_TEXTURE_EDITOR
            var editing = MaskTextureEditor.Editor.Window.ObserveTextureFor(context.ComputeContext, m_Mask, context.OriginalRenderer, m_Slot,
                BoneWeightByMaskDrawer.MaskTextureEditorToken);
            if (editing)
            {
                m_Mask = editing;
            }
#endif

            if (m_Mask.isReadable)
            {
                return;
            }

            var rt = RenderTexture.GetTemporary(m_Mask.width, m_Mask.height);
            Graphics.Blit(m_Mask, rt);

            m_Mask = m_Temp = new(m_Mask.width, m_Mask.height)
            {
                filterMode = m_Mask.filterMode,
                wrapModeU = m_Mask.wrapModeU,
                wrapModeV = m_Mask.wrapModeV,
            };
            m_Mask.ReadPixels(new(0.0f, 0.0f, m_Mask.width, m_Mask.height), 0, 0);
            m_Mask.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
        }

        public override void Process(BoneWeightModifierProcessor.Context context, int index, ref BoneWeight1 result)
        {
            if (!m_Mask || !context.SubMeshMasks.IsSet(context.VertexCount * m_Slot + index))
            {
                return;
            }

            var uv = context.VertexUvs[index];
            var strength = m_Mask.GetPixel((int)(uv.x * m_Mask.width), (int)(uv.y * m_Mask.height)).r;
            result.weight = Mathf.Lerp(result.weight, Weight.Weight, strength);
        }

        public override void Dispose()
        {
            if (!m_Temp)
            {
                return;
            }

            Object.DestroyImmediate(m_Temp);
        }
    }
}
