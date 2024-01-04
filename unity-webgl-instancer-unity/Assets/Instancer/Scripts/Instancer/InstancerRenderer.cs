using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instancer
{
    public class InstancerRenderer : MonoBehaviour
    {
        public InstancerObject instancerObject;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Animator _animator;

        public Vector4 customColor = Vector4.one;
        public Vector4 customValue = Vector4.zero;

        private void Start()
        {
            InstancerManager.instance.toggleInstancerAction += HandleInstancerToggle;
        }

        private void OnDestroy()
        {
            if (InstancerManager.instance != null)
            {
                InstancerManager.instance.toggleInstancerAction -= HandleInstancerToggle;
            }
        }

        private void OnEnable()
        {
            InstancerManager.instance.AddInstancerRenderer(this);
            _renderer.enabled = !InstancerManager.instance.useInstancer;
            if (_animator != null)
            {
                _animator.enabled = !InstancerManager.instance.useInstancer;
            }
        }

        private void OnDisable()
        {
            if (InstancerManager.instance != null)
            {
                InstancerManager.instance.RemoveInstancerRenderer(this);
            }
        }

        private void OnValidate()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (instancerObject != null)
            {
                if (_renderer is MeshRenderer)
                {
                    if (GetComponent<MeshFilter>().sharedMesh != instancerObject.mesh)
                    {
                        Debug.LogWarning($"MeshFilter.mesh does not match InstancerObject.mesh on {gameObject.name}.");
                    }
                }
                else if (_renderer is SkinnedMeshRenderer)
                {
                    if ((_renderer as SkinnedMeshRenderer).sharedMesh != instancerObject.mesh)
                    {
                        Debug.LogWarning($"SkinnedMeshRenderer.sharedMesh does not match InstancerObject.mesh on {gameObject.name}.");
                    }
                }
            }
        }

        private void HandleInstancerToggle(bool useInstancer)
        {
            _renderer.enabled = !useInstancer;
            if (_animator != null)
            {
                _animator.enabled = !useInstancer;
            }

            if (useInstancer)
            {
                InstancerManager.instance.AddInstancerRenderer(this);
            }
            else
            {
                if (InstancerManager.instance != null)
                {
                    InstancerManager.instance.RemoveInstancerRenderer(this);
                }
            }
        }
    }
}