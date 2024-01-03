using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public static Vector3 position => instance._playerPosition;
    [SerializeField] private Vector3 _playerPosition;
    [SerializeField] private float _radius = 5f;

    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private LineRenderer[] _lineRenderers;
    private float[] _lineRendererTimers;

    private bool _isAttack = true;

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

        if (!_isAttack)
        {
            return;
        }

        if (SpatialBridge.GetIsSceneInitialized())
        {
            _playerPosition = SpatialBridge.actorService.localActor.avatar.position;
        }
        else
        {
            _playerPosition = transform.position;
        }

        Collider[] colliders = Physics.OverlapSphere(_playerPosition, _radius, _layerMask);

        // Sort colliders by distance
        System.Array.Sort(colliders, (a, b) =>
        {
            float distanceA = Vector3.Distance(a.transform.position, _playerPosition);
            float distanceB = Vector3.Distance(b.transform.position, _playerPosition);
            return distanceA.CompareTo(distanceB);
        });

        int colliderIndex = 0;
        for (int i = 0; i < _lineRenderers.Length; i++)
        {
            _lineRenderers[i].SetPosition(0, _playerPosition + Vector3.up * 0.5f);

            if (_lineRendererTimers[i] <= 0f)
            {
                bool isHit = false;
                for (int j = colliderIndex; j < colliders.Length; j++)
                {
                    colliderIndex++;
                    if (colliders[j].TryGetComponent(out ZombieBehaviour zombie))
                    {
                        if (zombie.readyToHit)
                        {
                            zombie.Hit(10);
                            isHit = true;
                            _lineRenderers[i].SetPosition(1, new Vector3(zombie.transform.position.x, 0.5f, zombie.transform.position.z));
                            _lineRendererTimers[i] = 0.02f;
                        }
                        break;
                    }
                }
                _lineRenderers[i].enabled = isHit;
            }
            else
            {
                _lineRendererTimers[i] -= Time.deltaTime;
            }
        }
    }
}
