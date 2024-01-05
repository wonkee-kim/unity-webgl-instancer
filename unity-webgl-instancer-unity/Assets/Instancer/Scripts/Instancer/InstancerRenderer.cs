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
        public Vector4 animationParams = new Vector4(0, 0, 1, 1); // x: index, y: time, z: animLengthInv, w: isLooping (0 or 1)

        private void Start()
        {
            InstancerManager.instance.toggleInstancerAction += HandleInstancerToggle;
            if (instancerObject.useAnimation)
            {
                PlayAnimationClip(0, initialize: true); // Initialize parameters
            }
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

        public void PlayAnimationClip(int clipIndex, bool initialize = false)
        {
            if (clipIndex != animationParams.x || initialize)
            {
                VertexAnimationDataObject.AnimationClipData animationClipData = instancerObject.animationDataObject.animationClipDatas[clipIndex];
                // x: index, y: time, z: animLengthInv, w: isLooping (0 or 1)
                animationParams = new Vector4(
                    clipIndex,
                    Time.time,
                    animationClipData.animationLengthInv,
                    animationClipData.isLooping);
            }
            else
            {
                animationParams.y = Time.time;
            }
        }
    }
}