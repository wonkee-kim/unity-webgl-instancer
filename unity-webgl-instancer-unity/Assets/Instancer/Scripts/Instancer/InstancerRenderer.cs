using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instancer
{
    public class InstancerRenderer : MonoBehaviour
    {
        public InstancerObject instancerObject;
        [SerializeField] private Renderer _renderer;

        public Vector4 customColor = Vector4.one;
        public Vector4 customValue = Vector4.zero;

        private void OnEnable()
        {
            InstancerManager.instance.AddInstancerRenderer(this);
            InstancerManager.instance.toggleInstancerAction += HandleInstancerToggle;
            _renderer.enabled = false;
        }

        private void OnDisable()
        {
            if (InstancerManager.instance != null)
            {
                InstancerManager.instance.RemoveInstancerRenderer(this);
                InstancerManager.instance.toggleInstancerAction -= HandleInstancerToggle;
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
                if (GetComponent<MeshFilter>().sharedMesh != instancerObject.mesh)
                {
                    Debug.LogWarning($"MeshFilter.mesh does not match InstancerObject.mesh on {gameObject.name}.");
                }
            }
        }

        private void HandleInstancerToggle(bool useInstancer)
        {
            _renderer.enabled = !useInstancer;
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