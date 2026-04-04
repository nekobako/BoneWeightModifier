using System;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Runtime
{
    internal enum BoneWeightType
    {
        Volume,
        Mask,
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class BoneWeightAttribute : PropertyAttribute
    {
        public readonly BoneWeightType Type = 0;
        public readonly string Name = string.Empty;

        public BoneWeightAttribute(BoneWeightType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    internal interface IBoneWeight : IEquatable<IBoneWeight>
    {
        IBoneWeight Clone();
    }
}
