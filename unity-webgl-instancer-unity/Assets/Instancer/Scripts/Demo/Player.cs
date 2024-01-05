using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;
using Instancer;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public static Vector3 position => instance._playerPosition;
    [SerializeField] private Vector3 _playerPosition;
    [SerializeField] private Vector3 _playerForward = Vector3.forward;
    [SerializeField] private float _radius = 5f;

    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private LineRenderer[] _lineRenderers;
    private float[] _lineRendererTimers;

    private bool _isAttack = true;

    [SerializeField] private InstancerObject _instancerObject;

    [SerializeField] private SpatialVirtualCamera _virtualCamera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _lineRendererTimers = new float[_lineRenderers.Length];
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_playerPosition, _radius);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            _isAttack = !_isAttack;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _virtualCamera.gameObject.SetActive(!_virtualCamera.gameObject.activeSelf);
        }

        if (!_isAttack)
        {
            return;
        }

#if !UNITY_EDITOR
        if (SpatialBridge.GetIsSceneInitialized())
        {
            _playerPosition = SpatialBridge.actorService.localActor.avatar.position;
            _playerForward = SpatialBridge.actorService.localActor.avatar.rotation * Vector3.forward;
        }
        else
#endif
        {
            _playerPosition = transform.position;
            _playerForward = transform.forward;
        }
        _instancerObject.customUniformValues[0].value = new Vector4(_playerPosition.x, _playerPosition.y, _playerPosition.z, 0f);

        Collider[] colliders = Physics.OverlapSphere(_playerPosition, _radius, _layerMask);

        // Sort colliders by distance
        System.Array.Sort(colliders, (a, b) =>
        {
            Vector3 aToPlayer = a.transform.position - _playerPosition;
            Vector3 bToPlayer = b.transform.position - _playerPosition;

            float distanceA = Vector3.Dot(aToPlayer, aToPlayer);
            float distanceB = Vector3.Dot(bToPlayer, bToPlayer);

            float dotA = Vector3.Dot(_playerForward, aToPlayer / distanceA);
            float dotB = Vector3.Dot(_playerForward, bToPlayer / distanceB);
            dotA = Mathf.Max(dotA, 0.1f);
            dotB = Mathf.Max(dotB, 0.1f);

            float weightA = distanceA / dotA; // add weight to forward
            float weightB = distanceB / dotB; // add weight to forward

            return weightA.CompareTo(weightB);
        });

        for (int i = 0; i < _lineRenderers.Length; i++)
        {
            _lineRenderers[i].SetPosition(0, _playerPosition + Vector3.up * 0.5f);

            if (_lineRendererTimers[i] <= 0f)
            {
                bool isHit = false;
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (colliders[j].TryGetComponent(out ZombieBehaviour zombie))
                    {
                        if (zombie.readyToHit)
                        {
                            zombie.Hit(10);
                            isHit = true;
                            _lineRenderers[i].SetPosition(1, new Vector3(zombie.transform.position.x, 0.5f, zombie.transform.position.z));
                            _lineRendererTimers[i] = 0.15f;
                            break;
                        }
                    }
                }
                _lineRenderers[i].enabled = isHit;
            }
            else
            {
                _lineRendererTimers[i] -= Time.deltaTime;
                if (_lineRendererTimers[i] <= 0.1f)
                {
                    _lineRenderers[i].enabled = false;
                }
            }
        }
    }
}
