using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using nadena.dev.ndmf;

namespace net.nekobako.BoneWeightModifier.Runtime
{
    [HelpURL("https://bone-weight-modifier.nekobako.net")]
    internal class BoneWeightModifier : MonoBehaviour, INDMFEditorOnly
    {
        [SerializeField, NotKeyable]
        public Transform Bone = null;

        [SerializeField, NotKeyable]
        public Renderer Renderer = null;

        [SerializeReference, NotKeyable]
        public List<IBoneWeight> Weights = new();
    }
}
