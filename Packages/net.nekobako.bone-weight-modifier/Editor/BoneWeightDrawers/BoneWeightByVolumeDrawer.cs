using UnityEditor;
using UnityEngine;

namespace net.nekobako.BoneWeightModifier.Editor
{
    using Runtime;

    internal class BoneWeightByVolumeDrawer : BoneWeightDrawer<BoneWeightByVolume>
    {
        private const float k_BackHandleAlphaMultiplier = 0.2f;
        private static readonly Color s_HandleColor = new(1.0f, 1.0f, 1.0f, 0.4f);
        private static readonly Color s_ActiveHandleColor = new(1.0f, 1.0f, 1.0f, 0.8f);
        private static readonly Color s_InactiveHandleColor = new(1.0f, 1.0f, 1.0f, 0.2f);
        private static BoneWeightByVolumeDrawer s_EditingDrawer = null;

        private readonly SerializedProperty m_Property = null;
        private readonly SerializedProperty m_PositionProperty = null;
        private readonly SerializedProperty m_RadiusProperty = null;
        private readonly SerializedProperty m_HardnessProperty = null;
        private readonly SerializedProperty m_StrengthProperty = null;
        private readonly SerializedProperty m_WeightProperty = null;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Register(x => new BoneWeightByVolumeDrawer(x));
        }

        private static bool IsEditing()
        {
            return s_EditingDrawer != null;
        }

        private static bool IsEditing(BoneWeightByVolumeDrawer drawer)
        {
            return s_EditingDrawer == drawer;
        }

        private static void SetEditing(BoneWeightByVolumeDrawer drawer)
        {
            s_EditingDrawer = drawer;

            Tools.hidden = drawer != null;
            SceneView.RepaintAll();
        }

        private BoneWeightByVolumeDrawer(SerializedProperty property) : base(property)
        {
            m_Property = property;
            m_PositionProperty = property.FindPropertyRelative(nameof(BoneWeightByVolume.Position));
            m_RadiusProperty = property.FindPropertyRelative(nameof(BoneWeightByVolume.Radius));
            m_HardnessProperty = property.FindPropertyRelative(nameof(BoneWeightByVolume.Hardness));
            m_StrengthProperty = property.FindPropertyRelative(nameof(BoneWeightByVolume.Strength));
            m_WeightProperty = property.FindPropertyRelative(nameof(BoneWeightByVolume.Weight));

            SceneView.RepaintAll();
        }

