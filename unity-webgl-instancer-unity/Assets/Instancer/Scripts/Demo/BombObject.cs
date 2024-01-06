using UnityEngine;

public class BombObject : MonoBehaviour
{
    private static readonly int PROP_OPACITY = Shader.PropertyToID("_Opacity");

    private const float BOMB_ATTACK_RADIUS_START = 1.0f;
    private const float BOMB_ATTACK_RADIUS_END = 25f;
    private const float BOMB_EFFECT_SPEED = 20f;

    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private LayerMask _layerMask;

    private bool _isBombAttack = false;
    private float _radius = BOMB_ATTACK_RADIUS_START;

    private void Awake()
    {
        _renderer.enabled = false;
    }

    private void Update()
    {
        if (_isBombAttack)
        {
            if (_radius < BOMB_ATTACK_RADIUS_END)
            {
                _radius += Time.deltaTime * BOMB_EFFECT_SPEED;

                transform.localScale = Vector3.one * _radius * 2f;
                _renderer.material.SetFloat(PROP_OPACITY, 2f - _radius / BOMB_ATTACK_RADIUS_END);

                Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _layerMask);
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].TryGetComponent(out ZombieBehaviour zombie))
                    {
                        zombie.Hit(20, transform.position);
                    }
                }
            }
            else
            {
                _isBombAttack = false;
                _renderer.enabled = false;
            }
        }
    }

    public void PlayAttack(Vector3 position)
    {
        transform.position = position;

        _isBombAttack = true;
        _radius = BOMB_ATTACK_RADIUS_START;
        _renderer.enabled = true;
    }
}
