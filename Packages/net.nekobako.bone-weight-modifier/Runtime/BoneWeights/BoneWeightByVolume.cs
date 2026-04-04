using System;
using UnityEngine;
using UnityEngine.Animations;

namespace net.nekobako.BoneWeightModifier.Runtime
{
    [Serializable, BoneWeight(BoneWeightType.Volume, "Volume")]
    internal class BoneWeightByVolume : IBoneWeight
    {
        [SerializeField, NotKeyable]
        public Vector3 Position = Vector3.zero;

        [SerializeField, NotKeyable, Min(0.0f)]
        public float Radius = 0.1f;

        [SerializeField, NotKeyable, Range(0.0f, 1.0f)]
        public float Hardness = 0.0f;

        [SerializeField, NotKeyable, Range(0.0f, 1.0f)]
        public float Strength = 1.0f;

        [SerializeField, NotKeyable, Range(0.0f, 1.0f)]
        public float Weight = 1.0f;

        public IBoneWeight Clone()
        {
            return new BoneWeightByVolume
            {
                Position = Position,
                Radius = Radius,
                Hardness = Hardness,
                Strength = Strength,
                Weight = Weight,
            };
        }

        public bool Equals(IBoneWeight other)
        {
            return other is BoneWeightByVolume weight
                && Position.Equals(weight.Position)
                && Radius.Equals(weight.Radius)
                && Hardness.Equals(weight.Hardness)
                && Strength.Equals(weight.Strength)
                && Weight.Equals(weight.Weight);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IBoneWeight);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            hash = HashCode.Combine(hash, Position);
            hash = HashCode.Combine(hash, Radius);
            hash = HashCode.Combine(hash, Hardness);
            hash = HashCode.Combine(hash, Strength);
            hash = HashCode.Combine(hash, Weight);
            return hash;
        }
    }
}
