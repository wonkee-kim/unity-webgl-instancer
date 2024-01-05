using UnityEngine;
using SpatialSys.UnitySDK;
public class ToggleVirtualCamera : MonoBehaviour
{
    [SerializeField] private SpatialVirtualCamera _virtualCamera;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _virtualCamera.gameObject.SetActive(!_virtualCamera.gameObject.activeSelf);
        }
    }
}