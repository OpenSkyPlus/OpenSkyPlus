﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OpenSkyPlus;

public class OpenSkyPlusUi : MonoBehaviour
{
    private static GameObject _statusCircle;
    private static GameObject _popupConsole;
    private static string _consoleTextPlaceholder = string.Empty;
    private static Text _consoleText;
    private static ScrollRect _consoleScrollRect;
    private static Canvas _canvas;
    private static GameObject _openSkyPlusUiObject;
    private static bool _pluginLoaded;

    public static async void Initialize()
    {
        // Wait for App UI to be loaded first            
        await Task.Delay(1000);
        _openSkyPlusUiObject = new GameObject("OpenSkyPlusUi");
        _openSkyPlusUiObject.AddComponent<OpenSkyPlusUi>();
        DontDestroyOnLoad(_openSkyPlusUiObject);
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private static void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (_canvas == null) CreateUi();
    }

    private static void CreateUi()
    {
        _canvas = new GameObject("OpenSkyPlusCanvas").AddComponent<Canvas>();
        DontDestroyOnLoad(_canvas);
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.gameObject.layer = LayerMask.NameToLayer("UI");
        _canvas.gameObject.AddComponent<CanvasScaler>();
        _canvas.gameObject.AddComponent<GraphicRaycaster>();

        var statusBox = CreateStatusBox(_canvas.transform);
        _statusCircle = CreateStatusCircle(statusBox.transform);
        OpenSkyPlusApiInjector.MessageOpenSkyPlusReady += () =>
        {
            if (_statusCircle == null) return;
            var circleImage = _statusCircle.GetComponent<Image>();
            circleImage.color = _pluginLoaded ? Color.green : Color.yellow;
        };
        OpenSkyPlusPluginLoader.MessagePluginLoaded += () =>
        {
            _pluginLoaded = true;
            if (_statusCircle == null) return;
            var circleImage = _statusCircle.GetComponent<Image>();
            circleImage.color = Color.green;
        };
        OpenSkyPlusApiInjector.MessageMonitorDisconnected += () =>
        {
            if (_statusCircle == null) return;
            var circleImage = _statusCircle.GetComponent<Image>();
            circleImage.color = Color.red;
        };
        OpenSkyPlusApiInjector.MessageMonitorConnected += () =>
        {
            if (_statusCircle == null) return;
            var circleImage = _statusCircle.GetComponent<Image>();
            circleImage.color = _pluginLoaded ? Color.green : Color.yellow;
        };
        CreateStatusBoxLabel(statusBox.transform);

        BuildPopupConsole();
        statusBox.AddComponent<Button>().onClick.AddListener(TogglePopupConsole);
    }

    private static GameObject CreateStatusBox(Transform parent)
    {
        var statusBox = new GameObject("MainMenuStatusBox", typeof(RectTransform));
        statusBox.transform.SetParent(parent, false);
        var image = statusBox.AddComponent<Image>();
        image.color = Color.black;

        var rectTransform = statusBox.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(Screen.width / 10f, Screen.height / 15f);
        rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x / 2, rectTransform.sizeDelta.y / 2);

