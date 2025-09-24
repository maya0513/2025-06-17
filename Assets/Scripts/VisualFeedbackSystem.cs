using UnityEngine;
using UnityEngine.UI;

public class VisualFeedbackSystem : MonoBehaviour
{
    [Header("Feedback Bars")]
    public Slider leftWheelBar;
    public Slider rightWheelBar;

    [Header("Bar Visual Settings")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.gray;

    [Header("Animation Settings")]
    public float barUpdateSpeed = 2f;
    public bool enableSmoothAnimation = true;

    [Header("VIVE Tracker References")]
    public Transform leftWheelTracker;
    public Transform rightWheelTracker;

    private bool feedbackEnabled = false;
    private bool isRunning = false;

    // ハリボテ用の値
    private float leftWheelValue = 0f;
    private float rightWheelValue = 0f;

    // アニメーション用の現在値
    private float currentLeftValue = 0f;
    private float currentRightValue = 0f;

    private void Start()
    {
        InitializeBars();
    }

    private void InitializeBars()
    {
        if (leftWheelBar != null)
        {
            leftWheelBar.minValue = 0f;
            leftWheelBar.maxValue = 1f;
            leftWheelBar.value = 0f;
        }

        if (rightWheelBar != null)
        {
            rightWheelBar.minValue = 0f;
            rightWheelBar.maxValue = 1f;
            rightWheelBar.value = 0f;
        }

        UpdateBarColors();
    }

    private void Update()
    {
        if (isRunning && feedbackEnabled)
        {
            UpdateFeedbackValues();
            UpdateBarDisplay();
        }
    }

    public void SetFeedbackEnabled(bool enabled)
    {
        feedbackEnabled = enabled;
        UpdateBarColors();

        if (!enabled)
        {
            // フィードバック無効時はバーを0にリセット
            currentLeftValue = 0f;
            currentRightValue = 0f;
            UpdateBarValues();
        }
    }

    public void StartFeedback()
    {
        isRunning = true;
        Debug.Log($"Visual feedback started. Enabled: {feedbackEnabled}");
    }

    public void StopFeedback()
    {
        isRunning = false;
        ResetBars();
        Debug.Log("Visual feedback stopped");
    }

    private void UpdateFeedbackValues()
    {
        // ハリボテ実装：実際はVIVE Trackerから車輪の動きデータを取得
        if (leftWheelTracker != null && rightWheelTracker != null)
        {
            // 実装時はトラッカーの位置・速度データから車輪の推進力を計算
            leftWheelValue = CalculateWheelForce(leftWheelTracker);
            rightWheelValue = CalculateWheelForce(rightWheelTracker);
        }
        else
        {
            // ハリボテ用のサイン波デモ
            float time = Time.time;
            leftWheelValue = (Mathf.Sin(time * 0.5f) + 1f) * 0.5f;
            rightWheelValue = (Mathf.Sin(time * 0.7f) + 1f) * 0.5f;
        }
    }

    private float CalculateWheelForce(Transform tracker)
    {
        // ハリボテ実装：実際はトラッカーの速度や加速度から推進力を計算
        Vector3 velocity = tracker.GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero;
        float force = Mathf.Clamp01(velocity.magnitude * 0.1f);
        return force;
    }

    private void UpdateBarDisplay()
    {
        if (enableSmoothAnimation)
        {
            // スムーズなアニメーション
            currentLeftValue = Mathf.Lerp(currentLeftValue, leftWheelValue, barUpdateSpeed * Time.deltaTime);
            currentRightValue = Mathf.Lerp(currentRightValue, rightWheelValue, barUpdateSpeed * Time.deltaTime);
        }
        else
        {
            // 即座に更新
            currentLeftValue = leftWheelValue;
            currentRightValue = rightWheelValue;
        }

        UpdateBarValues();
    }

    private void UpdateBarValues()
    {
        if (leftWheelBar != null)
        {
            leftWheelBar.value = currentLeftValue;
        }

        if (rightWheelBar != null)
        {
            rightWheelBar.value = currentRightValue;
        }
    }

    private void UpdateBarColors()
    {
        Color targetColor = feedbackEnabled ? activeColor : inactiveColor;

        if (leftWheelBar != null && leftWheelBar.fillRect != null)
        {
            Image leftFillImage = leftWheelBar.fillRect.GetComponent<Image>();
            if (leftFillImage != null)
            {
                leftFillImage.color = targetColor;
            }
        }

        if (rightWheelBar != null && rightWheelBar.fillRect != null)
        {
            Image rightFillImage = rightWheelBar.fillRect.GetComponent<Image>();
            if (rightFillImage != null)
            {
                rightFillImage.color = targetColor;
            }
        }
    }

    private void ResetBars()
    {
        currentLeftValue = 0f;
        currentRightValue = 0f;
        UpdateBarValues();
    }

    // デバッグ用のパブリックメソッド
    public void SetTestValues(float left, float right)
    {
        leftWheelValue = Mathf.Clamp01(left);
        rightWheelValue = Mathf.Clamp01(right);
    }

    // ゲームオブジェクトの表示状態を設定
    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}