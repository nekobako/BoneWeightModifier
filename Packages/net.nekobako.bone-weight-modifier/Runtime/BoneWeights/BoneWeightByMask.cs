using System;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BoneWeightModifier.Runtime
{
    [Serializable, BoneWeight(BoneWeightType.Mask, "Mask")]
    internal class BoneWeightByMask : IBoneWeight
    {
        [SerializeField, NotKeyable]
        public int Slot = 0;

        [SerializeField, NotKeyable]
        public Texture2D Mask = null;

        [SerializeField, NotKeyable]
        public float Weight = 1.0f;

        public IBoneWeight Clone()
        {
            return new BoneWeightByMask
            {
                Slot = Slot,
                Mask = Mask,
                Weight = Weight,
            };
        }

        public bool Equals(IBoneWeight other)
        {
            return other is BoneWeightByMask weight
                && Slot.Equals(weight.Slot)
                && Mask.Equals(weight.Mask)
                && Weight.Equals(weight.Weight);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IBoneWeight);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Slot);
            hash = HashCode.Combine(hash, Mask);
            hash = HashCode.Combine(hash, Weight);
            return hash;
        }
    }
}
