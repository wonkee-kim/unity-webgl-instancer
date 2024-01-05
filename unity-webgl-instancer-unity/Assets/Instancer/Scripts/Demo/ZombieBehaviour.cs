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
    private float _dieTime;
    public bool readyToHit = true;

    private void Awake()
    {
        _dieTime = _instancerRenderer.instancerObject.animationDataObject.animationClipDatas[1].animationLength;
    }

    private void Update()
    {
        if (Player.instance != null && readyToHit)
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

        Color color = Color.HSVToRGB(Random.Range(0.0f, 0.2f) - 0.1f, Random.Range(0.8f, 1f), Random.Range(0.2f, 1f));
        _instancerRenderer.customColor = new Vector4(color.r, color.g, color.b, Random.Range(0f, 1f));
        _instancerRenderer.customValue = new Vector4(transform.position.x, transform.position.z, transform.localScale.y, emission);

        _instancerRenderer.PlayAnimationClip(0);
    }

    public void Hit(int damage)
    {
        readyToHit = false;
        _health -= damage;
        transform.position += (transform.position - Player.position).normalized * 1f;
        _instancerRenderer.PlayAnimationClip(1);

        if (_hitCoroutine != null)
        {
            StopCoroutine(_hitCoroutine);
        }
        _hitCoroutine = HitCoroutine();
        StartCoroutine(_hitCoroutine);
    }

    private IEnumerator HitCoroutine()
    {
        float currentTime = Time.realtimeSinceStartup;
        emission = 8f;
        // transform.localScale = new Vector3(1f, emission * 0.2f + 1f, 1f);
        yield return new WaitForSeconds(0.01f);
        while (emission > 0f)
        {
            emission -= Time.deltaTime * 30f;
            // transform.localScale = new Vector3(1f, emission * 0.2f + 1f, 1f);
            yield return null;
        }
        emission = 0f;
        if (_health > 0)
        {
            readyToHit = true;
        }
        else
        {
            yield return new WaitForSeconds(_dieTime - (Time.realtimeSinceStartup - currentTime));
            gameObject.SetActive(false);
        }
    }
}
