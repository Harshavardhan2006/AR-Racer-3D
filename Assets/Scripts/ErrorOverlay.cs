using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorOverlay : MonoBehaviour
{
    private readonly Queue<string> entries = new Queue<string>();
    private GameObject errorPanel;
    private TextMeshProUGUI errorText;
    private TextMeshProUGUI statusText;
    private string lastEntry = "";

    private void Awake()
    {
        GameObject canvasObject = new GameObject("DebugCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        errorPanel = new GameObject("ErrorPanel", typeof(RectTransform));
        errorPanel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = errorPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.2f);
        panelRect.anchorMax = new Vector2(0.85f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image background = errorPanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.75f);
        background.raycastTarget = false;

        errorText = NewText("ErrorText", errorPanel.transform, 26f, new Color(1f, 0.55f, 0.55f));
        RectTransform errorRect = errorText.GetComponent<RectTransform>();
        errorRect.anchorMin = Vector2.zero;
        errorRect.anchorMax = Vector2.one;
        errorRect.offsetMin = new Vector2(20f, 20f);
        errorRect.offsetMax = new Vector2(-20f, -20f);
        errorText.alignment = TextAlignmentOptions.TopLeft;
        errorPanel.SetActive(false);

        statusText = NewText("StatusText", canvasObject.transform, 30f, Color.yellow);
        RectTransform statusRect = statusText.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 1f);
        statusRect.anchorMax = new Vector2(0.5f, 1f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -150f);
        statusRect.sizeDelta = new Vector2(900f, 50f);
        statusText.alignment = TextAlignmentOptions.Center;
    }

    private void OnEnable()
    {
        Application.logMessageReceived += OnLog;
        InvokeRepeating(nameof(RefreshStatus), 0.1f, 0.1f);
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= OnLog;
        CancelInvoke(nameof(RefreshStatus));
    }

    private void RefreshStatus()
    {
        CarController car = FindFirstObjectByType<CarController>();
        HoldButton[] pads = FindObjectsByType<HoldButton>(FindObjectsSortMode.None);

        string held = "";

        foreach (HoldButton pad in pads)
        {
            if (pad.IsHeld)
                held += pad.name + " ";
        }

        statusText.text = car != null
            ? "car yes  held [" + held + "]  spd " + car.CurrentSpeedKmh.ToString("F1")
            : "car NO  held [" + held + "]";
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert)
            return;

        string entry = Shorten(condition, 220);

        if (!string.IsNullOrEmpty(stackTrace))
        {
            int end = stackTrace.IndexOf('\n');
            string firstLine = end > 0 ? stackTrace.Substring(0, end) : stackTrace;
            entry += "\n   at " + Shorten(firstLine, 170);
        }

        if (entry == lastEntry)
            return;

        lastEntry = entry;
        entries.Enqueue(entry);

        while (entries.Count > 4)
            entries.Dequeue();

        errorText.text = string.Join("\n\n", entries);
        errorPanel.SetActive(true);
    }

    private static string Shorten(string value, int max)
    {
        if (value == null)
            return "";

        return value.Length <= max ? value : value.Substring(0, max) + "...";
    }

    private static TextMeshProUGUI NewText(string objectName, Transform parent, float size, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }
}