        protected override void OnDrawInspectorGUI(Rect rect)
        {
            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_PositionProperty, GUIUtils.TrText("volume-position"), true), true);
            EditorGUI.PropertyField(rect, m_PositionProperty, GUIUtils.TrText("volume-position"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_RadiusProperty, GUIUtils.TrText("volume-radius"), true));
            EditorGUI.PropertyField(rect, m_RadiusProperty, GUIUtils.TrText("volume-radius"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_HardnessProperty, GUIUtils.TrText("volume-hardness"), true));
            EditorGUI.PropertyField(rect, m_HardnessProperty, GUIUtils.TrText("volume-hardness"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_StrengthProperty, GUIUtils.TrText("volume-strength"), true));
            EditorGUI.PropertyField(rect, m_StrengthProperty, GUIUtils.TrText("volume-strength"), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, GUIUtils.TrText("volume-weight"), true));
            EditorGUI.PropertyField(rect, m_WeightProperty, GUIUtils.TrText("volume-weight"), true);

            rect = GUIUtils.Line(rect);
            DrawEditButton(rect);
        }

        protected override float OnCalcInspectorHeight()
        {
            var rect = GUIUtils.Line(default, EditorGUI.GetPropertyHeight(m_PositionProperty, GUIUtils.TrText("volume-position"), true), true);

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_RadiusProperty, GUIUtils.TrText("volume-radius"), true));

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_HardnessProperty, GUIUtils.TrText("volume-hardness"), true));

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_StrengthProperty, GUIUtils.TrText("volume-strength"), true));

            rect = GUIUtils.Line(rect, EditorGUI.GetPropertyHeight(m_WeightProperty, GUIUtils.TrText("volume-weight"), true));

            rect = GUIUtils.Line(rect);

            return rect.yMax;
        }

        protected override void OnDrawSceneGUI()
        {
            if (m_Property.serializedObject.targetObject is not BoneWeightModifier modifier)
            {
                return;
            }

            var bone = modifier.Bone ? modifier.Bone : modifier.transform;

            if (IsEditing(this))
            {
                EditorGUI.BeginChangeCheck();

                var position = Handles.PositionHandle(bone.TransformPoint(m_PositionProperty.vector3Value), Tools.pivotRotation == PivotRotation.Local ? bone.rotation : Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    m_PositionProperty.vector3Value = bone.InverseTransformPoint(position);
                }
            }

            using var scope = new Handles.DrawingScope(
                IsEditing(this) ? s_ActiveHandleColor :
                IsEditing() ? s_InactiveHandleColor :
                s_HandleColor,
                bone.localToWorldMatrix);

            DrawSphere(m_PositionProperty.vector3Value, Quaternion.identity, m_RadiusProperty.floatValue);

            if (m_HardnessProperty.floatValue < 1.0f)
            {
                DrawSphere(m_PositionProperty.vector3Value, Quaternion.identity, m_RadiusProperty.floatValue * m_HardnessProperty.floatValue);
            }

            if (IsEditing(this))
            {
                EditorGUI.BeginChangeCheck();

                var radius = Handles.RadiusHandle(Quaternion.identity, m_PositionProperty.vector3Value, m_RadiusProperty.floatValue, true);

                if (EditorGUI.EndChangeCheck())
                {
                    m_RadiusProperty.floatValue = radius;
                }
            }
        }

        protected override void OnDispose()
        {
            if (IsEditing(this))
            {
                SetEditing(null);
            }

            SceneView.RepaintAll();
        }

        private void DrawEditButton(Rect rect)
        {
            EditorGUI.BeginChangeCheck();

            var edit = GUI.Toggle(rect, IsEditing(this), GUIUtils.TrText("volume-edit"), GUI.skin.button);

            if (EditorGUI.EndChangeCheck())
            {
                SetEditing(edit ? this : null);
            }
        }

        private void DrawSphere(Vector3 position, Quaternion rotation, float radius)
        {
            var handlesColor = Handles.color;

            if (Camera.current.orthographic)
            {
                var forward = Handles.matrix.inverse.MultiplyVector(Camera.current.transform.forward).normalized;
                Handles.color = handlesColor;
                Handles.DrawWireDisc(position, forward, radius);

                DrawAxisDisc(rotation * Vector3.right);
                DrawAxisDisc(rotation * Vector3.up);
                DrawAxisDisc(rotation * Vector3.forward);

                void DrawAxisDisc(Vector3 axis)
                {
                    var from = Vector3.Cross(axis, forward).normalized;
                    Handles.color = handlesColor;
                    Handles.DrawWireArc(position, axis, from, +180.0f, radius);
                    Handles.color = new(handlesColor.r, handlesColor.g, handlesColor.b, handlesColor.a * k_BackHandleAlphaMultiplier);
                    Handles.DrawWireArc(position, axis, from, -180.0f, radius);
                }
            }
            else
            {
                var origin = Handles.matrix.inverse.MultiplyPoint(Camera.current.transform.position);
                var forward = (position - origin).normalized;
                var distance = (position - origin).magnitude;
                var sqrDistance = distance * distance;
                var sqrRadius = radius * radius;
                if (sqrDistance > sqrRadius)
                {
                    // カメラを点 C、球の中心を点 S、カメラから球の表面に引いた任意の接線の接点を点 T、点 T から直線 CS に引いた垂線の足を点 U とすると、
                    // △CST ∽ △TSU より、
                    //   ST / CS = SU / ST
                    // ここで、ST = radius、CS = distance、SU = offset とすると、
                    //   radius / distance = offset / radius
                    // よって、
                    //   offset = radius * radius / distance
                    var offset = sqrRadius / distance;
                    var sqrOffset = offset * offset;
                    Handles.color = handlesColor;
                    Handles.DrawWireDisc(position - forward * offset, forward, Mathf.Sqrt(sqrRadius - sqrOffset));
                }

                DrawAxisDisc(rotation * Vector3.right);
                DrawAxisDisc(rotation * Vector3.up);
                DrawAxisDisc(rotation * Vector3.forward);

                void DrawAxisDisc(Vector3 axis)
                {
                    // 点 S を含み axis 方向に垂直な平面を平面 A、点 U を含み forward 方向に垂直な平面を平面 F、点 S から平面 A と平面 F の交線に引いた垂線の足を点 V、axis と forward のなす角を θ とすると、
                    //   SU / SV = sin(∠SVU)
                    //     = sin(90° - ∠USV)
                    //     = sin(θ)
                    // よって、
                    //   SV = SU / sin(θ)
                    // ここで、平面 A と平面 F の交線と球の交点のうち 1 点を点 W とすると、
                    //   SV / SW = sin(∠SWV)
                    // よって、
                    //   SV = SW * sin(∠SWV)
                    // ゆえに、
                    //   SU / sin(θ) = SW * sin(∠SWV)
                    // したがって、
                    //   sin(∠SWV) = SU / SW / sin(θ)
                    //     = offset / radius / |axis × forward|
                    //     = radius / distance / |axis × forward|
                    var sin = radius / distance / Vector3.Cross(axis, forward).magnitude;
                    if (sqrDistance > sqrRadius && sin < 1.0f)
                    {
                        var angle = Mathf.Asin(sin) * Mathf.Rad2Deg;
                        var from = Quaternion.AngleAxis(angle, axis) * Vector3.Cross(axis, forward).normalized;
                        Handles.color = handlesColor;
                        Handles.DrawWireArc(position, axis, from, +180.0f - angle * 2.0f, radius);
                        Handles.color = new(handlesColor.r, handlesColor.g, handlesColor.b, handlesColor.a * k_BackHandleAlphaMultiplier);
                        Handles.DrawWireArc(position, axis, from, -180.0f - angle * 2.0f, radius);
                    }
                    else
                    {
                        Handles.color = new(handlesColor.r, handlesColor.g, handlesColor.b, handlesColor.a * k_BackHandleAlphaMultiplier);
                        Handles.DrawWireDisc(position, axis, radius);
                    }
                }
            }

            Handles.color = handlesColor;
        }
    }
}
