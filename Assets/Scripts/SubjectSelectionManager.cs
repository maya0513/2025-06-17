using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubjectSelectionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button newSubjectButton;
    public Button existingSubjectButton;
    public GameObject subjectIdPanel;
    public TMP_InputField subjectIdInputField;
    public Button confirmIdButton;

    [Header("New Subject Setup")]
    public GameObject groupSelectionPanel;
    public Button visualFeedbackButton;
    public Button audioFeedbackButton;
    public Button noFeedbackButton;

    [Header("Next Phase")]
    public ExperimentPhaseManager experimentPhaseManager;

    private int currentSubjectId = 1;
    private SubjectData.FeedbackGroup selectedGroup;

    private void Start()
    {
        SetupEventListeners();
        ShowSubjectSelection();
    }

    private void SetupEventListeners()
    {
        newSubjectButton.onClick.AddListener(OnNewSubjectSelected);
        existingSubjectButton.onClick.AddListener(OnExistingSubjectSelected);
        confirmIdButton.onClick.AddListener(OnConfirmSubjectId);

        visualFeedbackButton.onClick.AddListener(() => OnFeedbackGroupSelected(SubjectData.FeedbackGroup.Visual));
        audioFeedbackButton.onClick.AddListener(() => OnFeedbackGroupSelected(SubjectData.FeedbackGroup.Audio));
        noFeedbackButton.onClick.AddListener(() => OnFeedbackGroupSelected(SubjectData.FeedbackGroup.None));
    }

    private void ShowSubjectSelection()
    {
        newSubjectButton.gameObject.SetActive(true);
        existingSubjectButton.gameObject.SetActive(true);
        subjectIdPanel.SetActive(false);
        groupSelectionPanel.SetActive(false);
    }

    private void OnNewSubjectSelected()
    {
        currentSubjectId = GenerateNewSubjectId();
        ShowGroupSelection();
    }

    private void OnExistingSubjectSelected()
    {
        ShowSubjectIdInput();
    }

    private void ShowSubjectIdInput()
    {
        newSubjectButton.gameObject.SetActive(false);
        existingSubjectButton.gameObject.SetActive(false);
        subjectIdPanel.SetActive(true);
    }

    private void OnConfirmSubjectId()
    {
        if (int.TryParse(subjectIdInputField.text, out int subjectId) && subjectId > 0)
        {
            currentSubjectId = subjectId;
            ProceedToExperimentPhase();
        }
        else
        {
            Debug.LogWarning("Invalid subject ID entered");
        }
    }

    private void ShowGroupSelection()
    {
        newSubjectButton.gameObject.SetActive(false);
        existingSubjectButton.gameObject.SetActive(false);
        groupSelectionPanel.SetActive(true);
    }

    private void OnFeedbackGroupSelected(SubjectData.FeedbackGroup group)
    {
        selectedGroup = group;
        SubjectData.Instance.SetSubjectInfo(currentSubjectId, group);
        ProceedToExperimentPhase();
    }

    private void ProceedToExperimentPhase()
    {
        gameObject.SetActive(false);
        experimentPhaseManager.gameObject.SetActive(true);
    }

    private int GenerateNewSubjectId()
    {
        // ハリボテ実装：実際はファイルシステムから最新IDを読み取る
        return PlayerPrefs.GetInt("LastSubjectId", 0) + 1;
    }
}