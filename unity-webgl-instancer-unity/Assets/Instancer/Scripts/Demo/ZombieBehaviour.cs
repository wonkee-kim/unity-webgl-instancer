using UnityEngine;
using Instancer;
using System.Collections;

public class ZombieBehaviour : MonoBehaviour
{
    [SerializeField] private InstancerRenderer _instancerRenderer;
    [SerializeField] private Rigidbody _rigidbody;

    private float _speed = 3f;
    private int _health = 10;
    private float emission = 0f;

    private IEnumerator _hitCoroutine;
    public bool readyToHit = true;

    private void Update()
    {
        if (Player.instance != null)
        {
            Vector3 direction = (Player.position - transform.position).normalized;
            _rigidbody.velocity = direction * _speed;
        }

        _instancerRenderer.customValue = new Vector4(transform.position.x, transform.position.z, transform.localScale.y, emission);
    }

    public void Spawn(Vector3 position)
    {
        readyToHit = true;
        transform.position = position;
        _rigidbody.velocity = Vector3.zero;
        _health = 10;

        _instancerRenderer.customValue = new Vector4(transform.position.x, transform.position.z, transform.localScale.y, emission);
    }

    public void Hit(int damage)
    {
        readyToHit = false;
        _health -= damage;
        transform.position += (transform.position - Player.position).normalized * 0.5f;

        if (_hitCoroutine != null)
        {
            StopCoroutine(_hitCoroutine);
        }
        _hitCoroutine = HitCoroutine();
        StartCoroutine(_hitCoroutine);
    }

    private IEnumerator HitCoroutine()
    {
        emission = 4f;
        transform.localScale = new Vector3(1f, emission * 0.2f + 1f, 1f);
        yield return new WaitForSeconds(0.02f);
        while (emission > 0f)
        {
            emission -= Time.deltaTime * 20f;
            transform.localScale = new Vector3(1f, emission * 0.2f + 1f, 1f);
            yield return null;
        }
        emission = 0f;
        if (_health <= 0)
        {
            gameObject.SetActive(false);
        }
        readyToHit = true;
    }
}