        return statusBox;
    }

    private static GameObject CreateStatusCircle(Transform parent)
    {
        var circle = new GameObject("ApiStatusLight", typeof(RectTransform));
        circle.transform.SetParent(parent, false);
        var circleRenderer = circle.AddComponent<UiCircle>();
        circleRenderer.color = OpenSkyPlusApi.IsLoaded() ? Color.green : Color.red;

        var rectTransform = circle.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(1f / 3f, 0.5f);
        rectTransform.sizeDelta = new Vector2(0, parent.GetComponent<RectTransform>().sizeDelta.y / 3f);
        rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x / 2, 0);

        return circle;
    }

    private static void CreateStatusBoxLabel(Transform parent)
    {
        var labelText = new GameObject("OpenSkyPlus", typeof(RectTransform));
        labelText.transform.SetParent(parent, false);
        var text = labelText.AddComponent<Text>();
        text.text = "OpenSkyPlus";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleRight;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        var rectTransform = labelText.GetComponent<RectTransform>();
        var paddingWidth = parent.GetComponent<RectTransform>().rect.width * 0.02f;
        var paddingHeight = parent.GetComponent<RectTransform>().rect.height * 0.02f;
        rectTransform.anchorMin = new Vector2(1f / 3f + paddingWidth / parent
                .GetComponent<RectTransform>().rect.width,
            0 + paddingHeight / parent.GetComponent<RectTransform>().rect.height);
        rectTransform.anchorMax = new Vector2(1 - paddingWidth / parent
                .GetComponent<RectTransform>().rect.width,
            1 - paddingHeight / parent.GetComponent<RectTransform>().rect.height);
        rectTransform.offsetMin = new Vector2(paddingWidth, paddingHeight);
        rectTransform.offsetMax = new Vector2(-paddingWidth, -paddingHeight);
    }

    private static void BuildPopupConsole()
    {
        CreatePopupConsole();
        CreateCloseButton();
        CreateBottomButtons();
        CreateConsoleTextBox();
    }

    private static void TogglePopupConsole()
    {
        if (_popupConsole == null) return;
        _popupConsole.SetActive(!_popupConsole.activeSelf);
    }

    private static void CreatePopupConsole()
    {
        _popupConsole = new GameObject("PopupConsole", typeof(RectTransform));
        _popupConsole.SetActive(false);
        _popupConsole.transform.SetParent(_canvas.transform, false);
        var popupImage = _popupConsole.AddComponent<Image>();
        popupImage.color = new Color(0.25f, 0.25f, 1f);

        var rectTransform = _popupConsole.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        _popupConsole.AddComponent<Draggable>();
    }

    private static void CreateCloseButton()
    {
        // Close button
        var closeButton = new GameObject("CloseButton", typeof(RectTransform));
        closeButton.transform.SetParent(_popupConsole.transform, false);
        var buttonComponent = closeButton.AddComponent<Button>();
        buttonComponent.onClick.AddListener(TogglePopupConsole);
        var buttonText = closeButton.AddComponent<Text>();
        buttonText.text = "X";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;

        var buttonRect = closeButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.sizeDelta = new Vector2(20, 20);
        buttonRect.anchoredPosition = new Vector2(-10, -10);
    }

    private static void CreateBottomButtons()
    {
        string[] buttonTexts = ["Force Normal Mode", "Force Putting Mode", "Force Monitor Arm", "Resend Last Shot"];
        var popupRectTransform = _popupConsole.GetComponent<RectTransform>();
        var buttonHeight = popupRectTransform.rect.height / 10;

        for (var i = 0; i < buttonTexts.Length; i++)
        {
            var buttonObj = new GameObject(buttonTexts[i], typeof(Button), typeof(Image));
            buttonObj.transform.SetParent(_popupConsole.transform, false);

            var rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(i / (float)buttonTexts.Length + 0.01f, 0);
            rectTransform.anchorMax = new Vector2((i + 1) / (float)buttonTexts.Length - 0.01f, 0);
            rectTransform.sizeDelta = new Vector2(0, buttonHeight);
            rectTransform.anchoredPosition = new Vector2(0, -buttonHeight / 2);

            var textObj = new GameObject(string.Concat(buttonTexts[i], "Text"), typeof(Text));
            textObj.transform.SetParent(buttonObj.transform, false);
            var text = textObj.GetComponent<Text>();
            text.text = buttonTexts[i];
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            var textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.anchoredPosition = Vector2.zero;

            var buttonImage = buttonObj.GetComponent<Image>();
            buttonImage.color = Color.gray;
            Lazy<OpenSkyPlusApi> apiInstance = new(() => OpenSkyPlusApi.GetInstance());
            buttonObj.SetActive(apiInstance.IsValueCreated);

            var buttonText = buttonTexts[i];
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                switch (buttonText)
                {
                    case "Force Normal Mode":
                        apiInstance.Value.SetNormalMode();
                        break;
                    case "Force Putting Mode":
                        apiInstance.Value.SetPuttingMode();
                        break;
                    case "Force Monitor Arm":
                        apiInstance.Value.ReadyForNextShot();
                        break;
                    case "Resend Last Shot":
                        apiInstance.Value.ReplayLastShot();
                        break;
                }
            });

            // Only enable when the device is connected
            OpenSkyPlusApiInjector.MessageMonitorConnected += () => { buttonObj.SetActive(true); };
            OpenSkyPlusApiInjector.MessageMonitorDisconnected += () => { buttonObj.SetActive(false); };
        }
    }

    private static void CreateConsoleTextBox()
    {
        var consoleScrollView = new GameObject("ConsoleScrollView", typeof(Image), typeof(ScrollRect));
        consoleScrollView.transform.SetParent(_popupConsole.transform, false);
        _consoleScrollRect = consoleScrollView.GetComponent<ScrollRect>();
        consoleScrollView.GetComponent<Image>().color = Color.black;

        var scrollViewRectTransform = consoleScrollView.GetComponent<RectTransform>();
        scrollViewRectTransform.sizeDelta = new Vector2(0, -20);
        scrollViewRectTransform.anchoredPosition = new Vector2(0, -10);
        scrollViewRectTransform.anchorMin = new Vector2(0, 0);
        scrollViewRectTransform.anchorMax = new Vector2(1, 1);
        scrollViewRectTransform.pivot = new Vector2(0.5f, 0.5f);

        var consoleViewport = new GameObject("ConsoleViewport", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Mask));
        consoleViewport.transform.SetParent(consoleScrollView.transform, false);
        consoleViewport.GetComponent<Image>().color = Color.black;
        consoleViewport.GetComponent<Mask>().showMaskGraphic = true;

        var viewportRectTransform = consoleViewport.GetComponent<RectTransform>();
        viewportRectTransform.sizeDelta = new Vector2(-17, 0);
        viewportRectTransform.anchorMin = new Vector2(0, 0);
        viewportRectTransform.anchorMax = new Vector2(1, 1);
        viewportRectTransform.anchoredPosition = new Vector2(-1, 0);
        viewportRectTransform.pivot = new Vector2(0.5f, 0.5f);

        var consoleTextContainer = new GameObject("ConsoleTextContainer", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(VerticalLayoutGroup));
        consoleTextContainer.transform.SetParent(consoleViewport.transform, false);
        consoleTextContainer.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

        var textContainerRectTransform = consoleTextContainer.GetComponent<RectTransform>();
        textContainerRectTransform.sizeDelta = new Vector2(0, 0);
        textContainerRectTransform.anchorMin = new Vector2(0, 1);
        textContainerRectTransform.anchorMax = new Vector2(1, 1);
        textContainerRectTransform.pivot = new Vector2(0.5f, 1);

        var contentSizeFitter = consoleTextContainer.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _consoleScrollRect.content = textContainerRectTransform;
        _consoleScrollRect.viewport = viewportRectTransform;
        _consoleScrollRect.horizontal = false;
        _consoleScrollRect.vertical = true;
        _consoleScrollRect.scrollSensitivity = 10;

        var consoleTextObject = new GameObject("ConsoleText", typeof(Text));
        consoleTextObject.transform.SetParent(consoleTextContainer.transform, false);
        _consoleText = consoleTextObject.GetComponent<Text>();
        _consoleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _consoleText.fontSize = 14;
        _consoleText.color = Color.white;
        _consoleText.alignment = TextAnchor.UpperLeft;
        _consoleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _consoleText.verticalOverflow = VerticalWrapMode.Truncate;

        var consoleTextRectTransform = consoleTextObject.GetComponent<RectTransform>();
        consoleTextRectTransform.sizeDelta = new Vector2(0, 30);
        consoleTextRectTransform.anchorMin = new Vector2(0, 0);
        consoleTextRectTransform.anchorMax = new Vector2(1, 0);
        consoleTextRectTransform.pivot = new Vector2(0.5f, 0);

        var scrollbarGameObject = new GameObject("Scrollbar", typeof(Image), typeof(Scrollbar));
        scrollbarGameObject.transform.SetParent(consoleScrollView.transform, false);
        var scrollbar = scrollbarGameObject.GetComponent<Scrollbar>();
        _consoleScrollRect.verticalScrollbar = scrollbar;

        var scrollbarRectTransform = scrollbarGameObject.GetComponent<RectTransform>();
        scrollbarRectTransform.sizeDelta = new Vector2(10, 0);
        scrollbarRectTransform.anchorMin = new Vector2(1, 0);
        scrollbarRectTransform.anchorMax = new Vector2(1, 1);
        scrollbarRectTransform.pivot = new Vector2(1, 0.5f);
        scrollbarRectTransform.anchoredPosition = new Vector2(-2f, 0);

        var scrollbarHandle = new GameObject("ScrollbarHandle", typeof(Image));
        scrollbarHandle.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);
        scrollbarHandle.transform.SetParent(scrollbarGameObject.transform, false);
        scrollbar.handleRect = scrollbarHandle.GetComponent<RectTransform>();
        scrollbar.handleRect.sizeDelta = new Vector2(2, 2);
        scrollbar.targetGraphic = scrollbarHandle.GetComponent<Image>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.transition = Selectable.Transition.ColorTint;
        scrollbar.colors = ColorBlock.defaultColorBlock;

        LogToConsole();
    }

    // Method to add messages to the console _consoleText
    public static void LogToConsole(string message = null)
    {
        _consoleTextPlaceholder += message == null ? string.Empty : message + "\n";
        if (_consoleText == null || _consoleScrollRect == null)
            return;
        _consoleText.text = _consoleTextPlaceholder;
        _consoleScrollRect.verticalNormalizedPosition = 0f;
    }
}

// Draws the status light on the UI box
public class UiCircle : Image
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        const int segments = 360;
        const float angleStep = 360.0f / segments;
        var rect = GetPixelAdjustedRect();

        var paddingWidth = rect.width * 0.02f;
        var paddingHeight = rect.height * 0.02f;

        var radius = Mathf.Min(rect.width / 2, rect.height / 2) - Mathf.Max(paddingWidth, paddingHeight);
        var center = new Vector2(0, 0);

        vh.AddVert(center, color, new Vector2(0.5f, 0.5f));
        for (var i = 0; i <= segments; i++)
        {
            var angle = Mathf.Deg2Rad * angleStep * i;
            var perimeterPoint = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            vh.AddVert(perimeterPoint, color, Vector2.zero);
        }

        for (var i = 1; i <= segments; i++) vh.AddTriangle(0, i, i == segments ? 1 : i + 1);
    }
}

// Makes the window draggable
public class Draggable : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        transform.position += (Vector3)eventData.delta;
    }
}