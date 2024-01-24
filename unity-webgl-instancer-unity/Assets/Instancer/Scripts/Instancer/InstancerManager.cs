using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instancer
{
    public class InstancerManager : MonoBehaviour
    {
        public static InstancerManager instance { get; private set; }

        private static Dictionary<string, int> _materialPropertyIDs = new Dictionary<string, int>();

        private Dictionary<InstancerObject, List<InstancerRenderer>> _instancerRenderers = new Dictionary<InstancerObject, List<InstancerRenderer>>();

        private const int MAX_INSTANCE_COUNT_PER_BATCH = 1000;

        // Array size should be max count to match to the shader array size.
        private Matrix4x4[] _matrices = new Matrix4x4[MAX_INSTANCE_COUNT_PER_BATCH];
        private Vector4[] _customColors = new Vector4[MAX_INSTANCE_COUNT_PER_BATCH];
        private Vector4[] _customValues = new Vector4[MAX_INSTANCE_COUNT_PER_BATCH];
        private Vector4[] _customValues2 = new Vector4[MAX_INSTANCE_COUNT_PER_BATCH];
        private Vector4[] _animParams = new Vector4[MAX_INSTANCE_COUNT_PER_BATCH];

        public bool useInstancer = true;
        private bool _useInstancerCached = true;
        public Action<bool> toggleInstancerAction;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("InstancerManager already exists!");
                Destroy(this);
                return;
            }
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void LateUpdate()
        {
            // if (Input.GetKeyDown(KeyCode.L))
            // {
            //     useInstancer = !useInstancer;
            // }

            if (_useInstancerCached != useInstancer)
            {
                _useInstancerCached = useInstancer;
                toggleInstancerAction?.Invoke(useInstancer);
            }

            if (useInstancer)
            {
                Draw();
            }
        }

        public static void ToggleInstancer(bool isOn)
        {
            instance.useInstancer = isOn;
        }

        private void Draw()
        {
            foreach (InstancerObject instancer in _instancerRenderers.Keys)
            {
                if (instancer.instanceMode == InstancerObject.InstanceMode.DrawMeshInstancedProcedural)
                {
                    DrawMeshInstancedProcedural(instancer, _instancerRenderers[instancer]);
                }
                else
                {
                    DrawMeshInstanced(instancer, _instancerRenderers[instancer]);
                }
            }
        }

        private void DrawMeshInstanced(InstancerObject instancer, List<InstancerRenderer> instancerRenderers)
        {
            int totalInstanceCount = instancerRenderers.Count;
            if (totalInstanceCount == 0)
            {
                return;
            }

            int batchCount = Mathf.CeilToInt((float)totalInstanceCount / MAX_INSTANCE_COUNT_PER_BATCH);
            instancer.materialPropertyBlocks = new MaterialPropertyBlock[batchCount];

            if (!_materialPropertyIDs.ContainsKey(instancer.customColorPropertyName))
            {
                _materialPropertyIDs.Add(instancer.customColorPropertyName, Shader.PropertyToID(instancer.customColorPropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.customValuePropertyName))
            {
                _materialPropertyIDs.Add(instancer.customValuePropertyName, Shader.PropertyToID(instancer.customValuePropertyName));
            }
            int customColorPropertyID = _materialPropertyIDs[instancer.customColorPropertyName];
            int customValuePropertyID = _materialPropertyIDs[instancer.customValuePropertyName];

            foreach (var customUniformValue in instancer.customUniformValues)
            {
                if (!_materialPropertyIDs.ContainsKey(customUniformValue.propertyName))
                {
                    _materialPropertyIDs.Add(customUniformValue.propertyName, Shader.PropertyToID(customUniformValue.propertyName));
                }
            }

            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * MAX_INSTANCE_COUNT_PER_BATCH;
                int endIndex = Mathf.Min(startIndex + MAX_INSTANCE_COUNT_PER_BATCH, totalInstanceCount);
                int instanceCount = endIndex - startIndex;

                for (int j = 0; j < instanceCount; j++)
                {
                    InstancerRenderer instancerRenderer = instancerRenderers[startIndex + j];
                    _matrices[j] = instancerRenderer.transform.localToWorldMatrix;
                    if (instancer.useCustomData)
                    {
                        _customColors[j] = instancerRenderer.customColor;
                        _customValues[j] = instancerRenderer.customValue;
                    }
                }

                if (instancer.useCustomData)
                {
                    instancer.materialPropertyBlocks[i] = new MaterialPropertyBlock();
                    instancer.materialPropertyBlocks[i].SetVectorArray(customColorPropertyID, _customColors);
                    instancer.materialPropertyBlocks[i].SetVectorArray(customValuePropertyID, _customValues);
                }

                foreach (var customUniformValue in instancer.customUniformValues)
                {
                    instancer.materialPropertyBlocks[i].SetVector(_materialPropertyIDs[customUniformValue.propertyName], customUniformValue.value);
                }

                Graphics.DrawMeshInstanced(
                    mesh: instancer.mesh,
                    submeshIndex: 0,
                    material: instancer.material,
                    matrices: _matrices,
                    count: _matrices.Length,
                    properties: instancer.useCustomData ? instancer.materialPropertyBlocks[i] : null,
                    castShadows: instancer.shadowCastingMode,
                    receiveShadows: instancer.receiveShadows,
                    layer: instancer.layer,
                    camera: null,
                    lightProbeUsage: instancer.lightProbeUsage,
                    lightProbeProxyVolume: null);
            }
        }

        private void DrawMeshInstancedProcedural(InstancerObject instancer, List<InstancerRenderer> instancerRenderers)
        {
            int totalInstanceCount = instancerRenderers.Count;
            if (totalInstanceCount == 0)
            {
                return;
            }

            int batchCount = Mathf.CeilToInt((float)totalInstanceCount / MAX_INSTANCE_COUNT_PER_BATCH);
            if (instancer.materials.Length != batchCount)
            {
                instancer.materials = new Material[batchCount];
                for (int i = 0; i < batchCount; i++)
                {
                    instancer.materials[i] = Instantiate(instancer.material);
                }
            }
            else
            {
                for (int i = 0; i < instancer.materials.Length; i++)
                {
                    if (instancer.materials[i] == null)
                    {
                        instancer.materials[i] = Instantiate(instancer.material);
                    }
                }
            }

            // Cache values, to avoid getting values in the loop.
            bool useCustomColor = instancer.useCustomColor;
            bool useCustomValue = instancer.useCustomValue;
            bool useCustomData = instancer.useCustomData;
            bool useAnimation = instancer.animationDataObject != null && instancer.animationDataObject.animationClipDatas.Length > 0;
            if (!_materialPropertyIDs.ContainsKey(instancer.customColorPropertyName))
            {
                _materialPropertyIDs.Add(instancer.customColorPropertyName, Shader.PropertyToID(instancer.customColorPropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.customValuePropertyName))
            {
                _materialPropertyIDs.Add(instancer.customValuePropertyName, Shader.PropertyToID(instancer.customValuePropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.customValuePropertyName2))
            {
                _materialPropertyIDs.Add(instancer.customValuePropertyName2, Shader.PropertyToID(instancer.customValuePropertyName2));
            }
            int customColorPropertyID = _materialPropertyIDs[instancer.customColorPropertyName];
            int customValuePropertyID = _materialPropertyIDs[instancer.customValuePropertyName];
            int customValuePropertyID2 = _materialPropertyIDs[instancer.customValuePropertyName2];

            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.positionTexture0PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.positionTexture0PropertyName, Shader.PropertyToID(instancer.animationDataObject.positionTexture0PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.normalTexture0PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.normalTexture0PropertyName, Shader.PropertyToID(instancer.animationDataObject.normalTexture0PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.positionTexture1PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.positionTexture1PropertyName, Shader.PropertyToID(instancer.animationDataObject.positionTexture1PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.normalTexture1PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.normalTexture1PropertyName, Shader.PropertyToID(instancer.animationDataObject.normalTexture1PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.positionTexture2PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.positionTexture2PropertyName, Shader.PropertyToID(instancer.animationDataObject.positionTexture2PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.normalTexture2PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.normalTexture2PropertyName, Shader.PropertyToID(instancer.animationDataObject.normalTexture2PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.positionTexture3PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.positionTexture3PropertyName, Shader.PropertyToID(instancer.animationDataObject.positionTexture3PropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.normalTexture3PropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.normalTexture3PropertyName, Shader.PropertyToID(instancer.animationDataObject.normalTexture3PropertyName));
            }

            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.texelSizePropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.texelSizePropertyName, Shader.PropertyToID(instancer.animationDataObject.texelSizePropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.animationDataObject.animationParamsPropertyName))
            {
                _materialPropertyIDs.Add(instancer.animationDataObject.animationParamsPropertyName, Shader.PropertyToID(instancer.animationDataObject.animationParamsPropertyName));
            }
            int[] positionTexturePropertyIDs = new int[4]
            {
                _materialPropertyIDs[instancer.animationDataObject.positionTexture0PropertyName],
                _materialPropertyIDs[instancer.animationDataObject.positionTexture1PropertyName],
                _materialPropertyIDs[instancer.animationDataObject.positionTexture2PropertyName],
                _materialPropertyIDs[instancer.animationDataObject.positionTexture3PropertyName],
            };
            int[] normalTexturePropertyIDs = new int[4]
            {
                _materialPropertyIDs[instancer.animationDataObject.normalTexture0PropertyName],
                _materialPropertyIDs[instancer.animationDataObject.normalTexture1PropertyName],
                _materialPropertyIDs[instancer.animationDataObject.normalTexture2PropertyName],
                _materialPropertyIDs[instancer.animationDataObject.normalTexture3PropertyName],
            };
            int texelSizePropertyID = _materialPropertyIDs[instancer.animationDataObject.texelSizePropertyName];
            int animationParamsPropertyID = _materialPropertyIDs[instancer.animationDataObject.animationParamsPropertyName];
            float texelSize = instancer.animationDataObject.texelSize; // vertex count

            foreach (var customUniformValue in instancer.customUniformValues)
            {
                if (!_materialPropertyIDs.ContainsKey(customUniformValue.propertyName))
                {
                    _materialPropertyIDs.Add(customUniformValue.propertyName, Shader.PropertyToID(customUniformValue.propertyName));
                }
            }

            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * MAX_INSTANCE_COUNT_PER_BATCH;
                int endIndex = Mathf.Min(startIndex + MAX_INSTANCE_COUNT_PER_BATCH, totalInstanceCount);
                int instanceCount = endIndex - startIndex;

                if (useCustomData || useAnimation)
                {
                    for (int j = 0; j < instanceCount; j++)
                    {
                        InstancerRenderer instancerRenderer = instancerRenderers[startIndex + j];
                        if (useCustomColor)
                        {
                            _customColors[j] = instancerRenderer.customColor;
                        }
                        if (useCustomValue)
                        {
                            _customValues[j] = instancerRenderer.customValue;
                            _customValues2[j] = instancerRenderer.customValue2;
                        }
                        if (useAnimation)
                        {
                            _animParams[j] = instancerRenderer.animationParams;
                        }
                    }
                }

                // Set custom data
                if (useCustomColor)
                {
                    instancer.materials[i].SetVectorArray(customColorPropertyID, _customColors);
                }
                if (useCustomValue)
                {
                    instancer.materials[i].SetVectorArray(customValuePropertyID, _customValues);
                    instancer.materials[i].SetVectorArray(customValuePropertyID2, _customValues2);
                }
                foreach (var customUniformValue in instancer.customUniformValues)
                {
                    instancer.materials[i].SetVector(_materialPropertyIDs[customUniformValue.propertyName], customUniformValue.value);
                }

                // Set animation data
                if (useAnimation)
                {
                    for (int j = 0; j < instancer.animationDataObject.animationClipDatas.Length; j++)
                    {
                        if (instancer.animationDataObject.animationClipDatas[j] != null)
                        {
                            instancer.materials[i].SetTexture(positionTexturePropertyIDs[j], instancer.animationDataObject.animationClipDatas[j].positionTexture);
                            instancer.materials[i].SetTexture(normalTexturePropertyIDs[j], instancer.animationDataObject.animationClipDatas[j].normalTexture);
                        }
                    }
                    instancer.materials[i].SetFloat(texelSizePropertyID, texelSize);
                    instancer.materials[i].SetVectorArray(animationParamsPropertyID, _animParams);
                }

                Graphics.DrawMeshInstancedProcedural(
                    mesh: instancer.mesh,
                    submeshIndex: 0,
                    material: instancer.materials[i],
                    bounds: new Bounds(Vector3.zero, Vector3.one * 1000f),
                    count: instanceCount,
                    properties: null,
                    castShadows: instancer.shadowCastingMode,
                    receiveShadows: instancer.receiveShadows,
                    layer: instancer.layer,
                    camera: null,
                    lightProbeUsage: instancer.lightProbeUsage,
                    lightProbeProxyVolume: null);
            }
        }

        public void AddInstancerRenderer(InstancerRenderer instancerRenderer)
        {
            if (!_instancerRenderers.ContainsKey(instancerRenderer.instancerObject))
            {
                _instancerRenderers.Add(instancerRenderer.instancerObject, new List<InstancerRenderer>());
            }
            _instancerRenderers[instancerRenderer.instancerObject].Add(instancerRenderer);
        }

        public void RemoveInstancerRenderer(InstancerRenderer instancerRenderer)
        {
            if (_instancerRenderers.ContainsKey(instancerRenderer.instancerObject))
            {
                _instancerRenderers[instancerRenderer.instancerObject].Remove(instancerRenderer);
            }
        }
    }
}