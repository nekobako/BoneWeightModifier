using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CustomLocalization4EditorExtension;

namespace net.nekobako.BoneWeightModifier.Editor
{
    internal static class GUIUtils
    {
        private static readonly GUIContent s_Content = new();

        public static GUIContent Text(string text)
        {
            s_Content.text = text;
            s_Content.image = null;
            s_Content.tooltip = string.Empty;
            return s_Content;
        }

        public static GUIContent TrText(string key)
        {
            return Text(CL4EE.Tr(key));
        }

        public static void Space()
        {
            Space(EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight);
        }

        public static void Space(float height)
        {
            GUILayout.Space(height);
        }

        public static Rect Line(Rect rect, bool init = false)
        {
            return Line(rect, EditorGUIUtility.singleLineHeight, init);
        }

        public static Rect Line(Rect rect, float height, bool init = false)
        {
            if (!init)
            {
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
            rect.height = height;
            return rect;
        }

        public static int GetSelectedIndex(this ReorderableList list)
        {
            return list.selectedIndices.DefaultIfEmpty(-1).First();
        }
    }
}
