using UnityEditor;
using UnityEngine;
using nadena.dev.ndmf.runtime;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    [CustomEditor(typeof(BoneWeightBinder))]
    internal class BoneWeightBinderEditor : UnityEditor.Editor
    {
        private const float k_HandleSize = 4.0f;

        public override void OnInspectorGUI()
        {
            if (serializedObject.targetObject is not BoneWeightBinder binder)
            {
                return;
            }

            serializedObject.Update();

            CL4EE.DrawLanguagePicker();

            GUIUtils.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PrefixLabel(GUIUtils.TrText("preview"));
            var enable = GUILayout.Toolbar(BoneWeightModifierPreview.PreviewNode.IsEnabled.Value ? 1 : 0, new[] { CL4EE.Tr("disable-preview"), CL4EE.Tr("enable-preview") }) == 1;

            if (EditorGUI.EndChangeCheck())
            {
                BoneWeightModifierPreview.PreviewNode.IsEnabled.Value = enable;
            }

            EditorGUILayout.EndHorizontal();

            GUIUtils.Space();

            EditorGUI.BeginChangeCheck();

            var bind = GUILayout.Toggle(binder.IsBound, GUIUtils.TrText("bind-bone"), GUI.skin.button, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));

            if (EditorGUI.EndChangeCheck())
            {
                var root = RuntimeUtil.FindAvatarInParents(binder.transform);
                if (root)
                {
                    if (bind)
                    {
                        Undo.RecordObject(binder, "Bind Bone");
                        binder.IsBound = true;
                        binder.Bindpose = binder.transform.worldToLocalMatrix * root.localToWorldMatrix;
                    }
                    else
                    {
                        Undo.RecordObject(binder.transform, "Unbind Bone");
                        binder.transform.position = (binder.Bindpose * root.worldToLocalMatrix).inverse.MultiplyPoint(Vector3.zero);

                        Undo.RecordObject(binder, "Unbind Bone");
                        binder.IsBound = false;
                        binder.Bindpose = Matrix4x4.zero;
                    }
                }
                else
                {
                    Undo.RecordObject(binder, "Unbind Bone");
                    binder.IsBound = false;
                    binder.Bindpose = Matrix4x4.zero;
                }
            }

            if (binder.IsBound)
            {
                EditorGUI.BeginDisabledGroup(true);
                for (var i = 0; i < 4; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (var j = 0; j < 4; j++)
                    {
                        EditorGUILayout.FloatField(binder.Bindpose[i, j]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (serializedObject.targetObject is not BoneWeightBinder { IsBound: true } binder)
            {
                return;
            }

            var root = RuntimeUtil.FindAvatarInParents(binder.transform);
            if (root)
            {
                Handles.DrawDottedLine((binder.Bindpose * root.worldToLocalMatrix).inverse.MultiplyPoint(Vector3.zero), binder.transform.position, k_HandleSize);
            }
        }
    }
}
