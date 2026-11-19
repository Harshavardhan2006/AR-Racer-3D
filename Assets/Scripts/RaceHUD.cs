using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaceHUD : MonoBehaviour
{
    private static readonly Color PanelColor = new Color(0.04f, 0.06f, 0.09f, 0.62f);

    private GameObject placementRoot;
    private GameObject gameplayRoot;
    private TextMeshProUGUI speedText;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI lapText;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI statusText;
    private CanvasGroup statusGroup;
    private HoldButton[] pads;
    private Image[] padImages;
    private Color[] padColors;
    private Sprite roundedSprite;
    private Sprite circleSprite;
    private string lastStatus = "";
    private float statusTimer;
    private bool statusSticky;
    private float statusTargetAlpha;

    private void Awake()
    {
        EnsureEventSystem();
        roundedSprite = BuildSprite(64, 20, true);
        circleSprite = BuildSprite(128, 64, false);
        BuildCanvas();
        ShowPlacement();
    }

    private void Start()
    {
        if (RaceManager.Instance != null)
            RaceManager.Instance.BindHud(speedText, timerText, lapText, coinText, statusText);
        else
            Debug.LogWarning("RaceHUD could not find a RaceManager in the scene.");
    }

    private void Update()
    {
        UpdatePadTouches();

        for (int i = 0; i < pads.Length; i++)
        {
            Color color = padColors[i];
            color.a = pads[i].IsHeld ? 0.95f : 0.5f;
            padImages[i].color = color;
            padImages[i].transform.localScale = Vector3.one * (pads[i].IsHeld ? 0.93f : 1f);
        }

        UpdateStatus();
    }

    private void UpdatePadTouches()
    {
        if (!gameplayRoot.activeInHierarchy)
            return;

        for (int i = 0; i < pads.Length; i++)
        {
            RectTransform rt = (RectTransform)pads[i].transform;
            Vector2 center = rt.TransformPoint(rt.rect.center);
            float radius = rt.rect.width * 0.5f * rt.lossyScale.x * 1.1f;
            bool held = false;

            for (int t = 0; t < Input.touchCount && !held; t++)
            {
                Touch touch = Input.GetTouch(t);

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    continue;

                held = Vector2.Distance(touch.position, center) <= radius;
            }

            if (!held && Input.mousePresent && Input.GetMouseButton(0))
                held = Vector2.Distance(Input.mousePosition, center) <= radius;

            pads[i].SetPressed(held);
        }
    }

    public void ShowPlacement()
    {
        placementRoot.SetActive(true);
        gameplayRoot.SetActive(false);
    }

    public void ShowGameplay()
    {
        placementRoot.SetActive(false);
        gameplayRoot.SetActive(true);
    }

    private void UpdateStatus()
    {
        string current = statusText.text;

        if (current != lastStatus)
        {
            lastStatus = current;
            statusSticky = current.Contains("COMPLETE");
            statusTimer = 3f;
            statusTargetAlpha = string.IsNullOrEmpty(current) ? 0f : 1f;
        }

        if (!statusSticky && statusTargetAlpha > 0f)
        {
            statusTimer -= Time.deltaTime;

            if (statusTimer <= 0f)
                statusTargetAlpha = 0f;
        }

        statusGroup.alpha = Mathf.MoveTowards(
            statusGroup.alpha, statusTargetAlpha, Time.deltaTime * 3f);
    }

    private void ResetTrack()
    {
        if (RaceManager.Instance != null)
            RaceManager.Instance.ResetRace();

        ARTrackPlacement placement = FindFirstObjectByType<ARTrackPlacement>();

        if (placement != null)
            placement.ResetPlacement();
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("RaceCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform safe = Stretch("SafeArea", canvasObject.transform);
        ApplySafeArea(safe);

        placementRoot = Stretch("Placement", safe).gameObject;
        gameplayRoot = Stretch("Gameplay", safe).gameObject;

        BuildPlacement(placementRoot.transform);
        BuildGameplay(gameplayRoot.transform);
    }

    private void BuildPlacement(Transform parent)
    {
        RectTransform panel = MakePanel("Prompt", parent,
            new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(1200f, 170f));

        TextMeshProUGUI text = AddText(panel,
            "Move your phone slowly to scan the floor, then tap a flat surface to place the track.",
            42f, TextAlignmentOptions.Center);
        text.margin = new Vector4(30f, 10f, 30f, 10f);
    }

    private void BuildGameplay(Transform parent)
    {
        Vector2 topLeft = new Vector2(0f, 1f);
        Vector2 topCenter = new Vector2(0.5f, 1f);
        Vector2 topRight = new Vector2(1f, 1f);
        Vector2 bottomCenter = new Vector2(0.5f, 0f);
        Vector2 bottomLeft = new Vector2(0f, 0f);
        Vector2 bottomRight = new Vector2(1f, 0f);

        lapText = AddText(MakePanel("Lap", parent, topLeft, topLeft,
            new Vector2(30f, -30f), new Vector2(270f, 100f)),
            "LAP  0/3", 46f, TextAlignmentOptions.Center);

        timerText = AddText(MakePanel("Timer", parent, topCenter, topCenter,
            new Vector2(0f, -30f), new Vector2(380f, 100f)),
            "TIME  0.0 s", 48f, TextAlignmentOptions.Center);

        coinText = AddText(MakePanel("Coins", parent, topRight, topRight,
            new Vector2(-30f, -30f), new Vector2(290f, 100f)),
            "COINS  0", 46f, TextAlignmentOptions.Center);

        speedText = AddText(MakePanel("Speed", parent, bottomCenter, bottomCenter,
            new Vector2(0f, 40f), new Vector2(400f, 110f)),
            "SPEED  0 km/h", 46f, TextAlignmentOptions.Center);

        RectTransform banner = MakePanel("Banner", parent,
            new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(1300f, 190f));
        statusGroup = banner.gameObject.AddComponent<CanvasGroup>();
        statusGroup.blocksRaycasts = false;
        statusGroup.interactable = false;
        statusText = AddText(banner, "", 60f, TextAlignmentOptions.Center);
        statusText.margin = new Vector4(30f, 10f, 30f, 10f);

        RectTransform resetRect = MakePanel("Reset", parent, topRight, topRight,
            new Vector2(-30f, -150f), new Vector2(290f, 90f));
        Image resetImage = resetRect.GetComponent<Image>();
        resetImage.raycastTarget = true;
        Button resetButton = resetRect.gameObject.AddComponent<Button>();
        resetButton.targetGraphic = resetImage;
        resetButton.onClick.AddListener(ResetTrack);
        AddText(resetRect, "RESET TRACK", 34f, TextAlignmentOptions.Center);

        Color steer = new Color(0.25f, 0.45f, 0.85f);
        Color gas = new Color(0.15f, 0.78f, 0.3f);
        Color brake = new Color(0.9f, 0.2f, 0.2f);
        Color reverse = new Color(0.95f, 0.6f, 0.1f);

        HoldButton left = CreatePad(parent, "Left", "<", HoldButton.ControlAction.Left,
            bottomLeft, new Vector2(50f, 60f), 230f, steer, 130f);
        HoldButton right = CreatePad(parent, "Right", ">", HoldButton.ControlAction.Right,
            bottomLeft, new Vector2(320f, 60f), 230f, steer, 130f);
        HoldButton gasPad = CreatePad(parent, "Gas", "GAS", HoldButton.ControlAction.Accelerate,
            bottomRight, new Vector2(-50f, 60f), 280f, gas, 56f);
        HoldButton brakePad = CreatePad(parent, "Brake", "BRAKE", HoldButton.ControlAction.Brake,
            bottomRight, new Vector2(-360f, 60f), 210f, brake, 44f);
        HoldButton reversePad = CreatePad(parent, "Reverse", "REV", HoldButton.ControlAction.Reverse,
            bottomRight, new Vector2(-360f, 290f), 150f, reverse, 38f);

        pads = new[] { left, right, gasPad, brakePad, reversePad };
        padImages = new Image[pads.Length];
        padColors = new[] { steer, steer, gas, brake, reverse };

        for (int i = 0; i < pads.Length; i++)
            padImages[i] = pads[i].GetComponent<Image>();
    }

    private HoldButton CreatePad(
        Transform parent,
        string objectName,
        string label,
        HoldButton.ControlAction action,
        Vector2 anchor,
        Vector2 position,
        float size,
        Color color,
        float fontSize)
    {
        RectTransform rt = NewRect(objectName, parent, anchor, anchor, anchor,
            position, new Vector2(size, size));

        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = circleSprite;
        image.color = color;
        image.raycastTarget = false;

        AddText(rt, label, fontSize, TextAlignmentOptions.Center);

        HoldButton hold = rt.gameObject.AddComponent<HoldButton>();
        hold.Configure(action);
        return hold;
    }

    private RectTransform MakePanel(
        string objectName,
        Transform parent,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 position,
        Vector2 size)
    {
        RectTransform rt = NewRect(objectName, parent, anchor, anchor, pivot, position, size);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = PanelColor;
        image.raycastTarget = false;
        return rt;
    }

    private static TextMeshProUGUI AddText(
        RectTransform parent, string value, float size, TextAlignmentOptions alignment)
    {
        RectTransform rt = Stretch("Text", parent);
        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform NewRect(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 position,
        Vector2 size)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return rt;
    }

    private static RectTransform Stretch(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static void ApplySafeArea(RectTransform rt)
    {
        Rect area = Screen.safeArea;
        Vector2 min = area.position;
        Vector2 max = area.position + area.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();

        if (eventSystem.GetComponent<BaseInputModule>() == null)
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
    }

    private static Sprite BuildSprite(int size, int radius, bool sliced)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[size * size];
        float r = radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                float cx = Mathf.Clamp(px, r, size - r);
                float cy = Mathf.Clamp(py, r, size - r);
                float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(r - distance + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        Vector4 border = sliced ? new Vector4(r, r, r, r) : Vector4.zero;

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border);
    }
}
