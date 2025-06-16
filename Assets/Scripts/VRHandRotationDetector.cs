using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// 高精度な手回し回転検出システム
    /// 両手での協調動作や複雑な回転パターンを検出
    /// </summary>
    public class VRHandRotationDetector : MonoBehaviour
    {
        [Header("回転検出設定")]
        [SerializeField]
        [Tooltip("回転検出の精度")]
        [Range(0.1f, 2f)]
        private float m_DetectionAccuracy = 1f;

        [SerializeField]
        [Tooltip("最小回転角度（度）")]
        [Range(1f, 30f)]
        private float m_MinRotationThreshold = 5f;

        [SerializeField]
        [Tooltip("連続回転の判定時間（秒）")]
        [Range(0.1f, 1f)]
        private float m_ContinuousRotationWindow = 0.4f;

        [SerializeField]
        [Tooltip("両手協調動作の検出")]
        private bool m_DetectBimanualRotation = true;

        [Header("フィルタリング設定")]
        [SerializeField]
        [Tooltip("ノイズフィルタの強度")]
        [Range(0.1f, 10f)]
        private float m_NoiseFilterStrength = 3f;

        [SerializeField]
        [Tooltip("滑らかさフィルタの強度")]
        [Range(0.1f, 10f)]
        private float m_SmoothingFilterStrength = 5f;

        // 回転検出データ構造
        [System.Serializable]
        public struct RotationData
        {
            public float angle;
            public float velocity;
            public float acceleration;
            public Vector3 axis;
            public float confidence;
            public float timestamp;
        }

        // 手の追跡データ
        private struct HandTrackingData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public float timestamp;
        }

        // 検出データの履歴
        private Queue<HandTrackingData> m_LeftHandHistory = new Queue<HandTrackingData>();
        private Queue<HandTrackingData> m_RightHandHistory = new Queue<HandTrackingData>();
        
        // 現在の回転状態
        private RotationData m_CurrentLeftRotation;
        private RotationData m_CurrentRightRotation;
        private RotationData m_CombinedRotation;

        // フィルタリング用
        private float[] m_LeftRotationFilter = new float[5];
        private float[] m_RightRotationFilter = new float[5];
        private int m_FilterIndex = 0;

        // イベント
        public System.Action<RotationData> OnLeftWheelRotation;
        public System.Action<RotationData> OnRightWheelRotation;
        public System.Action<RotationData> OnCombinedRotation;

        /// <summary>
        /// 現在の左手回転データ
        /// </summary>
        public RotationData currentLeftRotation => m_CurrentLeftRotation;

        /// <summary>
        /// 現在の右手回転データ
        /// </summary>
        public RotationData currentRightRotation => m_CurrentRightRotation;

        /// <summary>
        /// 現在の統合回転データ
        /// </summary>
        public RotationData combinedRotation => m_CombinedRotation;

        void Update()
        {
            // 手の追跡データを更新
            UpdateHandTracking();

            // 回転を検出・分析
            DetectRotations();

            // フィルタリングを適用
            ApplyFiltering();

            // 古いデータをクリーンアップ
            CleanupOldData();
        }

        /// <summary>
        /// 手の追跡データを更新
        /// </summary>
        private void UpdateHandTracking()
        {
            float currentTime = Time.time;

            // 左手の追跡
            var leftInteractor = FindLeftHandInteractor();
            if (leftInteractor != null)
            {
                HandTrackingData leftData = new HandTrackingData
                {
                    position = leftInteractor.transform.position,
                    rotation = leftInteractor.transform.rotation,
                    velocity = CalculateVelocity(m_LeftHandHistory, leftInteractor.transform.position),
                    timestamp = currentTime
                };
                m_LeftHandHistory.Enqueue(leftData);
            }

            // 右手の追跡
            var rightInteractor = FindRightHandInteractor();
            if (rightInteractor != null)
            {
                HandTrackingData rightData = new HandTrackingData
                {
                    position = rightInteractor.transform.position,
                    rotation = rightInteractor.transform.rotation,
                    velocity = CalculateVelocity(m_RightHandHistory, rightInteractor.transform.position),
                    timestamp = currentTime
                };
                m_RightHandHistory.Enqueue(rightData);
            }
        }

        /// <summary>
        /// 左手のインタラクターを検索
        /// </summary>
        private Transform FindLeftHandInteractor()
        {
            var interactors = FindObjectsByType<XRDirectInteractor>(FindObjectsSortMode.None);
            foreach (var interactor in interactors)
            {
                if (interactor.name.ToLower().Contains("left"))
                    return interactor.transform;
            }
            return null;
        }

        /// <summary>
        /// 右手のインタラクターを検索
        /// </summary>
        private Transform FindRightHandInteractor()
        {
            var interactors = FindObjectsByType<XRDirectInteractor>(FindObjectsSortMode.None);
            foreach (var interactor in interactors)
            {
                if (interactor.name.ToLower().Contains("right"))
                    return interactor.transform;
            }
            return null;
        }

        /// <summary>
        /// 速度を計算
        /// </summary>
        private Vector3 CalculateVelocity(Queue<HandTrackingData> history, Vector3 currentPosition)
        {
            if (history.Count == 0)
                return Vector3.zero;

            var lastData = history.ToArray()[history.Count - 1];
            float deltaTime = Time.time - lastData.timestamp;
            
            if (deltaTime > 0)
                return (currentPosition - lastData.position) / deltaTime;
            
            return Vector3.zero;
        }

        /// <summary>
        /// 回転を検出・分析
        /// </summary>
        private void DetectRotations()
        {
            // 左手の回転検出
            m_CurrentLeftRotation = AnalyzeHandRotation(m_LeftHandHistory, true);
            
            // 右手の回転検出
            m_CurrentRightRotation = AnalyzeHandRotation(m_RightHandHistory, false);

            // 両手協調動作の検出
            if (m_DetectBimanualRotation)
                m_CombinedRotation = AnalyzeBimanualRotation();

            // イベント発火
            if (m_CurrentLeftRotation.confidence > 0.5f)
                OnLeftWheelRotation?.Invoke(m_CurrentLeftRotation);

            if (m_CurrentRightRotation.confidence > 0.5f)
                OnRightWheelRotation?.Invoke(m_CurrentRightRotation);

            if (m_CombinedRotation.confidence > 0.5f)
                OnCombinedRotation?.Invoke(m_CombinedRotation);
        }

        /// <summary>
        /// 手の回転を分析
        /// </summary>
        private RotationData AnalyzeHandRotation(Queue<HandTrackingData> handHistory, bool isLeftHand)
        {
            RotationData rotationData = new RotationData();

            if (handHistory.Count < 3)
                return rotationData;

            var dataArray = handHistory.ToArray();
            int dataCount = dataArray.Length;

            // 回転中心の推定（タイヤの位置を基準とする）
            Vector3 wheelCenter = transform.position;

            // 最近の複数の点から回転を分析
            List<float> angleChanges = new List<float>();
            List<float> timeDeltas = new List<float>();

            for (int i = 1; i < Mathf.Min(dataCount, 5); i++)
            {
                Vector3 prevPos = dataArray[dataCount - i - 1].position;
                Vector3 currPos = dataArray[dataCount - i].position;
                float timeDelta = dataArray[dataCount - i].timestamp - dataArray[dataCount - i - 1].timestamp;

                if (timeDelta > 0.001f)
                {
                    // タイヤ中心からの相対位置
                    Vector3 prevRelative = prevPos - wheelCenter;
                    Vector3 currRelative = currPos - wheelCenter;

                    // Y軸周りの回転を計算
                    Vector3 prevProjected = Vector3.ProjectOnPlane(prevRelative, Vector3.up);
                    Vector3 currProjected = Vector3.ProjectOnPlane(currRelative, Vector3.up);

                    if (prevProjected.magnitude > 0.01f && currProjected.magnitude > 0.01f)
                    {
                        float angleChange = Vector3.SignedAngle(prevProjected, currProjected, Vector3.up);
                        
                        if (Mathf.Abs(angleChange) > m_MinRotationThreshold * timeDelta)
                        {
                            angleChanges.Add(angleChange);
                            timeDeltas.Add(timeDelta);
                        }
                    }
                }
            }

            // 回転データを統計的に処理
            if (angleChanges.Count > 0)
            {
                float totalAngle = 0f;
                float totalTime = 0f;
                float variance = 0f;

                // 平均と合計を計算
                for (int i = 0; i < angleChanges.Count; i++)
                {
                    totalAngle += angleChanges[i];
                    totalTime += timeDeltas[i];
                }

                float avgAngle = totalAngle / angleChanges.Count;
                
                // 分散を計算（信頼度の指標）
                for (int i = 0; i < angleChanges.Count; i++)
                {
                    variance += Mathf.Pow(angleChanges[i] - avgAngle, 2);
                }
                variance /= angleChanges.Count;

                // 回転データを構築
                rotationData.angle = totalAngle;
                rotationData.velocity = totalAngle / (totalTime > 0 ? totalTime : Time.deltaTime);
                rotationData.axis = Vector3.up;
                rotationData.confidence = Mathf.Clamp01(1f - (variance / 100f)); // 低分散ほど高信頼度
                rotationData.timestamp = Time.time;

                // 左タイヤの場合は方向を調整
                if (isLeftHand)
                {
                    rotationData.velocity *= -1f;
                    rotationData.angle *= -1f;
                }
            }

            return rotationData;
        }

        /// <summary>
        /// 両手協調動作の分析
        /// </summary>
        private RotationData AnalyzeBimanualRotation()
        {
            RotationData combinedData = new RotationData();

            // 両手の回転データが有効な場合のみ処理
            if (m_CurrentLeftRotation.confidence > 0.3f && m_CurrentRightRotation.confidence > 0.3f)
            {
                // 同期度を計算
                float velocityDifference = Mathf.Abs(m_CurrentLeftRotation.velocity - m_CurrentRightRotation.velocity);
                float synchronization = Mathf.Clamp01(1f - (velocityDifference / 10f));

                if (synchronization > 0.5f)
                {
                    // 両手の動作を統合
                    combinedData.velocity = (m_CurrentLeftRotation.velocity + m_CurrentRightRotation.velocity) * 0.5f;
                    combinedData.angle = (m_CurrentLeftRotation.angle + m_CurrentRightRotation.angle) * 0.5f;
                    combinedData.axis = Vector3.up;
                    combinedData.confidence = (m_CurrentLeftRotation.confidence + m_CurrentRightRotation.confidence) * 0.5f * synchronization;
                    combinedData.timestamp = Time.time;
                }
            }

            return combinedData;
        }

        /// <summary>
        /// フィルタリングを適用
        /// </summary>
        private void ApplyFiltering()
        {
            // 移動平均フィルタを適用
            m_LeftRotationFilter[m_FilterIndex] = m_CurrentLeftRotation.velocity;
            m_RightRotationFilter[m_FilterIndex] = m_CurrentRightRotation.velocity;
            
            m_FilterIndex = (m_FilterIndex + 1) % m_LeftRotationFilter.Length;

            // フィルタ済みの値を計算
            float leftFiltered = CalculateMovingAverage(m_LeftRotationFilter);
            float rightFiltered = CalculateMovingAverage(m_RightRotationFilter);

            // フィルタ済みデータで更新
            RotationData filteredLeft = m_CurrentLeftRotation;
            filteredLeft.velocity = leftFiltered;

            RotationData filteredRight = m_CurrentRightRotation;
            filteredRight.velocity = rightFiltered;

            m_CurrentLeftRotation = filteredLeft;
            m_CurrentRightRotation = filteredRight;
        }

        /// <summary>
        /// 移動平均を計算
        /// </summary>
        private float CalculateMovingAverage(float[] values)
        {
            float sum = 0f;
            int count = 0;

            foreach (float value in values)
            {
                if (!float.IsNaN(value) && !float.IsInfinity(value))
                {
                    sum += value;
                    count++;
                }
            }

            return count > 0 ? sum / count : 0f;
        }

        /// <summary>
        /// 古いデータをクリーンアップ
        /// </summary>
        private void CleanupOldData()
        {
            float currentTime = Time.time;
            float cutoffTime = currentTime - m_ContinuousRotationWindow;

            // 左手データのクリーンアップ
            while (m_LeftHandHistory.Count > 0 && m_LeftHandHistory.Peek().timestamp < cutoffTime)
                m_LeftHandHistory.Dequeue();

            // 右手データのクリーンアップ
            while (m_RightHandHistory.Count > 0 && m_RightHandHistory.Peek().timestamp < cutoffTime)
                m_RightHandHistory.Dequeue();

            // 最大サイズ制限
            while (m_LeftHandHistory.Count > 20)
                m_LeftHandHistory.Dequeue();

            while (m_RightHandHistory.Count > 20)
                m_RightHandHistory.Dequeue();
        }

        /// <summary>
        /// デバッグ情報の描画
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                // 左手の回転を可視化
                if (m_CurrentLeftRotation.confidence > 0.5f)
                {
                    Gizmos.color = Color.blue;
                    Vector3 leftDirection = Vector3.forward * m_CurrentLeftRotation.velocity * 0.1f;
                    Gizmos.DrawRay(transform.position + Vector3.left * 0.5f, leftDirection);
                }

                // 右手の回転を可視化
                if (m_CurrentRightRotation.confidence > 0.5f)
                {
                    Gizmos.color = Color.red;
                    Vector3 rightDirection = Vector3.forward * m_CurrentRightRotation.velocity * 0.1f;
                    Gizmos.DrawRay(transform.position + Vector3.right * 0.5f, rightDirection);
                }

                // 統合回転を可視化
                if (m_CombinedRotation.confidence > 0.5f)
                {
                    Gizmos.color = Color.green;
                    Vector3 combinedDirection = Vector3.forward * m_CombinedRotation.velocity * 0.1f;
                    Gizmos.DrawRay(transform.position, combinedDirection);
                }
            }
        }
    }
}