using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal abstract class BoneWeightProcessor : IDisposable
    {
        private static readonly Dictionary<Type, Func<Transform, IBoneWeight, BoneWeightModifierProcessor.Context, BoneWeightProcessor>> s_Creators = new();

        protected static void Register<T>(Func<Transform, T, BoneWeightModifierProcessor.Context, BoneWeightProcessor> creator) where T : IBoneWeight
        {
            s_Creators[typeof(T)] = (bone, weight, context) => creator(bone, (T)weight, context);
        }

        public static BoneWeightProcessor Create(Transform bone, IBoneWeight weight, BoneWeightModifierProcessor.Context context)
        {
            return s_Creators[weight.GetType()].Invoke(bone, weight, context);
        }

        public abstract void Process(int index, ref BoneWeight1 result);

        public abstract void Dispose();
    }

    internal abstract class BoneWeightProcessor<T> : BoneWeightProcessor where T : IBoneWeight
    {
        protected readonly Transform Bone = null;
        protected readonly T Weight = default;
        protected readonly BoneWeightModifierProcessor.Context Context = default;

        protected static void Register(Func<Transform, T, BoneWeightModifierProcessor.Context, BoneWeightProcessor<T>> creator)
        {
            Register<T>(creator);
        }

        protected BoneWeightProcessor(Transform bone, T weight, BoneWeightModifierProcessor.Context context)
        {
            Bone = bone;
            Weight = weight;
            Context = context;
        }
    }
}
