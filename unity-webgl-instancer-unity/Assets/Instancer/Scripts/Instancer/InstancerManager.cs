using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace Instancer
{
    public class InstancerManager : MonoBehaviour
    {
        public static InstancerManager instance { get; private set; }

        private static Dictionary<string, int> _materialPropertyIDs = new Dictionary<string, int>();

        private const int MAX_INSTANCE_COUNT_PER_BATCH = 256;
        private Dictionary<InstancerObject, List<InstancerRenderer>> _instancerRenderers = new Dictionary<InstancerObject, List<InstancerRenderer>>();

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
            if (Input.GetKeyDown(KeyCode.L))
            {
                useInstancer = !useInstancer;
            }

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
            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * MAX_INSTANCE_COUNT_PER_BATCH;
                int endIndex = Mathf.Min(startIndex + MAX_INSTANCE_COUNT_PER_BATCH, totalInstanceCount);
                int instanceCount = endIndex - startIndex;

                Matrix4x4[] matrices = new Matrix4x4[instanceCount];
                Vector4[] customColors = new Vector4[instanceCount];
                Vector4[] customValues = new Vector4[instanceCount];
                for (int j = 0; j < instanceCount; j++)
                {
                    InstancerRenderer instancerRenderer = instancerRenderers[startIndex + j];
                    matrices[j] = instancerRenderer.transform.localToWorldMatrix;
                    if (instancer.useCustomData)
                    {
                        customColors[j] = instancerRenderer.customColor;
                        customValues[j] = instancerRenderer.customValue;
                    }
                }

                if (instancer.useCustomData)
                {
                    if (!_materialPropertyIDs.ContainsKey(instancer.customColorPropertyName))
                    {
                        _materialPropertyIDs.Add(instancer.customColorPropertyName, Shader.PropertyToID(instancer.customColorPropertyName));
                    }
                    if (!_materialPropertyIDs.ContainsKey(instancer.customValuePropertyName))
                    {
                        _materialPropertyIDs.Add(instancer.customValuePropertyName, Shader.PropertyToID(instancer.customValuePropertyName));
                    }
                    instancer.materialPropertyBlocks[i] = new MaterialPropertyBlock();
                    instancer.materialPropertyBlocks[i].SetVectorArray(_materialPropertyIDs[instancer.customColorPropertyName], customColors);
                    instancer.materialPropertyBlocks[i].SetVectorArray(_materialPropertyIDs[instancer.customValuePropertyName], customValues);
                }

                Graphics.DrawMeshInstanced(
                    mesh: instancer.mesh,
                    submeshIndex: 0,
                    material: instancer.material,
                    matrices: matrices,
                    count: matrices.Length,
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

            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * MAX_INSTANCE_COUNT_PER_BATCH;
                int endIndex = Mathf.Min(startIndex + MAX_INSTANCE_COUNT_PER_BATCH, totalInstanceCount);
                int instanceCount = endIndex - startIndex;

                Vector4[] customColors = new Vector4[instanceCount];
                Vector4[] customValues = new Vector4[instanceCount];
                for (int j = 0; j < instanceCount; j++)
                {
                    InstancerRenderer instancerRenderer = instancerRenderers[startIndex + j];
                    if (instancer.useCustomData)
                    {
                        customColors[j] = instancerRenderer.customColor;
                        customValues[j] = instancerRenderer.customValue;
                    }
                }

                if (!_materialPropertyIDs.ContainsKey(instancer.customColorPropertyName))
                {
                    _materialPropertyIDs.Add(instancer.customColorPropertyName, Shader.PropertyToID(instancer.customColorPropertyName));
                }
                if (!_materialPropertyIDs.ContainsKey(instancer.customValuePropertyName))
                {
                    _materialPropertyIDs.Add(instancer.customValuePropertyName, Shader.PropertyToID(instancer.customValuePropertyName));
                }
                instancer.materials[i].SetVectorArray(_materialPropertyIDs[instancer.customColorPropertyName], customColors);
                instancer.materials[i].SetVectorArray(_materialPropertyIDs[instancer.customValuePropertyName], customValues);
                instancer.materials[i].EnableKeyword("PROCEDURAL_INSTANCING_ON");
                instancer.materials[i].EnableKeyword("UNITY_PROCEDURAL_INSTANCING_ENABLED");
                instancer.materials[i].EnableKeyword("PROCEDURAL_INSTANCING_ON");

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