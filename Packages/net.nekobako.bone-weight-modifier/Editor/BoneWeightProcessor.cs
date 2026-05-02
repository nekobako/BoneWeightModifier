using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal abstract class BoneWeightProcessor : IDisposable
    {
        private static readonly Dictionary<Type, Func<Transform, IBoneWeight, Matrix4x4, BoneWeightModifierProcessor.Context, BoneWeightProcessor>> s_Creators = new();

        protected static void Register<T>(Func<Transform, T, Matrix4x4, BoneWeightModifierProcessor.Context, BoneWeightProcessor> creator) where T : IBoneWeight
        {
            s_Creators[typeof(T)] = (bone, weight, bindpose, context) => creator(bone, (T)weight, bindpose, context);
        }

        public static BoneWeightProcessor Create(Transform bone, IBoneWeight weight, Matrix4x4 bindpose, BoneWeightModifierProcessor.Context context)
        {
            return s_Creators[weight.GetType()].Invoke(bone, weight, bindpose, context);
        }

        public abstract void Process(int index, ref BoneWeight1 result);

        public abstract void Dispose();
    }

    internal abstract class BoneWeightProcessor<T> : BoneWeightProcessor where T : IBoneWeight
    {
        protected readonly Transform Bone = null;
        protected readonly T Weight = default;
        protected readonly Matrix4x4 Bindpose = default;
        protected readonly BoneWeightModifierProcessor.Context Context = default;

        protected static void Register(Func<Transform, T, Matrix4x4, BoneWeightModifierProcessor.Context, BoneWeightProcessor<T>> creator)
        {
            Register<T>(creator);
        }

        protected BoneWeightProcessor(Transform bone, T weight, Matrix4x4 bindpose, BoneWeightModifierProcessor.Context context)
        {
            Bone = bone;
            Weight = weight;
            Bindpose = bindpose;
            Context = context;
        }
    }
}
