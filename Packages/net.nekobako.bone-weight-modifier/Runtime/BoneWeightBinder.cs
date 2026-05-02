using UnityEngine;
using UnityEngine.Animations;
using nadena.dev.ndmf;

namespace net.nekobako.BoneWeightModifier.Runtime
{
    [HelpURL("https://bone-weight-modifier.nekobako.net"), DisallowMultipleComponent]
    internal class BoneWeightBinder : MonoBehaviour, INDMFEditorOnly
    {
        [SerializeField, NotKeyable]
        public bool IsBound = false;

        [SerializeField, NotKeyable]
        public Matrix4x4 Bindpose = Matrix4x4.zero;
    }
}
