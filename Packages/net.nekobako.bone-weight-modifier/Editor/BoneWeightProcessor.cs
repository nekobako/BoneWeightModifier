using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal abstract class BoneWeightProcessor : IDisposable
    {
        public readonly Transform Bone = null;
        public readonly IBoneWeight Weight = null;
        public readonly Matrix4x4 Bindpose = default;

        private static readonly Dictionary<Type, Func<Transform, IBoneWeight, Matrix4x4, BoneWeightProcessor>> s_Creators = new();

        protected static void Register<T>(Func<Transform, T, Matrix4x4, BoneWeightProcessor> creator) where T : IBoneWeight
        {
            s_Creators[typeof(T)] = (bone, weight, bindpose) => creator(bone, (T)weight, bindpose);
        }

        public static BoneWeightProcessor Create(Transform bone, IBoneWeight weight, Matrix4x4 bindpose)
        {
            return s_Creators[weight.GetType()].Invoke(bone, weight, bindpose);
        }

        protected BoneWeightProcessor(Transform bone, IBoneWeight weight, Matrix4x4 bindpose)
        {
            Bone = bone;
            Weight = weight;
            Bindpose = bindpose;
        }

        public abstract void Prepare(BoneWeightModifierProcessor.Context context);
        public abstract void Process(BoneWeightModifierProcessor.Context context, int index, ref BoneWeight1 result);
        public abstract void Dispose();
    }

    internal abstract class BoneWeightProcessor<T> : BoneWeightProcessor where T : IBoneWeight
    {
        protected new readonly T Weight = default;

        protected static void Register(Func<Transform, T, Matrix4x4, BoneWeightProcessor<T>> creator)
        {
            Register<T>(creator);
        }

        protected BoneWeightProcessor(Transform bone, T weight, Matrix4x4 bindpose) : base(bone, weight, bindpose)
        {
            Weight = weight;
        }
    }
}
