using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal abstract class BoneWeightDrawer : IDisposable
    {
        private static readonly Dictionary<Type, Func<SerializedProperty, BoneWeightDrawer>> s_Creators = new();
        private static readonly (Type type, string name)[] s_TypeNames = TypeCache.GetTypesDerivedFrom<IBoneWeight>()
            .Select(x => (type: x, attribute: x.GetCustomAttribute<BoneWeightAttribute>()))
            .Where(x => x.type.IsClass && !x.type.IsAbstract && x.attribute != null)
            .OrderBy(x => x.attribute.Type)
            .Select(x => (x.type, x.attribute.Name))
            .ToArray();
        private static readonly Type[] s_Types = s_TypeNames
            .Select(x => x.type)
            .ToArray();
        private static readonly string[] s_Names = s_TypeNames
            .Select(x => x.name)
            .ToArray();

        private readonly IBoneWeight m_Weight = null;
        private readonly SerializedProperty m_Property = null;

        protected static void Register<T>(Func<SerializedProperty, BoneWeightDrawer> creator) where T : IBoneWeight
        {
            s_Creators[typeof(T)] = creator;
        }

        public static BoneWeightDrawer Create(SerializedProperty property)
        {
            return s_Creators[property.managedReferenceValue.GetType()].Invoke(property);
        }

        protected BoneWeightDrawer(SerializedProperty property)
        {
            m_Weight = property.managedReferenceValue as IBoneWeight;
            m_Property = property;
        }

        public bool IsValid(SerializedProperty property)
        {
            return ReferenceEquals(m_Weight, property.managedReferenceValue) && SerializedProperty.EqualContents(m_Property, property);
        }

        public void DrawInspectorGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();

            rect = GUIUtils.Line(rect, true);
            EditorGUI.BeginProperty(rect, GUIContent.none, m_Property);

            var type = EditorGUI.Popup(rect, Array.IndexOf(s_Types, m_Property.managedReferenceValue.GetType()), s_Names);

            EditorGUI.EndProperty();

            if (EditorGUI.EndChangeCheck())
            {
                m_Property.managedReferenceValue = Activator.CreateInstance(s_Types[type]);
                return;
            }

            rect = GUIUtils.Line(rect, OnCalcInspectorHeight());
            OnDrawInspectorGUI(rect);
        }

        public float CalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, true);

            rect = GUIUtils.Line(rect, OnCalcInspectorHeight());

            return rect.yMax;
        }

        public void DrawSceneGUI()
        {
            OnDrawSceneGUI();
        }

        public void Dispose()
        {
            OnDispose();
        }

        protected abstract void OnDrawInspectorGUI(Rect rect);
        protected abstract float OnCalcInspectorHeight();
        protected abstract void OnDrawSceneGUI();
        protected abstract void OnDispose();
    }

    internal abstract class BoneWeightDrawer<T> : BoneWeightDrawer where T : IBoneWeight
    {
        protected static void Register(Func<SerializedProperty, BoneWeightDrawer<T>> creator)
        {
            Register<T>(creator);
        }

        protected BoneWeightDrawer(SerializedProperty property) : base(property)
        {
        }
    }
}
