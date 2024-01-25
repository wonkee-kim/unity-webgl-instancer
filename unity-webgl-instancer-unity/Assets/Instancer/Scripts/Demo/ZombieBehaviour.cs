using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
// using Instancer;

[Preserve]
public class ZombieBehaviour : MonoBehaviour
{
    private static readonly int PROP_EMISSION = Shader.PropertyToID("_Emission");
    private static readonly int PROP_RANDOM_SEED = Shader.PropertyToID("_RandomSeed");

    public enum RenderMode
    {
        Instancer,
        VertexAnimation,
    }
    [SerializeField] private RenderMode _renderMode = RenderMode.Instancer;

    //[SerializeField] private InstancerRenderer _instancerRenderer;
    //[SerializeField] private VertexAnimationRenderer _vertexAnimationRenderer;
    //private VertexAnimationDataObject _animationDataObject => _vertexAnimationRenderer.animationDataObject;
        //(_renderMode == RenderMode.Instancer) ? _instancerRenderer.instancerObject.animationDataObject : _vertexAnimationRenderer.animationDataObject;

    /*[SerializeField] private Rigidbody _rigidbody;

    private float _speed = 3f;
    private int _health = 10;
    private float _emission = 0f;

    /// <summary>
    // private IEnumerator _hitCoroutine;
    /// </summary>
    private float _dieTime;
    public bool readyToHit = true;

    private Vector3 _hitPosition;*/

    /*private void Awake()
    {
        //_dieTime = _animationDataObject.animationClipDatas[1].animationLength;
        if (_renderMode == RenderMode.VertexAnimation)
        {
            //_vertexAnimationRenderer.renderer.material.SetFloat(PROP_RANDOM_SEED, Random.Range(0f, 1f));
        }
    }

    private void OnDestroy() {
        Debug.Log("ZombieBehaviour.OnDestroy()");
        // is this good enough?
        // StopAllCoroutines();
        // _hitCoroutine = null;
    }

    private void Update()
    {
        // if (Player.instance != null && readyToHit)
        // {
        //     Vector3 direction = (Player.position - transform.position).normalized;
        //     _rigidbody.velocity = direction * _speed;
        // }

        switch (_renderMode)
        {
            case RenderMode.Instancer:
                // _instancerRenderer.customValue = new Vector4(transform.position.x, transform.position.z, transform.localScale.y, _emission);
                // if (readyToHit)
                // {
                //     _instancerRenderer.customValue2 = Player.position;
                // }
                // else
                // {
                //     _instancerRenderer.customValue2 = _hitPosition;
                // }
                // break;
            case RenderMode.VertexAnimation:
                //_vertexAnimationRenderer.renderer.material.SetFloat(PROP_EMISSION, _emission);

                if (Input.GetKeyDown(KeyCode.C))
                {
                    //_vertexAnimationRenderer.PlayAnimationClip(0);
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    //_vertexAnimationRenderer.PlayAnimationClip(1);
                }
                break;
        }
    }

    public void Spawn(Vector3 position)
    {
        readyToHit = true;
        transform.position = position;
        _rigidbody.velocity = Vector3.zero;
        _health = 10;

        Color color = Color.HSVToRGB(Random.Range(0.0f, 0.2f) - 0.1f, Random.Range(0.8f, 1f), Random.Range(0.2f, 1f));
        // _instancerRenderer.customColor = new Vector4(color.r, color.g, color.b, Random.Range(0f, 1f));
        // _instancerRenderer.customValue = new Vector4(transform.position.x, transform.position.z, transform.localScale.y, _emission);

        // _instancerRenderer.PlayAnimationClip(0);
    }

    public void Hit(int damage, Vector3 hitPosition)
    {
        if (!readyToHit)
        {
            return;
        }

        readyToHit = false;
        _hitPosition = hitPosition;

        _rigidbody.velocity = Vector3.zero;
        transform.position += (transform.position - _hitPosition).normalized * 1.5f;

        _health -= damage;
        if (_health <= 0)
        {
            // _instancerRenderer.PlayAnimationClip(1);
            //ZombieGenerator.AddKill();
        }

        // if (_hitCoroutine != null)
        // {
        //     StopCoroutine(_hitCoroutine);
        // }
        //_hitCoroutine = HitCoroutine();
        //StartCoroutine(_hitCoroutine);
    }*/

    // private IEnumerator HitCoroutine()
    // {
    //     float currentTime = Time.realtimeSinceStartup;
    //     _emission = 8f;
    //     // transform.localScale = new Vector3(1f, emission * 0.2f + 1f, 1f);
    //     yield return new WaitForSeconds(0.01f);
    //     Debug.Log("XXXX ZombieBehaviour.HitCoroutine() A");
    //     while (_emission > 0f)
    //     {
    //         Debug.Log("XXXX ZombieBehaviour.HitCoroutine() B " + _emission);
    //         _emission -= Time.deltaTime * 30f;
    //         // transform.localScale = new Vector3(1f, emission * 0.2f + 1f, 1f);
    //         yield return null;
    //     }
    //     Debug.Log("XXXX ZombieBehaviour.HitCoroutine() C");

    //     _emission = 0f;
    //     if (_health > 0)
    //     {
    //         readyToHit = true;
    //     }
    //     else
    //     {
    //         yield return new WaitForSeconds(_dieTime - (Time.realtimeSinceStartup - currentTime));
    //         gameObject.SetActive(false);
    //     }
    //     Debug.Log("XXXX ZombieBehaviour.HitCoroutine() D");
        
    // }
}
