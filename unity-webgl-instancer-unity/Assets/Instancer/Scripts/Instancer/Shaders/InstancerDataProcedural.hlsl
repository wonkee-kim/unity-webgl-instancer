#ifndef INSTANCER_DATA_PROCEDURAL_INCLUDED
    #define INSTANCER_DATA_PROCEDURAL_INCLUDED

    float4 _CustomColors[1000];
    float4 _CustomValues[1000];

    // #define vert(A) vert(A, uint instanceID : SV_InstanceID)
    // uint instanceID : SV_InstanceID;

    // #pragma instancing_options procedural:ConfigureProcedural
    void ConfigureProcedural()
    {
        // #if defined(UNITY_ANY_INSTANCING_ENABLED)
        // #if UNITY_ANY_INSTANCING_ENABLED
        //     instanceID = unity_InstanceID;
        // #endif
    }

    void GetInstancerData_float(float In, out float4 customColor, out float4 customValue)
    {
        // #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        // #ifdef PROCEDURAL_INSTANCING_ON
        // #if UNITY_ANY_INSTANCING_ENABLED
        //     customColor = _CustomColors[unity_InstanceID];
        //     customValue = _CustomValues[unity_InstanceID];
        // #else
        //     customColor = (float4)1;
        //     customValue = (float4)1;
        // #endif
        customColor = _CustomColors[In];
        customValue = _CustomValues[In];
        // customColor = _CustomColors[DEFAULT_UNITY_VERTEX_INPUT_INSTANCE_ID];
        // customValue = _CustomValues[DEFAULT_UNITY_VERTEX_INPUT_INSTANCE_ID];
    }

#endif