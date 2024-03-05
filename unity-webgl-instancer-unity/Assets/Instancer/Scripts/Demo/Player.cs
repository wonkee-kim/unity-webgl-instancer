using UnityEngine;
using SpatialSys.UnitySDK;
using Instancer;

public class Player : MonoBehaviour
{
    private static readonly int PROP_OPACITY = Shader.PropertyToID("_Opacity");

    public static Player instance { get; private set; }
    public static Vector3 position => instance._playerPosition;
    [SerializeField] private Vector3 _playerPosition;
    [SerializeField] private Vector3 _playerForward = Vector3.forward;

    [Header("Laser Attack")]
    [SerializeField] private float _laserRadius = 7f;
    [SerializeField] private LineRenderer[] _lineRenderers;
    private float[] _lineRendererTimers;
    private bool _isLaserAttack = true;

    [Header("Bomb Attack")]
    [SerializeField] private float _bombRadius = 15;
    [SerializeField] private float _bombAttackRadius = 5f;
    [SerializeField] private MeshRenderer _bombEffectRenderer;
    private const float BOMB_EFFECT_STAY_TIME = 0.2f;
    private float _bombEffectTime = 0f;

    [Header("Large Bomb Attack")]
    [SerializeField] private BombObject _largeBombObject;

    [Space(10)]
    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private InstancerObject _instancerObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _lineRendererTimers = new float[_lineRenderers.Length];
        _bombEffectRenderer.transform.localScale = Vector3.one * _bombAttackRadius * 2f;
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
        Gizmos.DrawWireSphere(_playerPosition, _laserRadius);
    }

    private void Update()
    {
#if !UNITY_EDITOR
        if (SpatialBridge.spaceContentService.isSceneInitialized)
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

        // if (Input.GetKeyDown(KeyCode.J))
        // {
        //     ToggleLaserAttack();
        // }

        // if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
        // {
        //     BombAttackInternal();
        // }

        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     _largeBombObject.PlayAttack(_playerPosition);
        // }

        if (_isLaserAttack)
        {
            UpdateLaserAttack();
        }
        UpdateBombAttack();
    }

    private void UpdateLaserAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(_playerPosition, _laserRadius, _layerMask);

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
                            zombie.Hit(10, _playerPosition);
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

    private void UpdateBombAttack()
    {
        float bombAttackTime = Time.time - _bombEffectTime;
        if (bombAttackTime > BOMB_EFFECT_STAY_TIME)
        {
            _bombEffectRenderer.enabled = false;
        }
        else
        {
            float opacity = 1f - bombAttackTime / BOMB_EFFECT_STAY_TIME;
            opacity = opacity * opacity * opacity;
            _bombEffectRenderer.material.SetFloat(PROP_OPACITY, opacity);
        }
    }

    public static void ToggleLaserAttack(bool isOn)
    {
        instance.ToggleLaserAttackInternal(isOn);
    }
    public static void ToggleLaserAttack()
    {
        instance.ToggleLaserAttackInternal(!instance._isLaserAttack);
    }
    private void ToggleLaserAttackInternal(bool isOn)
    {
        _isLaserAttack = isOn;
        if (!_isLaserAttack)
        {
            for (int i = 0; i < _lineRenderers.Length; i++)
            {
                _lineRenderers[i].enabled = false;
            }
            for (int i = 0; i < _lineRendererTimers.Length; i++)
            {
                _lineRendererTimers[i] = -1f;
            }
        }
    }

    public static void BombAttack()
    {
        instance.BombAttackInternal();
    }
    private void BombAttackInternal()
    {
        Vector3 randomPosition = Random.insideUnitCircle * _bombRadius;
        Vector3 attackPosition = new Vector3(randomPosition.x, 0f, randomPosition.y) + _playerPosition;

        _bombEffectTime = Time.time;
        _bombEffectRenderer.enabled = true;
        _bombEffectRenderer.transform.position = attackPosition;

        Collider[] colliders = Physics.OverlapSphere(attackPosition, _bombAttackRadius, _layerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent(out ZombieBehaviour zombie))
            {
                float distance = Vector3.Distance(attackPosition, zombie.transform.position);
                zombie.Hit((int)(30f * (1f - distance / _bombAttackRadius)), attackPosition);
            }
        }
    }

    public static void LargeBombAttack()
    {
        instance.LargeBombAttackInternal();
    }
    private void LargeBombAttackInternal()
    {
        _largeBombObject.PlayAttack(_playerPosition);
    }
}
