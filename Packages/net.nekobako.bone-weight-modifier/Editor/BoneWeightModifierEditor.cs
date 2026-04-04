using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    [CustomEditor(typeof(BoneWeightModifier))]
    internal class BoneWeightModifierEditor : UnityEditor.Editor
    {
        private static readonly Lazy<RectOffset> s_HeaderPadding = new(() => new(15, 1, 0, 0));

        private SerializedProperty m_BoneProperty = null;
        private SerializedProperty m_RendererProperty = null;
        private SerializedProperty m_WeightsProperty = null;
        private ReorderableList m_ReorderableList = null;
        private Dictionary<string, BoneWeightDrawer> m_WeightDrawers = null;

        private void OnEnable()
        {
            m_BoneProperty = serializedObject.FindProperty(nameof(BoneWeightModifier.Bone));
            m_RendererProperty = serializedObject.FindProperty(nameof(BoneWeightModifier.Renderer));
            m_WeightsProperty = serializedObject.FindProperty(nameof(BoneWeightModifier.Weights));
            m_ReorderableList = new(serializedObject, m_WeightsProperty)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = ElementHeight,
                onAddCallback = OnAdd,
                onRemoveCallback = OnRemove,
            };
            m_WeightDrawers = new();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CL4EE.DrawLanguagePicker();

            GUIUtils.Space();

            EditorGUILayout.PropertyField(m_BoneProperty, GUIUtils.TrText("bone"), true);

            EditorGUILayout.PropertyField(m_RendererProperty, GUIUtils.TrText("renderer"), true);

            GUIUtils.Space();
            m_ReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();

            for (var i = 0; i < m_WeightsProperty.arraySize; i++)
            {
                GetWeightDrawer(i).DrawSceneGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            foreach (var drawer in m_WeightDrawers.Values)
            {
                drawer.Dispose();
            }
            m_WeightDrawers.Clear();
        }

        private void DrawHeader(Rect rect)
        {
            rect = s_HeaderPadding.Value.Remove(rect);
            EditorGUI.LabelField(rect, CL4EE.Tr("weight"));
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            rect.y = rect.center.y - GetWeightDrawer(index).CalcInspectorHeight() * 0.5f;
            rect.height = GetWeightDrawer(index).CalcInspectorHeight();
            GetWeightDrawer(index).DrawInspectorGUI(rect);
        }

        private float ElementHeight(int index)
        {
            return GetWeightDrawer(index).CalcInspectorHeight() + EditorGUIUtility.standardVerticalSpacing;
        }

        private void OnAdd(ReorderableList list)
        {
            var newWeightIndex = m_WeightsProperty.arraySize;
            m_WeightsProperty.InsertArrayElementAtIndex(newWeightIndex);

            var newWeightProperty = m_WeightsProperty.GetArrayElementAtIndex(newWeightIndex);
            newWeightProperty.managedReferenceValue = CreateWeight();

            m_ReorderableList.Select(newWeightIndex);
        }

        private void OnRemove(ReorderableList list)
        {
            var selectedWeightIndex = m_ReorderableList.GetSelectedIndex();
            if (selectedWeightIndex >= 0 && selectedWeightIndex < m_WeightsProperty.arraySize)
            {
                m_WeightsProperty.DeleteArrayElementAtIndex(selectedWeightIndex);
            }
            else
            {
                m_WeightsProperty.DeleteArrayElementAtIndex(m_WeightsProperty.arraySize - 1);
            }

            if (m_WeightsProperty.arraySize == 0)
            {
                m_ReorderableList.ClearSelection();
            }
            else if (selectedWeightIndex > m_WeightsProperty.arraySize - 1)
            {
                m_ReorderableList.Select(m_WeightsProperty.arraySize - 1);
            }
        }

        private IBoneWeight CreateWeight()
        {
            return new BoneWeightByVolume();
        }

        private BoneWeightDrawer GetWeightDrawer(int index)
        {
            var weightProperty = m_WeightsProperty.GetArrayElementAtIndex(index);
            if (m_WeightDrawers.TryGetValue(weightProperty.propertyPath, out var drawer) && drawer.IsValid(weightProperty))
            {
                return drawer;
            }

            drawer?.Dispose();
            drawer = BoneWeightDrawer.Create(weightProperty);
            return m_WeightDrawers[weightProperty.propertyPath] = drawer;
        }
    }
}
