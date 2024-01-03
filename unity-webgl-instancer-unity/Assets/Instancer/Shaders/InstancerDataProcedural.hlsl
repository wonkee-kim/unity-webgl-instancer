// #ifndef INSTANCER_DATA_PROCEDURAL_INCLUDED
//     #define INSTANCER_DATA_PROCEDURAL_INCLUDED

float4 _CustomColors[256];
float4 _CustomValues[256];

// uint instanceID : SV_InstanceID;

uint instanceID;

float4 cColor;
float4 cValue;

// #pragma instancing_options procedural:setup
void ConfigureProcedural()
{
    // #if defined(UNITY_ANY_INSTANCING_ENABLED)
    #if UNITY_ANY_INSTANCING_ENABLED
        instanceID = unity_InstanceID;
        cColor = _CustomColors[unity_InstanceID];
        cValue = _CustomValues[unity_InstanceID];
    #endif
}

void GetInstancerData_float(float In, out float4 customColor, out float4 customValue)
// void GetInstancerData_float(float In1, float In2, out float4 customColor, out float4 customValue)
// void GetInstancerData_float(float instanceID, out float4 customColor, out float4 customValue, uint id: SV_InstanceID)
{
    // #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
    // #ifdef PROCEDURAL_INSTANCING_ON
    // #if UNITY_ANY_INSTANCING_ENABLED
    // customColor = _CustomColors[unity_InstanceID];
    // customValue = _CustomValues[unity_InstanceID];
    // customColor = _CustomColors[instanceID];
    // customValue = _CustomValues[instanceID];
    // customColor = _CustomColors[id];
    // customValue = _CustomValues[id];
    // #else
    //     customColor = (float4)1;
    //     customValue = (float4)1;
    // #endif
    // customColor = _CustomColors[instanceID];
    // customValue = _CustomValues[instanceID];
    customColor = _CustomColors[input.instanceID];
    customValue = _CustomValues[input.instanceID];
    // customColor = cColor;
    // customValue = cValue;
    // customColor = _CustomColors[In];
    // customValue = _CustomValues[In];
    // customColor = _CustomColors[DEFAULT_UNITY_VERTEX_INPUT_INSTANCE_ID];
    // customValue = _CustomValues[DEFAULT_UNITY_VERTEX_INPUT_INSTANCE_ID];
}
// #endif