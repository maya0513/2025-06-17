using UnityEngine;

public class SubjectData : MonoBehaviour
{
    public static SubjectData Instance { get; private set; }

    public enum FeedbackGroup
    {
        Visual,
        Audio,
        None
    }

    public enum ExperimentPhase
    {
        PreTest,
        Training,
        PostTest,
        RetentionTest
    }

    public enum TaskType
    {
        ZigzagRun,
        FigureEightRun,
        BasketballFigureEightRun
    }

    [Header("Subject Information")]
    public int subjectId;
    public FeedbackGroup feedbackGroup;
    public ExperimentPhase currentPhase;
    public TaskType currentTask;

    [Header("Session Data")]
    public bool isExperimentRunning = false;
    public float sessionStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSubjectInfo(int id, FeedbackGroup group)
    {
        subjectId = id;
        feedbackGroup = group;
        PlayerPrefs.SetInt("LastSubjectId", id);
        Debug.Log($"Subject {id} assigned to {group} feedback group");
    }

    public void SetExperimentPhase(ExperimentPhase phase)
    {
        currentPhase = phase;
        Debug.Log($"Experiment phase set to: {phase}");
    }

    public void SetTaskType(TaskType task)
    {
        currentTask = task;
        Debug.Log($"Task type set to: {task}");
    }

    public void StartSession()
    {
        isExperimentRunning = true;
        sessionStartTime = Time.time;
        Debug.Log($"Session started for Subject {subjectId}, Phase: {currentPhase}, Task: {currentTask}");
    }

    public void EndSession()
    {
        isExperimentRunning = false;
        float sessionDuration = Time.time - sessionStartTime;
        Debug.Log($"Session ended. Duration: {sessionDuration:F2} seconds");
    }

    public string GetSubjectInfoString()
    {
        return $"Subject {subjectId} - {feedbackGroup} Group - Phase: {currentPhase} - Task: {currentTask}";
    }
}