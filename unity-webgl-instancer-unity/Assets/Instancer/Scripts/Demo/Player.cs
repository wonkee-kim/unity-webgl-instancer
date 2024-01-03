using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }
    public static Vector3 position => instance._playerPosition;
    [SerializeField] private Vector3 _playerPosition;
    [SerializeField] private float _radius = 5f;

    [SerializeField] private LayerMask _layerMask;

    [SerializeField] private LineRenderer _lineRenderer;
    private IEnumerator _lineCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
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
        foreach (Collider collider in Physics.OverlapSphere(_playerPosition, _radius, _layerMask))
        {
            if (collider.TryGetComponent(out ZombieBehaviour zombie))
            {
                if (zombie.readyToHit)
                {
                    if (_lineCoroutine != null)
                    {
                        StopCoroutine(_lineCoroutine);
                    }
                    _lineCoroutine = LineCoroutine(zombie);
                    StartCoroutine(_lineCoroutine);

                    zombie.Hit(5);
                }
            }
        }
    }

    private IEnumerator LineCoroutine(ZombieBehaviour zombie)
    {
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(1, new Vector3(zombie.transform.position.x, 0.5f, zombie.transform.position.z));
        yield return new WaitForSeconds(0.05f);
        _lineRenderer.enabled = false;
    }
}
