using UnityEngine;
using SpatialSys.UnitySDK;
public class ToggleVirtualCamera : MonoBehaviour
{
    public static ToggleVirtualCamera instance { get; private set; }
    [SerializeField] private SpatialVirtualCamera _virtualCamera;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static void ToggleVirtualCameraActive(bool isOn)
    {
        instance._virtualCamera.gameObject.SetActive(isOn);
    }
}