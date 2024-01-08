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
    [SerializeField] private RectTransform _transformButtons;
    private Vector2 _buttonsPosition;
    private Rect _safeAreaCache;

    [SerializeField] private TextMeshProUGUI _textKillCount;

    [SerializeField] private DemoButtonView _buttonBomb;
    [SerializeField] private DemoButtonView _buttonLargeBomb;

    [SerializeField] private DemoToggleView _toggleLaserAttack;
    [SerializeField] private DemoToggleView _toggleCameraView;
    [SerializeField] private DemoToggleView _toggleInstancing;

    [SerializeField] private GameObject _rootToShowHide;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        SpatialBridge.coreGUIService.SetCoreGUIEnabled(SpatialCoreGUITypeFlags.Chat, false);

        _safeAreaCache = Screen.safeArea;
        _killCountPosition = _transformKillCount.anchoredPosition;
        _buttonsPosition = _transformButtons.anchoredPosition;
        _transformKillCount.anchoredPosition = _killCountPosition + new Vector2(_safeAreaCache.xMin, _safeAreaCache.yMin);
        _transformButtons.anchoredPosition = _buttonsPosition - new Vector2(Screen.width - _safeAreaCache.xMax, Screen.height - _safeAreaCache.yMax);

        _buttonBomb.button.onClick.AddListener(Player.BombAttack);
        _buttonLargeBomb.button.onClick.AddListener(Player.LargeBombAttack);

        _toggleLaserAttack.toggle.onValueChanged.AddListener(Player.ToggleLaserAttack);
        _toggleCameraView.toggle.onValueChanged.AddListener(ToggleVirtualCamera.ToggleVirtualCameraActive);
        _toggleInstancing.toggle.onValueChanged.AddListener(InstancerManager.ToggleInstancer);
    }

    private void Update()
    {
        if (_safeAreaCache.xMin != Screen.safeArea.xMin ||
            _safeAreaCache.yMin != Screen.safeArea.yMin ||
            _safeAreaCache.xMax != Screen.safeArea.xMax ||
            _safeAreaCache.yMax != Screen.safeArea.yMax)
        {
            _safeAreaCache = Screen.safeArea;
            _transformKillCount.anchoredPosition = _killCountPosition + new Vector2(_safeAreaCache.xMin, _safeAreaCache.yMin);
            _transformButtons.anchoredPosition = _buttonsPosition - new Vector2(Screen.width - _safeAreaCache.xMax, Screen.height - _safeAreaCache.yMax);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            _rootToShowHide.SetActive(!_rootToShowHide.activeSelf);
        }

        if (!_rootToShowHide.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ToggleVirtualCamera.ToggleVirtualCameraActive();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Player.ToggleLaserAttack();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                Player.BombAttack();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Player.LargeBombAttack();
            }
        }
    }

    public static void UpdateKillCount(int killCount)
    {
        instance._textKillCount.text = "Score: " + killCount.ToString("N0");
    }
}
