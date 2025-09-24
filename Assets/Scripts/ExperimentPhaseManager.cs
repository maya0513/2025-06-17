using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperimentPhaseManager : MonoBehaviour
{
    [Header("Phase Selection UI")]
    public Button preTestButton;
    public Button trainingButton;
    public Button postTestButton;
    public Button retentionTestButton;

    [Header("Task Selection UI")]
    public GameObject taskSelectionPanel;
    public Button zigzagRunButton;
    public Button figureEightRunButton;
    public Button basketballFigureEightRunButton;

    [Header("Experiment Start")]
    public GameObject experimentStartPanel;
    public TextMeshProUGUI experimentInfoText;
    public Button startExperimentButton;

    [Header("Feedback Systems")]
    public VisualFeedbackSystem visualFeedbackSystem;

    private SubjectData.ExperimentPhase selectedPhase;
    private SubjectData.TaskType selectedTask;

    private void Start()
    {
        SetupEventListeners();
        ShowPhaseSelection();
    }

    private void SetupEventListeners()
    {
        preTestButton.onClick.AddListener(() => OnPhaseSelected(SubjectData.ExperimentPhase.PreTest));
        trainingButton.onClick.AddListener(() => OnPhaseSelected(SubjectData.ExperimentPhase.Training));
        postTestButton.onClick.AddListener(() => OnPhaseSelected(SubjectData.ExperimentPhase.PostTest));
        retentionTestButton.onClick.AddListener(() => OnPhaseSelected(SubjectData.ExperimentPhase.RetentionTest));

        zigzagRunButton.onClick.AddListener(() => OnTaskSelected(SubjectData.TaskType.ZigzagRun));
        figureEightRunButton.onClick.AddListener(() => OnTaskSelected(SubjectData.TaskType.FigureEightRun));
        basketballFigureEightRunButton.onClick.AddListener(() => OnTaskSelected(SubjectData.TaskType.BasketballFigureEightRun));

        startExperimentButton.onClick.AddListener(OnStartExperiment);
    }

    private void ShowPhaseSelection()
    {
        preTestButton.gameObject.SetActive(true);
        trainingButton.gameObject.SetActive(true);
        postTestButton.gameObject.SetActive(true);
        retentionTestButton.gameObject.SetActive(true);
        taskSelectionPanel.SetActive(false);
        experimentStartPanel.SetActive(false);
    }

    private void OnPhaseSelected(SubjectData.ExperimentPhase phase)
    {
        selectedPhase = phase;
        SubjectData.Instance.SetExperimentPhase(phase);
        ShowTaskSelection();
    }

    private void ShowTaskSelection()
    {
        preTestButton.gameObject.SetActive(false);
        trainingButton.gameObject.SetActive(false);
        postTestButton.gameObject.SetActive(false);
        retentionTestButton.gameObject.SetActive(false);
        taskSelectionPanel.SetActive(true);
    }

    private void OnTaskSelected(SubjectData.TaskType task)
    {
        selectedTask = task;
        SubjectData.Instance.SetTaskType(task);
        ShowExperimentStart();
    }

    private void ShowExperimentStart()
    {
        taskSelectionPanel.SetActive(false);
        experimentStartPanel.SetActive(true);
        UpdateExperimentInfo();
    }

    private void UpdateExperimentInfo()
    {
        if (experimentInfoText != null)
        {
            experimentInfoText.text = SubjectData.Instance.GetSubjectInfoString();
        }
    }

    private void OnStartExperiment()
    {
        SubjectData.Instance.StartSession();
        StartSelectedExperiment();
    }

    private void StartSelectedExperiment()
    {
        experimentStartPanel.SetActive(false);

        // フィードバックシステムを設定
        SetupFeedbackSystems();

        // 視覚フィードバックシステムをアクティブにする
        if (visualFeedbackSystem != null)
        {
            visualFeedbackSystem.gameObject.SetActive(true);
            visualFeedbackSystem.StartFeedback();
        }

        Debug.Log($"実験開始: {SubjectData.Instance.GetSubjectInfoString()}");
    }

    private void SetupFeedbackSystems()
    {
        var feedbackGroup = SubjectData.Instance.feedbackGroup;

        // 視覚フィードバックの設定
        if (visualFeedbackSystem != null)
        {
            bool enableVisual = feedbackGroup == SubjectData.FeedbackGroup.Visual;
            visualFeedbackSystem.SetFeedbackEnabled(enableVisual);
        }

        // 聴覚フィードバックの設定（後で実装予定）
        bool enableAudio = feedbackGroup == SubjectData.FeedbackGroup.Audio;
        Debug.Log($"Audio feedback enabled: {enableAudio}");
    }

    public void EndExperiment()
    {
        SubjectData.Instance.EndSession();

        if (visualFeedbackSystem != null)
        {
            visualFeedbackSystem.StopFeedback();
            visualFeedbackSystem.gameObject.SetActive(false);
        }

        ShowPhaseSelection();
        Debug.Log("実験終了");
    }
}