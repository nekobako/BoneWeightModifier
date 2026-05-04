using UnityEditor;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightByVolumeProcessor : BoneWeightProcessor<BoneWeightByVolume>
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register((bone, weight, bindpose) => new BoneWeightByVolumeProcessor(bone, weight, bindpose));
        }

        private BoneWeightByVolumeProcessor(Transform bone, BoneWeightByVolume weight, Matrix4x4 bindpose) : base(bone, weight, bindpose)
        {
        }

        public override void Prepare(BoneWeightModifierProcessor.Context context)
        {
        }

        public override void Process(BoneWeightModifierProcessor.Context context, int index, ref BoneWeight1 result)
        {
            var position = Bindpose.MultiplyPoint(context.VertexPositions[index]);
            var distance = Vector3.Distance(Weight.Position, position);
            var rate = InverseLerp(Weight.Radius * Weight.Hardness, Weight.Radius, distance);
            var strength = (1.0f - Mathf.SmoothStep(0.0f, 1.0f, rate)) * Weight.Strength;
            result.weight = Mathf.Lerp(result.weight, Weight.Weight, strength);
        }

        public override void Dispose()
        {
        }

        private float InverseLerp(float a, float b, float value)
        {
            var min = Mathf.Min(a, b);
            var max = Mathf.Max(a, b);
            return
                value < min ? 0.0f :
                value > max ? 1.0f :
                Mathf.InverseLerp(min, max, value);
        }
    }
}
