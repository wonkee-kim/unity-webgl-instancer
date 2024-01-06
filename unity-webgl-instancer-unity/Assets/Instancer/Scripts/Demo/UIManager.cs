using System.Collections;
using System.Collections.Generic;
using Instancer;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [SerializeField] private RectTransform _transformKillCount;
    private Vector2 _killCountPosition;
    private Rect _safeAreaCache;
    [SerializeField] private TextMeshProUGUI _textKillCount;

    [SerializeField] private DemoButtonView _buttonBomb;
    [SerializeField] private DemoButtonView _buttonLargeBomb;

    [SerializeField] private DemoToggleView _toggleLaserAttack;
    [SerializeField] private DemoToggleView _toggleCameraView;
    [SerializeField] private DemoToggleView _toggleInstancing;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        SpatialBridge.coreGUIService.SetCoreGUIEnabled(SpatialCoreGUITypeFlags.Chat, false);

        _killCountPosition = _transformKillCount.anchoredPosition;
        _safeAreaCache = Screen.safeArea;
        _transformKillCount.anchoredPosition = _killCountPosition + new Vector2(_safeAreaCache.xMin, _safeAreaCache.yMin);

        _buttonBomb.button.onClick.AddListener(Player.BombAttack);
        _buttonLargeBomb.button.onClick.AddListener(Player.LargeBombAttack);

        _toggleLaserAttack.toggle.onValueChanged.AddListener(Player.ToggleLaserAttack);
        _toggleCameraView.toggle.onValueChanged.AddListener(ToggleVirtualCamera.ToggleVirtualCameraActive);
        _toggleInstancing.toggle.onValueChanged.AddListener(InstancerManager.ToggleInstancer);
    }

    private void Update()
    {
        if (_safeAreaCache != Screen.safeArea)
        {
            _safeAreaCache = Screen.safeArea;
            _transformKillCount.anchoredPosition = _killCountPosition + new Vector2(_safeAreaCache.xMin, _safeAreaCache.yMin);
        }
    }

    public static void UpdateKillCount(int killCount)
    {
        instance._textKillCount.text = "Kill Count: " + killCount.ToString("N0");
    }
}
