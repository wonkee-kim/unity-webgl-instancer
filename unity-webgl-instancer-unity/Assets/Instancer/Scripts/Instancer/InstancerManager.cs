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
        private static readonly int PROP_TARGET_POSITION = Shader.PropertyToID("_TargetPosition");

        private Dictionary<InstancerObject, List<InstancerRenderer>> _instancerRenderers = new Dictionary<InstancerObject, List<InstancerRenderer>>();

        private const int MAX_INSTANCE_COUNT_PER_BATCH = 1000;

        // Array size should be max count to match to the shader array size.
        private Matrix4x4[] _matrices = new Matrix4x4[MAX_INSTANCE_COUNT_PER_BATCH];
        private Vector4[] _customColors = new Vector4[MAX_INSTANCE_COUNT_PER_BATCH];
        private Vector4[] _customValues = new Vector4[MAX_INSTANCE_COUNT_PER_BATCH];

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

            if (!_materialPropertyIDs.ContainsKey(instancer.customColorPropertyName))
            {
                _materialPropertyIDs.Add(instancer.customColorPropertyName, Shader.PropertyToID(instancer.customColorPropertyName));
            }
            if (!_materialPropertyIDs.ContainsKey(instancer.customValuePropertyName))
            {
                _materialPropertyIDs.Add(instancer.customValuePropertyName, Shader.PropertyToID(instancer.customValuePropertyName));
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
                    instancer.materialPropertyBlocks[i].SetVectorArray(_materialPropertyIDs[instancer.customColorPropertyName], _customColors);
                    instancer.materialPropertyBlocks[i].SetVectorArray(_materialPropertyIDs[instancer.customValuePropertyName], _customValues);
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
            Vector3 targetPosition = (Player.instance != null) ? Player.position : Vector3.zero;

            for (int i = 0; i < batchCount; i++)
            {
                int startIndex = i * MAX_INSTANCE_COUNT_PER_BATCH;
                int endIndex = Mathf.Min(startIndex + MAX_INSTANCE_COUNT_PER_BATCH, totalInstanceCount);
                int instanceCount = endIndex - startIndex;

                if (useCustomData)
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
                        }
                    }
                }

                if (useCustomColor)
                {
                    instancer.materials[i].SetVectorArray(customColorPropertyID, _customColors);
                }
                if (useCustomValue)
                {
                    instancer.materials[i].SetVectorArray(customValuePropertyID, _customValues);
                }
                instancer.materials[i].SetVector(PROP_TARGET_POSITION, targetPosition);

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