using UnityEngine;
using nadena.dev.ndmf.preview;

namespace net.nekobako.BoneWeightModifier.Editor
{
    internal static class PreviewUtils
    {
        public static bool TryGetComponent<T>(this ComputeContext context, GameObject gameObject, out T result) where T : Component
        {
            if (!gameObject)
            {
                result = null;
                return false;
            }

            context.GetComponent<T>(gameObject);
            return gameObject.TryGetComponent(out result);
        }

        public static bool TryGetComponent<T>(this ComputeContext context, Component component, out T result) where T : Component
        {
            if (!component)
            {
                result = null;
                return false;
            }

            context.GetComponent<T>(component.gameObject);
            return component.TryGetComponent(out result);
        }
    }
}
