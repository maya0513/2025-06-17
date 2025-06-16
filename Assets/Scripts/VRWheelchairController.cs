using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// VR車いすシステムのメインコントローラー
    /// プレイヤーがタイヤを手回しして移動するシステムを管理
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VRWheelchairController : MonoBehaviour
    {
        [Header("車いす設定")]
        [SerializeField]
        [Tooltip("車いす全体のRigidbody")]
        private Rigidbody m_WheelchairRigidbody;

        [SerializeField]
        [Tooltip("左タイヤのTransform")]
        private Transform m_LeftWheel;

        [SerializeField]
        [Tooltip("右タイヤのTransform")]
        private Transform m_RightWheel;

        [SerializeField]
        [Tooltip("プレイヤーの座る位置")]
        private Transform m_SeatPosition;

        [Header("移動設定")]
        [SerializeField]
        [Tooltip("最大移動速度 (m/s)")]
        [Range(0.1f, 10f)]
        private float m_MaxSpeed = 3f;

        [SerializeField]
        [Tooltip("加速度")]
        [Range(0.1f, 20f)]
        private float m_Acceleration = 5f;

        [SerializeField]
        [Tooltip("減速係数")]
        [Range(0.1f, 10f)]
        private float m_Deceleration = 2f;

        [SerializeField]
        [Tooltip("回転感度")]
        [Range(0.1f, 5f)]
        private float m_TurnSensitivity = 1f;

        [Header("手回し検出設定")]
        [SerializeField]
        [Tooltip("手回し検出の最小距離")]
        [Range(0.01f, 0.5f)]
        private float m_MinHandMovementDistance = 0.05f;

        [SerializeField]
        [Tooltip("手回し検出の最大時間間隔")]
        [Range(0.1f, 2f)]
        private float m_MaxHandMovementInterval = 0.5f;

        [Header("物理設定")]
        [SerializeField]
        [Tooltip("重心の位置オフセット")]
        private Vector3 m_CenterOfMassOffset = new Vector3(0, -0.5f, 0);

        [Header("XROrigin参照")]
        [SerializeField]
        [Tooltip("XROriginの参照（プレイヤー位置制御用）")]
        private XROrigin m_XROrigin;

        // 左右のタイヤコントローラー
        private VRWheelController m_LeftWheelController;
        private VRWheelController m_RightWheelController;

        // 現在の移動状態
        private Vector3 m_CurrentVelocity;
        private float m_LeftWheelSpeed;
        private float m_RightWheelSpeed;

        // XROriginの初期相対位置
        private Vector3 m_InitialXROriginOffset;
        private bool m_XROriginInitialized = false;

        /// <summary>
        /// 左タイヤの回転速度
        /// </summary>
        public float leftWheelSpeed => m_LeftWheelSpeed;

        /// <summary>
        /// 右タイヤの回転速度
        /// </summary>
        public float rightWheelSpeed => m_RightWheelSpeed;

        /// <summary>
        /// 現在の移動速度
        /// </summary>
        public Vector3 currentVelocity => m_CurrentVelocity;

        void Awake()
        {
            // Rigidbodyの設定
            if (m_WheelchairRigidbody == null)
                m_WheelchairRigidbody = GetComponent<Rigidbody>();

            m_WheelchairRigidbody.centerOfMass = m_CenterOfMassOffset;

            // XROriginの自動検索
            if (m_XROrigin == null)
                m_XROrigin = FindFirstObjectByType<XROrigin>();

            // タイヤコントローラーの初期化
            InitializeWheelControllers();
        }

        void Start()
        {
            // XROriginの初期位置を記録
            InitializeXROriginPosition();
        }

        void FixedUpdate()
        {
            // タイヤの回転速度を取得
            UpdateWheelSpeeds();

            // 車いすの移動を計算・適用
            ApplyWheelchairMovement();

            // XROriginの位置を更新
            UpdateXROriginPosition();
        }

        /// <summary>
        /// タイヤコントローラーの初期化
        /// </summary>
        private void InitializeWheelControllers()
        {
            if (m_LeftWheel != null)
            {
                m_LeftWheelController = m_LeftWheel.GetComponent<VRWheelController>();
                if (m_LeftWheelController == null)
                {
                    m_LeftWheelController = m_LeftWheel.gameObject.AddComponent<VRWheelController>();
                    m_LeftWheelController.Initialize(this, true);
                }
            }

            if (m_RightWheel != null)
            {
                m_RightWheelController = m_RightWheel.GetComponent<VRWheelController>();
                if (m_RightWheelController == null)
                {
                    m_RightWheelController = m_RightWheel.gameObject.AddComponent<VRWheelController>();
                    m_RightWheelController.Initialize(this, false);
                }
            }
        }

        /// <summary>
        /// XROriginの初期位置を設定
        /// </summary>
        private void InitializeXROriginPosition()
        {
            if (m_XROrigin != null && m_SeatPosition != null && !m_XROriginInitialized)
            {
                m_InitialXROriginOffset = m_XROrigin.transform.position - m_SeatPosition.position;
                m_XROriginInitialized = true;
            }
        }

        /// <summary>
        /// タイヤの回転速度を更新
        /// </summary>
        private void UpdateWheelSpeeds()
        {
            m_LeftWheelSpeed = m_LeftWheelController != null ? m_LeftWheelController.GetWheelRotationSpeed() : 0f;
            m_RightWheelSpeed = m_RightWheelController != null ? m_RightWheelController.GetWheelRotationSpeed() : 0f;
        }

        /// <summary>
        /// 車いすの移動を適用
        /// </summary>
        private void ApplyWheelchairMovement()
        {
            // 左右のタイヤ速度から前進・回転を計算
            float forwardSpeed = (m_LeftWheelSpeed + m_RightWheelSpeed) * 0.5f;
            float turnSpeed = (m_RightWheelSpeed - m_LeftWheelSpeed) * m_TurnSensitivity;

            // 最大速度制限
            forwardSpeed = Mathf.Clamp(forwardSpeed, -m_MaxSpeed, m_MaxSpeed);
            turnSpeed = Mathf.Clamp(turnSpeed, -m_MaxSpeed, m_MaxSpeed);

            // 前進移動
            Vector3 forwardDirection = transform.forward * forwardSpeed;
            Vector3 targetVelocity = new Vector3(forwardDirection.x, m_WheelchairRigidbody.linearVelocity.y, forwardDirection.z);

            // 滑らかな加速・減速
            m_CurrentVelocity = Vector3.MoveTowards(m_CurrentVelocity, targetVelocity, 
                (forwardSpeed > 0 ? m_Acceleration : m_Deceleration) * Time.fixedDeltaTime);

            m_WheelchairRigidbody.linearVelocity = m_CurrentVelocity;

            // 回転
            if (Mathf.Abs(turnSpeed) > 0.01f)
            {
                float angularVelocityY = turnSpeed * Mathf.Rad2Deg;
                Vector3 currentAngularVelocity = m_WheelchairRigidbody.angularVelocity;
                currentAngularVelocity.y = angularVelocityY;
                m_WheelchairRigidbody.angularVelocity = currentAngularVelocity;
            }

            // タイヤの視覚的回転を更新
            UpdateWheelVisualRotation();
        }

        /// <summary>
        /// タイヤの視覚的回転を更新
        /// </summary>
        private void UpdateWheelVisualRotation()
        {
            if (m_LeftWheel != null && m_LeftWheelController != null)
                m_LeftWheelController.UpdateVisualRotation();

            if (m_RightWheel != null && m_RightWheelController != null)
                m_RightWheelController.UpdateVisualRotation();
        }

        /// <summary>
        /// XROriginの位置を車いすに追従させる
        /// </summary>
        private void UpdateXROriginPosition()
        {
            if (m_XROrigin != null && m_SeatPosition != null && m_XROriginInitialized)
            {
                Vector3 targetPosition = m_SeatPosition.position + m_InitialXROriginOffset;
                m_XROrigin.transform.position = targetPosition;
                
                // 回転も車いすに合わせる（Y軸のみ）
                Vector3 currentEuler = m_XROrigin.transform.eulerAngles;
                currentEuler.y = transform.eulerAngles.y;
                m_XROrigin.transform.eulerAngles = currentEuler;
            }
        }

        /// <summary>
        /// 車いすをリセット（転倒時などに使用）
        /// </summary>
        public void ResetWheelchair()
        {
            m_WheelchairRigidbody.linearVelocity = Vector3.zero;
            m_WheelchairRigidbody.angularVelocity = Vector3.zero;
            m_CurrentVelocity = Vector3.zero;

            // 正立姿勢に戻す
            Vector3 currentPosition = transform.position;
            transform.rotation = Quaternion.identity;
            transform.position = currentPosition;
        }

        /// <summary>
        /// デバッグ情報の描画
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // 重心の表示
            Gizmos.color = Color.red;
            Vector3 centerOfMass = transform.TransformPoint(m_CenterOfMassOffset);
            Gizmos.DrawWireSphere(centerOfMass, 0.1f);

            // 座席位置の表示
            if (m_SeatPosition != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(m_SeatPosition.position, new Vector3(0.5f, 0.1f, 0.5f));
            }

            // 移動方向の表示
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, m_CurrentVelocity);
            }
        }
    }
}