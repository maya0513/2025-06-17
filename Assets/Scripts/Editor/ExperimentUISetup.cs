using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ExperimentUISetup : EditorWindow
{
    [MenuItem("Experiment/Setup AR UI")]
    public static void ShowWindow()
    {
        GetWindow<ExperimentUISetup>("Experiment UI Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("AR実験UI自動セットアップ", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        if (GUILayout.Button("1. MainMenuCanvas作成"))
        {
            CreateMainMenuCanvas();
        }

        if (GUILayout.Button("2. FeedbackCanvas作成"))
        {
            CreateFeedbackCanvas();
        }

        if (GUILayout.Button("3. Manager GameObjects作成"))
        {
            CreateManagerObjects();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("各ボタンを順番に実行してください。\n詳細な手順は docs/UI設定手順.md を参照してください。", MessageType.Info);
    }

    private static void CreateMainMenuCanvas()
    {
        // MainMenuCanvas作成
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // World Space設定
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.position = new Vector3(0, 1.5f, 3f);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        canvasRect.sizeDelta = new Vector2(800, 600);

        // Canvas Scaler追加
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        scaler.matchWidthOrHeight = 0.5f;

        // GraphicRaycaster追加
        canvasGO.AddComponent<GraphicRaycaster>();

        // SubjectSelectionPanel作成
        CreateSubjectSelectionPanel(canvasGO.transform);

        // ExperimentPhasePanel作成
        CreateExperimentPhasePanel(canvasGO.transform);

        Debug.Log("MainMenuCanvas created successfully");
        Selection.activeGameObject = canvasGO;
    }

    private static void CreateSubjectSelectionPanel(Transform parent)
    {
        GameObject panel = CreateUIPanel("SubjectSelectionPanel", parent);

        // 新規被験者ボタン
        CreateButton("NewSubjectButton", "新規被験者", panel.transform);

        // 既存被験者ボタン
        CreateButton("ExistingSubjectButton", "既存被験者", panel.transform);

        // 被験者IDパネル（初期非表示）
        GameObject idPanel = CreateUIPanel("SubjectIdPanel", panel.transform);
        idPanel.SetActive(false);

        // ID入力フィールド
        CreateInputField("SubjectIdInputField", "被験者ID入力", idPanel.transform);

        // ID確定ボタン
        CreateButton("ConfirmIdButton", "確定", idPanel.transform);

        // グループ選択パネル（初期非表示）
        GameObject groupPanel = CreateUIPanel("GroupSelectionPanel", panel.transform);
        groupPanel.SetActive(false);

        CreateButton("VisualFeedbackButton", "視覚フィードバック群", groupPanel.transform);
        CreateButton("AudioFeedbackButton", "聴覚フィードバック群", groupPanel.transform);
        CreateButton("NoFeedbackButton", "フィードバックなし群", groupPanel.transform);
    }

    private static void CreateExperimentPhasePanel(Transform parent)
    {
        GameObject panel = CreateUIPanel("ExperimentPhasePanel", parent);
        panel.SetActive(false);

        CreateButton("PreTestButton", "事前テスト", panel.transform);
        CreateButton("TrainingButton", "トレーニング", panel.transform);
        CreateButton("PostTestButton", "事後テスト", panel.transform);
        CreateButton("RetentionTestButton", "保持テスト", panel.transform);

        // タスク選択パネル
        GameObject taskPanel = CreateUIPanel("TaskSelectionPanel", panel.transform);
        taskPanel.SetActive(false);

        CreateButton("ZigzagRunButton", "ジグザグ走", taskPanel.transform);
        CreateButton("FigureEightRunButton", "8の字走", taskPanel.transform);
        CreateButton("BasketballFigureEightRunButton", "バスケットボール8の字走", taskPanel.transform);

        // 実験開始パネル
        GameObject startPanel = CreateUIPanel("ExperimentStartPanel", panel.transform);
        startPanel.SetActive(false);

        CreateTextMeshPro("ExperimentInfoText", "実験情報", startPanel.transform);
        CreateButton("StartExperimentButton", "実験開始", startPanel.transform);
    }

    private static void CreateFeedbackCanvas()
    {
        GameObject canvasGO = new GameObject("FeedbackCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.position = new Vector3(0, 0, 3f);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        canvasRect.sizeDelta = new Vector2(800, 600);

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.SetActive(false);

        GameObject feedbackPanel = CreateUIPanel("VisualFeedbackPanel", canvasGO.transform);

        // 左車輪バー
        CreateFeedbackSlider("LeftWheelBar", "左車輪", feedbackPanel.transform, new Vector2(-200, 0));

        // 右車輪バー
        CreateFeedbackSlider("RightWheelBar", "右車輪", feedbackPanel.transform, new Vector2(200, 0));

        Debug.Log("FeedbackCanvas created successfully");
        Selection.activeGameObject = canvasGO;
    }

    private static void CreateManagerObjects()
    {
        // ExperimentManager
        GameObject expManager = new GameObject("ExperimentManager");
        expManager.AddComponent<SubjectData>();

        // SubjectSelectionManager
        GameObject subjectManager = new GameObject("SubjectSelectionManager");
        subjectManager.AddComponent<SubjectSelectionManager>();

        // ExperimentPhaseManager
        GameObject phaseManager = new GameObject("ExperimentPhaseManager");
        phaseManager.AddComponent<ExperimentPhaseManager>();

        // VisualFeedbackSystem
        GameObject feedbackSystem = new GameObject("VisualFeedbackSystem");
        feedbackSystem.AddComponent<VisualFeedbackSystem>();

        Debug.Log("Manager GameObjects created successfully");
        Debug.Log("Inspector でスクリプトの参照を手動で設定してください");
    }

    private static GameObject CreateUIPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(1, 1, 1, 0.1f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(20, 20, 20, 20);

        return panel;
    }

    private static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        Button button = buttonGO.AddComponent<Button>();

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 18;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.black;

        return buttonGO;
    }

    private static GameObject CreateInputField(string name, string placeholder, Transform parent)
    {
        GameObject inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent);

        RectTransform rect = inputGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 30);

        Image image = inputGO.AddComponent<Image>();
        image.color = Color.white;

        TMP_InputField inputField = inputGO.AddComponent<TMP_InputField>();

        // Placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputGO.transform);
        RectTransform placeholderRect = placeholderGO.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 14;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        // Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.fontSize = 14;
        textComp.color = Color.black;

        inputField.textComponent = textComp;
        inputField.placeholder = placeholderText;

        return inputGO;
    }

    private static GameObject CreateTextMeshPro(string name, string text, Transform parent)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent);

        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 100);

        TextMeshProUGUI textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 16;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;

        return textGO;
    }

    private static GameObject CreateFeedbackSlider(string name, string label, Transform parent, Vector2 position)
    {
        GameObject sliderGO = new GameObject(name);
        sliderGO.transform.SetParent(parent);

        RectTransform rect = sliderGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150, 300);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.direction = Slider.Direction.BottomToTop;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        // Background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform);
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Fill Area
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform);
        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform);
        RectTransform fillRect = fillGO.AddComponent<RectTransform>();

        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.green;

        slider.fillRect = fillRect;

        // Label
        GameObject labelGO = new GameObject(label + "Label");
        labelGO.transform.SetParent(sliderGO.transform);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(0, -180);
        labelRect.sizeDelta = new Vector2(150, 30);

        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;

        return sliderGO;
    }
}