using UnityEditor;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightByMaskDrawer : BoneWeightDrawer<BoneWeightByMask>
    {
        public const string MaskTextureEditorToken = "net.nekobako.bone-weight-modifier.bone-weight-by-mask-drawer";
        private const float k_MaskButtonWidth = 50.0f;
        private const float k_MaskButtonSpacing = 2.0f;
        private static readonly Vector2Int s_DefaultMaskSize = new(1024, 1024);
        private static readonly Color s_DefaultMaskColor = Color.white;

        private readonly SerializedProperty m_SlotProperty = null;
        private readonly SerializedProperty m_MaskProperty = null;
        private readonly SerializedProperty m_WeightProperty = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(x => new BoneWeightByMaskDrawer(x));
        }

        private BoneWeightByMaskDrawer(SerializedProperty property) : base(property)
        {
            m_SlotProperty = property.FindPropertyRelative(nameof(BoneWeightByMask.Slot));
            m_MaskProperty = property.FindPropertyRelative(nameof(BoneWeightByMask.Mask));
            m_WeightProperty = property.FindPropertyRelative(nameof(BoneWeightByMask.Weight));
        }

        protected override void OnDrawInspectorGUI(Rect rect)
        {
            EditorGUI.BeginDisabledGroup(IsOpenMaskTextureEditor());

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_SlotProperty, GUIUtils.TrText("mask-slot"), true), true);
            var propertyContent = EditorGUI.BeginProperty(rect, GUIUtils.TrText("mask-slot"), m_SlotProperty);

            var propertyRect = EditorGUI.PrefixLabel(rect, propertyContent);
            if (EditorGUI.DropdownButton(propertyRect, GUIUtils.Text($"Element {m_SlotProperty.intValue}"), FocusType.Keyboard))
            {
                var menu = new GenericMenu();

                if (m_SlotProperty.serializedObject.targetObject is BoneWeightModifier modifier && modifier.Renderer)
                {
                    for (var i = 0; i < modifier.Renderer.sharedMaterials.Length; i++)
                    {
                        var slot = i;
                        menu.AddItem(new($"Element {slot}"), m_SlotProperty.intValue == slot, () => SetSlot(slot));
                    }
                }

                menu.DropDown(propertyRect);

                void SetSlot(int slot)
                {
                    m_SlotProperty.serializedObject.Update();

                    m_SlotProperty.intValue = slot;

                    m_SlotProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_MaskProperty, GUIUtils.TrText("mask-mask"), true));
            EditorGUI.PropertyField(Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax - k_MaskButtonWidth - k_MaskButtonSpacing, rect.yMax), m_MaskProperty, GUIUtils.TrText("mask-mask"), true);

            EditorGUI.EndDisabledGroup();

            DrawMaskButton(Rect.MinMaxRect(rect.xMax - k_MaskButtonWidth, rect.yMin, rect.xMax, rect.yMax));

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, GUIUtils.TrText("mask-weight"), true));
            EditorGUI.PropertyField(rect, m_WeightProperty, GUIUtils.TrText("mask-weight"), true);
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_SlotProperty, GUIUtils.TrText("mask-slot"), true), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_MaskProperty, GUIUtils.TrText("mask-mask"), true));

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, GUIUtils.TrText("mask-weight"), true));

            return rect.yMax;
        }

        protected override void OnDrawSceneGUI()
        {
        }

        protected override void OnDispose()
        {
        }

        private bool IsOpenMaskTextureEditor()
        {
#if BWM_MASK_TEXTURE_EDITOR
            return m_MaskProperty.objectReferenceValue is Texture2D mask
                && m_MaskProperty.serializedObject.targetObject is BoneWeightModifier modifier
                && MaskTextureEditor.Editor.Window.IsOpenFor(mask, modifier.Renderer, m_SlotProperty.intValue, MaskTextureEditorToken);
#else
            return false;
#endif
        }

        private void DrawMaskButton(Rect rect)
        {
            if (m_MaskProperty.objectReferenceValue is not Texture2D mask)
            {
                DrawCreateMaskButton(rect);
            }
            else
            {
                DrawEditMaskButton(rect, mask);
            }
        }

        private void DrawCreateMaskButton(Rect rect)
        {
            if (GUI.Button(rect, GUIUtils.TrText("mask-create")))
            {
#if BWM_MASK_TEXTURE_EDITOR
                var mask = MaskTextureEditor.Editor.Utility.CreateTexture(s_DefaultMaskSize, s_DefaultMaskColor);

                m_MaskProperty.serializedObject.Update();

                m_MaskProperty.objectReferenceValue = mask;

                m_MaskProperty.serializedObject.ApplyModifiedProperties();

                if (mask && m_MaskProperty.serializedObject.targetObject is BoneWeightModifier modifier)
                {
                    MaskTextureEditor.Editor.Window.TryOpen(mask, modifier.Renderer, m_SlotProperty.intValue, MaskTextureEditorToken);
                }
#else
                ShowMaskTextureEditorDialog();
#endif

                // Exit GUI to avoid "InvalidOperationException: Stack empty."
                GUIUtility.ExitGUI();
            }
        }

        private void DrawEditMaskButton(Rect rect, Texture2D mask)
        {
            EditorGUI.BeginChangeCheck();

            var open = GUI.Toggle(rect, IsOpenMaskTextureEditor(), GUIUtils.TrText("mask-edit"), GUI.skin.button);

            if (EditorGUI.EndChangeCheck())
            {
#if BWM_MASK_TEXTURE_EDITOR
                if (open && m_MaskProperty.serializedObject.targetObject is BoneWeightModifier modifier)
                {
                    MaskTextureEditor.Editor.Window.TryOpen(mask, modifier.Renderer, m_SlotProperty.intValue, MaskTextureEditorToken);
                }
                else
                {
                    MaskTextureEditor.Editor.Window.TryClose();
                }
#else
                ShowMaskTextureEditorDialog();
#endif

                // Exit GUI to avoid "InvalidOperationException: Stack empty."
                GUIUtility.ExitGUI();
            }
        }

        private void ShowMaskTextureEditorDialog()
        {
            switch (EditorUtility.DisplayDialogComplex(
                CL4EE.Tr("mask-texture-editor-dialog-title"),
                CL4EE.Tr("mask-texture-editor-dialog-message"),
                CL4EE.Tr("mask-texture-editor-dialog-open-vcc"),
                CL4EE.Tr("mask-texture-editor-dialog-cancel"),
                CL4EE.Tr("mask-texture-editor-dialog-open-web")))
            {
                case 0:
                    Application.OpenURL("vcc://vpm/addRepo?url=https://vpm.nekobako.net/index.json");
                    break;
                case 2:
                    Application.OpenURL("https://github.com/nekobako/MaskTextureEditor");
                    break;
            }
        }
    }
}
