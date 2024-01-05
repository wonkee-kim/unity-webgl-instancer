using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancer
{
    [CreateAssetMenu(fileName = "InstancerObject", menuName = "Instancer/InstancerObject", order = 1)]
    public class InstancerObject : ScriptableObject
    {
        public enum InstanceMode
        {
            DrawMeshInstancedProcedural,
            DrawMeshInstanced,
        }

        [System.Serializable]
        public struct CustomUniformValue
        {
            public string propertyName;
            public Vector4 value;
        }

        [Header("Instancer Object")]
        public Mesh mesh;
        public Material material;

        [Header("Custom Data")]
        public bool useCustomColor = false;
        public bool useCustomValue = false;
        public bool useCustomData => useCustomColor || useCustomValue;
        public string customColorPropertyName = "_CustomColors"; // Vector4
        public string customValuePropertyName = "_CustomValues"; // Vector4

        [Space(5), Tooltip("Use this to set custom values to the material. Not per instance.")]
        public CustomUniformValue[] customUniformValues;

        [Header("Animation Data")]
        public VertexAnimationDataObject animationDataObject;
        public bool useAnimation => animationDataObject != null && animationDataObject.animationClipDatas.Length > 0;

        [Header("Instance Settings")]
        public InstanceMode instanceMode = InstanceMode.DrawMeshInstancedProcedural;
        [HideInInspector] public MaterialPropertyBlock[] materialPropertyBlocks; // Only for DrawMeshInstanced
        [HideInInspector] public Material[] materials; // Only for DrawMeshInstancedProcedural

        [Header("Render Settings")]
        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public bool receiveShadows = true;
        public int layer = 0;
        public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;
    }
}