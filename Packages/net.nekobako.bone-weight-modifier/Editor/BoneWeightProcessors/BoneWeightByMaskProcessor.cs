using UnityEditor;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightByMaskProcessor : BoneWeightProcessor<BoneWeightByMask>
    {
        private readonly int m_Slot = 0;
        private readonly Texture2D m_Mask = null;
        private readonly Texture2D m_Temp = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register((bone, weight, context) => new BoneWeightByMaskProcessor(bone, weight, context));
        }

        private BoneWeightByMaskProcessor(Transform bone, BoneWeightByMask weight, BoneWeightModifierProcessor.Context context) : base(bone, weight, context)
        {
            m_Mask = Weight.Mask;
            m_Slot = Mathf.Min(Weight.Slot, Context.SubMeshCount);

            if (!m_Mask || m_Mask.isReadable)
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

        public override void Process(int index, ref BoneWeight1 result)
        {
            if (!m_Mask || !Context.SubMeshMasks.IsSet(Context.SubMeshCount * m_Slot + index))
            {
                return;
            }

            var uv = Context.VertexUvs[index];
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
