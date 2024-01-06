using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DemoToggleView : MonoBehaviour, IPointerEnterHandler
{
    public Toggle toggle => _toggle;

    [Header("References")]
    [SerializeField] private Toggle _toggle;
    [SerializeField] private TextMeshProUGUI _buttonText;

    [Header("Settings")]
    [SerializeField] private string _buttonName;
    [SerializeField] private KeyCode[] _hotkeys;

    // protected virtual void Awake()
    // {
    //     _button.onClick.AddListener(OnClick);
    // }

    private void Update()
    {
        foreach (var hotkey in _hotkeys)
        {
            if (Input.GetKeyDown(hotkey))
            {
                _toggle.isOn = !_toggle.isOn;
                break;
            }
        }
    }

    // private void OnClick()
    // {
    //     // AudioManager.PlaySFX(_soundClick);
    // }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // AudioManager.PlaySFX(_soundHover);
    }

#if UNITY_EDITOR
    public void ButtonSetup()
    {
        _buttonText.gameObject.SetActive(!String.IsNullOrEmpty(_buttonName));
        if (!String.IsNullOrEmpty(_buttonName))
        {
            _buttonText.text = _buttonName;
        }
        EditorUtility.SetDirty(this);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(DemoToggleView))]
public class DemoToggleViewInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        EditorGUILayout.Space(10f);

        DemoToggleView view = (DemoToggleView)target;

        if (GUILayout.Button(nameof(view.ButtonSetup)))
        {
            view.ButtonSetup();
        }
    }
}
#endif
