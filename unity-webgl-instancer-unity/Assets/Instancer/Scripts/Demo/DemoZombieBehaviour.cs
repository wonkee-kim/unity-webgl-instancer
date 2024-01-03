using UnityEngine;
using Instancer;

public class DemoZombieBehaviour : MonoBehaviour
{
    [SerializeField] private InstancerRenderer _instancerRenderer;
    public float emission = 0f;

    private void Update()
    {
        Vector3 position = transform.position;
        _instancerRenderer.customValue = new Vector4(position.x, position.z, transform.localScale.y, emission);
    }
}
