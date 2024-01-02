using UnityEngine;
using UnityEngine.Rendering;

namespace Instancer
{
    [CreateAssetMenu(fileName = "InstancerObject", menuName = "Instancer/InstancerObject", order = 1)]
    public class InstancerObject : ScriptableObject
    {
        [Header("Instancer Object")]
        public Mesh mesh;
        public Material material;

        [Header("Custom Data")]
        public bool useCustomColor = false;
        public bool useCustomValue = false;
        public bool useCustomData => useCustomColor || useCustomValue;
        public string customColorPropertyName = "_CustomColors"; // Vector4
        public string customValuePropertyName = "_CustomValues"; // Vector4

        [Header("Render Settings")]
        public MaterialPropertyBlock[] materialPropertyBlocks;
        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public bool receiveShadows = true;
        public int layer = 0;
        public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;
    }
}