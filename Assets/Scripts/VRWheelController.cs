using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// VR車いすの個別タイヤコントローラー
    /// 手回し操作の検出とタイヤの回転を管理
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class VRWheelController : MonoBehaviour
    {
        [Header("タイヤ設定")]
        [SerializeField]
        [Tooltip("タイヤの半径（メートル）")]
        [Range(0.1f, 1f)]
        private float m_WheelRadius = 0.3f;

        [SerializeField]
        [Tooltip("手回し感度")]
        [Range(0.1f, 5f)]
        private float m_RotationSensitivity = 2f;

        [SerializeField]
        [Tooltip("回転の減衰率")]
        [Range(0.1f, 10f)]
        private float m_RotationDamping = 3f;

        [Header("手回し検出設定")]
        [SerializeField]
        [Tooltip("手回し検出の最小角度")]
        [Range(1f, 45f)]
        private float m_MinRotationAngle = 10f;

        [SerializeField]
        [Tooltip("連続回転検出の時間窓")]
        [Range(0.1f, 1f)]
        private float m_RotationTimeWindow = 0.3f;

        // コンポーネント参照
        private XRGrabInteractable m_GrabInteractable;
        private VRWheelchairController m_WheelchairController;

        // 状態変数
        private bool m_IsLeftWheel;
        private float m_CurrentRotationSpeed;
        private float m_AccumulatedRotation;
        private float m_VisualRotation;

        // 手回し検出用
        private struct HandMovementData
        {
            public Vector3 position;
            public float time;
            public float angle;
        }

        private List<HandMovementData> m_LeftHandMovements = new List<HandMovementData>();
        private List<HandMovementData> m_RightHandMovements = new List<HandMovementData>();

        // グラブ状態
        private bool m_IsGrabbed;
        private Vector3 m_PreviousGrabPosition;
        private IXRSelectInteractor m_CurrentInteractor;

        /// <summary>
        /// 現在のタイヤ回転速度を取得
        /// </summary>
        /// <returns>回転速度（rad/s）</returns>
        public float GetWheelRotationSpeed()
        {
            return m_CurrentRotationSpeed;
        }

        /// <summary>
        /// コントローラーの初期化
        /// </summary>
        /// <param name="wheelchairController">親の車いすコントローラー</param>
        /// <param name="isLeftWheel">左タイヤかどうか</param>
        public void Initialize(VRWheelchairController wheelchairController, bool isLeftWheel)
        {
            m_WheelchairController = wheelchairController;
            m_IsLeftWheel = isLeftWheel;
        }

        void Awake()
        {
            m_GrabInteractable = GetComponent<XRGrabInteractable>();
            
            // XRGrabInteractableのイベント設定
            m_GrabInteractable.selectEntered.AddListener(OnGrabStart);
            m_GrabInteractable.selectExited.AddListener(OnGrabEnd);
        }

        void Update()
        {
            if (m_IsGrabbed && m_CurrentInteractor != null)
            {
                DetectHandRotation();
            }

            // 回転速度の減衰
            ApplyRotationDamping();

            // 古い手の動きデータをクリーンアップ
            CleanupOldMovementData();
        }

        /// <summary>
        /// グラブ開始時の処理
        /// </summary>
        private void OnGrabStart(SelectEnterEventArgs args)
        {
            m_IsGrabbed = true;
            m_CurrentInteractor = args.interactorObject;
            m_PreviousGrabPosition = GetInteractorPosition();
        }

        /// <summary>
        /// グラブ終了時の処理
        /// </summary>
        private void OnGrabEnd(SelectExitEventArgs args)
        {
            m_IsGrabbed = false;
            m_CurrentInteractor = null;
            
            // 手の動きデータをクリア
            m_LeftHandMovements.Clear();
            m_RightHandMovements.Clear();
        }

        /// <summary>
        /// 手回し回転の検出
        /// </summary>
        private void DetectHandRotation()
        {
            Vector3 currentGrabPosition = GetInteractorPosition();
            
            // タイヤ中心からの相対位置を計算
            Vector3 wheelCenter = transform.position;
            Vector3 currentRelative = currentGrabPosition - wheelCenter;
            Vector3 previousRelative = m_PreviousGrabPosition - wheelCenter;

            // タイヤ平面への投影（Y軸周りの回転を検出）
            Vector3 currentProjected = Vector3.ProjectOnPlane(currentRelative, transform.up);
            Vector3 previousProjected = Vector3.ProjectOnPlane(previousRelative, transform.up);

            if (currentProjected.magnitude > 0.01f && previousProjected.magnitude > 0.01f)
            {
                // 角度変化を計算
                float angleChange = Vector3.SignedAngle(previousProjected, currentProjected, transform.up);
                
                if (Mathf.Abs(angleChange) > m_MinRotationAngle * Time.deltaTime)
                {
                    // 手の動きを記録
                    RecordHandMovement(currentGrabPosition, angleChange);
                    
                    // 回転速度を更新
                    UpdateRotationSpeed(angleChange);
                }
            }

            m_PreviousGrabPosition = currentGrabPosition;
        }

        /// <summary>
        /// 手の動きを記録
        /// </summary>
        private void RecordHandMovement(Vector3 position, float angleChange)
        {
            HandMovementData movementData = new HandMovementData
            {
                position = position,
                time = Time.time,
                angle = angleChange
            };

            // インタラクターの種類に応じて記録（簡易的に位置で判定）
            bool isLeftHand = IsLeftHandInteractor();
            
            if (isLeftHand)
                m_LeftHandMovements.Add(movementData);
            else
                m_RightHandMovements.Add(movementData);
        }

        /// <summary>
        /// 左手のインタラクターかどうかを判定
        /// </summary>
        private bool IsLeftHandInteractor()
        {
            if (m_CurrentInteractor?.transform != null)
            {
                // XROriginを基準とした相対位置で判定
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                {
                    Vector3 relativePos = xrOrigin.transform.InverseTransformPoint(m_CurrentInteractor.transform.position);
                    return relativePos.x < 0; // 左側にあるかどうか
                }
            }
            return false;
        }

        /// <summary>
        /// 回転速度を更新
        /// </summary>
        private void UpdateRotationSpeed(float angleChange)
        {
            // 角度変化から回転速度を計算（rad/s）
            float angularVelocity = (angleChange * Mathf.Deg2Rad) / Time.deltaTime;
            
            // 感度を適用
            angularVelocity *= m_RotationSensitivity;
            
            // 左タイヤの場合は回転方向を反転
            if (m_IsLeftWheel)
                angularVelocity *= -1f;
            
            // 回転速度を更新（滑らかに変化）
            m_CurrentRotationSpeed = Mathf.Lerp(m_CurrentRotationSpeed, angularVelocity, Time.deltaTime * 10f);
            
            // 累積回転を更新
            m_AccumulatedRotation += angleChange;
        }

        /// <summary>
        /// 回転の減衰を適用
        /// </summary>
        private void ApplyRotationDamping()
        {
            if (!m_IsGrabbed)
            {
                m_CurrentRotationSpeed = Mathf.Lerp(m_CurrentRotationSpeed, 0f, Time.deltaTime * m_RotationDamping);
            }
        }

        /// <summary>
        /// 古い手の動きデータをクリーンアップ
        /// </summary>
        private void CleanupOldMovementData()
        {
            float currentTime = Time.time;
            float cutoffTime = currentTime - m_RotationTimeWindow;

            m_LeftHandMovements.RemoveAll(data => data.time < cutoffTime);
            m_RightHandMovements.RemoveAll(data => data.time < cutoffTime);
        }

        /// <summary>
        /// インタラクターの位置を取得
        /// </summary>
        private Vector3 GetInteractorPosition()
        {
            if (m_CurrentInteractor?.transform != null)
                return m_CurrentInteractor.transform.position;
            return transform.position;
        }

        /// <summary>
        /// 視覚的な回転を更新
        /// </summary>
        public void UpdateVisualRotation()
        {
            // 線速度から角速度を計算
            float linearSpeed = m_CurrentRotationSpeed * m_WheelRadius;
            float angularSpeed = linearSpeed / m_WheelRadius;
            
            // 視覚的回転を更新
            m_VisualRotation += angularSpeed * Time.deltaTime * Mathf.Rad2Deg;
            
            // タイヤを回転させる
            Vector3 rotation = transform.localEulerAngles;
            rotation.x = m_VisualRotation;
            transform.localEulerAngles = rotation;
        }

        /// <summary>
        /// デバッグ情報の描画
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // タイヤの円を描画
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_WheelRadius);

            // 回転方向を表示
            if (Application.isPlaying && Mathf.Abs(m_CurrentRotationSpeed) > 0.01f)
            {
                Gizmos.color = m_CurrentRotationSpeed > 0 ? Color.green : Color.red;
                Vector3 direction = transform.forward * (m_CurrentRotationSpeed > 0 ? 1 : -1);
                Gizmos.DrawRay(transform.position, direction * 0.5f);
            }

            // グラブ状態の表示
            if (m_IsGrabbed)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
            }
        }
    }
}