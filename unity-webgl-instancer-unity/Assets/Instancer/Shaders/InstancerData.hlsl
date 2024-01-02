#ifndef INSTANCER_DATA_INCLUDED
    #define INSTANCER_DATA_INCLUDED

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

    UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4, _CustomColors)
    UNITY_DEFINE_INSTANCED_PROP(float4, _CustomValues)
    UNITY_INSTANCING_BUFFER_END(Props)

    void GetInstancerData_float(float3 In, out float4 customColor, out float4 customValue)
    {
        customColor = UNITY_ACCESS_INSTANCED_PROP(Props, _CustomColors);
        customValue = UNITY_ACCESS_INSTANCED_PROP(Props, _CustomValues);
    }
#endif