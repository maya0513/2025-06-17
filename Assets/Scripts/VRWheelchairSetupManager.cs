using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// VR車いすシステムの自動セットアップマネージャー
    /// 既存の車いすアセットを自動的にVR対応に変換する
    /// </summary>
    public class VRWheelchairSetupManager : MonoBehaviour
    {
        [Header("セットアップ対象")]
        [SerializeField]
        [Tooltip("変換対象の車いすPrefab")]
        private GameObject m_WheelchairPrefab;

        [SerializeField]
        [Tooltip("XROriginの参照")]
        private XROrigin m_XROrigin;

        [Header("自動セットアップ設定")]
        [SerializeField]
        [Tooltip("起動時に自動セットアップを実行")]
        private bool m_AutoSetupOnStart = true;

        [SerializeField]
        [Tooltip("既存のコンポーネントを上書き")]
        private bool m_OverrideExistingComponents = false;

        [Header("物理設定")]
        [SerializeField]
        [Tooltip("車いすの質量")]
        [Range(10f, 200f)]
        private float m_WheelchairMass = 50f;

        [SerializeField]
        [Tooltip("タイヤの半径")]
        [Range(0.1f, 1f)]
        private float m_WheelRadius = 0.3f;

        [Header("インタラクション設定")]
        [SerializeField]
        [Tooltip("タイヤのインタラクションレイヤー")]
        private InteractionLayerMask m_WheelInteractionLayer = -1;

        [SerializeField]
        [Tooltip("VR移動プロバイダーを無効化")]
        private bool m_DisableStandardMovement = true;

        // セットアップ結果
        private GameObject m_SetupWheelchair;
        private VRWheelchairController m_WheelchairController;

        void Start()
        {
            if (m_AutoSetupOnStart)
            {
                SetupVRWheelchair();
            }
        }

        /// <summary>
        /// VR車いすの自動セットアップを実行
        /// </summary>
        [ContextMenu("Setup VR Wheelchair")]
        public void SetupVRWheelchair()
        {
            if (m_WheelchairPrefab == null)
            {
                Debug.LogError("VRWheelchairSetupManager: 車いすPrefabが設定されていません");
                return;
            }

            Debug.Log("VR車いすのセットアップを開始します...");

            // 1. 車いすインスタンスの作成または取得
            CreateWheelchairInstance();

            // 2. メインコントローラーのセットアップ
            SetupMainController();

            // 3. 物理コンポーネントのセットアップ
            SetupPhysicsComponents();

            // 4. タイヤコントローラーのセットアップ
            SetupWheelControllers();

            // 5. XR統合のセットアップ
            SetupXRIntegration();

            // 6. 座席位置の設定
            SetupSeatPosition();

            // 7. 検証とテスト
            ValidateSetup();

            Debug.Log("VR車いすのセットアップが完了しました！");
        }

        /// <summary>
        /// 車いすインスタンスの作成
        /// </summary>
        private void CreateWheelchairInstance()
        {
            // 既存のインスタンスをチェック
            if (m_SetupWheelchair == null)
            {
                // 新しいインスタンスを作成
                m_SetupWheelchair = Instantiate(m_WheelchairPrefab);
                m_SetupWheelchair.name = "VRWheelchair";
                
                // 適切な位置に配置
                m_SetupWheelchair.transform.position = transform.position;
                m_SetupWheelchair.transform.rotation = transform.rotation;
            }
        }

        /// <summary>
        /// メインコントローラーのセットアップ
        /// </summary>
        private void SetupMainController()
        {
            // VRWheelchairControllerの追加または取得
            m_WheelchairController = m_SetupWheelchair.GetComponent<VRWheelchairController>();
            if (m_WheelchairController == null)
            {
                m_WheelchairController = m_SetupWheelchair.AddComponent<VRWheelchairController>();
            }

            // 左右のタイヤを検索して設定
            Transform leftWheel = FindChildByName(m_SetupWheelchair.transform, "Left_Wheel");
            Transform rightWheel = FindChildByName(m_SetupWheelchair.transform, "Right_Wheel");

            if (leftWheel != null)
                SetPrivateField(m_WheelchairController, "m_LeftWheel", leftWheel);
            
            if (rightWheel != null)
                SetPrivateField(m_WheelchairController, "m_RightWheel", rightWheel);

            // XROriginの設定
            if (m_XROrigin == null)
                m_XROrigin = FindFirstObjectByType<XROrigin>();
            
            if (m_XROrigin != null)
                SetPrivateField(m_WheelchairController, "m_XROrigin", m_XROrigin);
        }

        /// <summary>
        /// 物理コンポーネントのセットアップ
        /// </summary>
        private void SetupPhysicsComponents()
        {
            // Rigidbodyの設定
            Rigidbody rb = m_SetupWheelchair.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = m_SetupWheelchair.AddComponent<Rigidbody>();
            }

            rb.mass = m_WheelchairMass;
            rb.linearDamping = 1f;
            rb.angularDamping = 5f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // 重心の設定
            rb.centerOfMass = new Vector3(0, -0.5f, 0);

            // メインコライダーの確認・設定
            Collider mainCollider = m_SetupWheelchair.GetComponent<Collider>();
            if (mainCollider == null)
            {
                BoxCollider boxCollider = m_SetupWheelchair.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1.2f, 1.0f, 1.5f); // 車いすの標準サイズ
                boxCollider.center = new Vector3(0, 0.5f, 0);
            }
        }

        /// <summary>
        /// タイヤコントローラーのセットアップ
        /// </summary>
        private void SetupWheelControllers()
        {
            // 左タイヤのセットアップ
            Transform leftWheel = FindChildByName(m_SetupWheelchair.transform, "Left_Wheel");
            if (leftWheel != null)
            {
                SetupSingleWheel(leftWheel.gameObject, true);
            }

            // 右タイヤのセットアップ
            Transform rightWheel = FindChildByName(m_SetupWheelchair.transform, "Right_Wheel");
            if (rightWheel != null)
            {
                SetupSingleWheel(rightWheel.gameObject, false);
            }
        }

        /// <summary>
        /// 個別タイヤのセットアップ
        /// </summary>
        private void SetupSingleWheel(GameObject wheelObject, bool isLeftWheel)
        {
            // VRWheelControllerの追加
            VRWheelController wheelController = wheelObject.GetComponent<VRWheelController>();
            if (wheelController == null || m_OverrideExistingComponents)
            {
                if (wheelController != null)
                    DestroyImmediate(wheelController);
                
                wheelController = wheelObject.AddComponent<VRWheelController>();
            }

            // XRGrabInteractableの追加
            XRGrabInteractable grabInteractable = wheelObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null || m_OverrideExistingComponents)
            {
                if (grabInteractable != null)
                    DestroyImmediate(grabInteractable);
                
                grabInteractable = wheelObject.AddComponent<XRGrabInteractable>();
                
                // XRGrabInteractableの設定
                grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                grabInteractable.interactionLayers = m_WheelInteractionLayer;
            }

            // タイヤコライダーの設定
            Collider wheelCollider = wheelObject.GetComponent<Collider>();
            if (wheelCollider == null)
            {
                SphereCollider sphereCollider = wheelObject.AddComponent<SphereCollider>();
                sphereCollider.radius = m_WheelRadius;
                sphereCollider.isTrigger = false;
            }

            // VRWheelControllerの初期化
            if (m_WheelchairController != null)
            {
                wheelController.Initialize(m_WheelchairController, isLeftWheel);
            }
        }

        /// <summary>
        /// XR統合のセットアップ
        /// </summary>
        private void SetupXRIntegration()
        {
            VRWheelchairOpenXRIntegration xrIntegration = m_SetupWheelchair.GetComponent<VRWheelchairOpenXRIntegration>();
            if (xrIntegration == null)
            {
                xrIntegration = m_SetupWheelchair.AddComponent<VRWheelchairOpenXRIntegration>();
            }

            // XR統合の設定
            if (m_XROrigin != null)
                SetPrivateField(xrIntegration, "m_XROrigin", m_XROrigin);
            
            if (m_WheelchairController != null)
                SetPrivateField(xrIntegration, "m_WheelchairController", m_WheelchairController);

            // 標準移動プロバイダーの無効化設定
            if (m_DisableStandardMovement)
            {
                var locomotionProviders = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(FindObjectsSortMode.None);
                foreach (var provider in locomotionProviders)
                {
                    if (provider.name.Contains("Move") || provider.name.Contains("Continuous"))
                    {
                        SetPrivateField(xrIntegration, "m_StandardMovementProvider", provider);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 座席位置の設定
        /// </summary>
        private void SetupSeatPosition()
        {
            // 既存の座席位置を検索
            Transform seatPosition = FindChildByName(m_SetupWheelchair.transform, "SeatPosition");
            
            if (seatPosition == null)
            {
                // 新しい座席位置オブジェクトを作成
                GameObject seatObject = new GameObject("SeatPosition");
                seatObject.transform.SetParent(m_SetupWheelchair.transform);
                seatObject.transform.localPosition = new Vector3(0, 1.0f, 0); // 座席の高さ
                seatObject.transform.localRotation = Quaternion.identity;
                seatPosition = seatObject.transform;
            }

            // VRWheelchairControllerに座席位置を設定
            if (m_WheelchairController != null)
            {
                SetPrivateField(m_WheelchairController, "m_SeatPosition", seatPosition);
            }
        }

        /// <summary>
        /// セットアップの検証
        /// </summary>
        private void ValidateSetup()
        {
            bool isValid = true;
            System.Text.StringBuilder validationReport = new System.Text.StringBuilder();
            validationReport.AppendLine("=== VR車いすセットアップ検証 ===");

            // メインコントローラーの確認
            if (m_WheelchairController == null)
            {
                validationReport.AppendLine("❌ VRWheelchairControllerが見つかりません");
                isValid = false;
            }
            else
            {
                validationReport.AppendLine("✅ VRWheelchairController正常");
            }

            // Rigidbodyの確認
            Rigidbody rb = m_SetupWheelchair.GetComponent<Rigidbody>();
            if (rb == null)
            {
                validationReport.AppendLine("❌ Rigidbodyが見つかりません");
                isValid = false;
            }
            else
            {
                validationReport.AppendLine("✅ Rigidbody正常");
            }

            // タイヤの確認
            Transform leftWheel = FindChildByName(m_SetupWheelchair.transform, "Left_Wheel");
            Transform rightWheel = FindChildByName(m_SetupWheelchair.transform, "Right_Wheel");
            
            if (leftWheel?.GetComponent<VRWheelController>() == null)
            {
                validationReport.AppendLine("❌ 左タイヤのVRWheelControllerが見つかりません");
                isValid = false;
            }
            else
            {
                validationReport.AppendLine("✅ 左タイヤ正常");
            }

            if (rightWheel?.GetComponent<VRWheelController>() == null)
            {
                validationReport.AppendLine("❌ 右タイヤのVRWheelControllerが見つかりません");
                isValid = false;
            }
            else
            {
                validationReport.AppendLine("✅ 右タイヤ正常");
            }

            // XROriginの確認
            if (m_XROrigin == null)
            {
                validationReport.AppendLine("⚠️ XROriginが設定されていません");
            }
            else
            {
                validationReport.AppendLine("✅ XROrigin正常");
            }

            validationReport.AppendLine($"=== 検証結果: {(isValid ? "成功" : "失敗")} ===");
            Debug.Log(validationReport.ToString());

            if (isValid)
            {
                Debug.Log("VR車いすが正常にセットアップされました。プレイモードで動作確認してください。");
            }
            else
            {
                Debug.LogError("セットアップに問題があります。上記のエラーを確認してください。");
            }
        }

        /// <summary>
        /// 名前で子オブジェクトを検索
        /// </summary>
        private Transform FindChildByName(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                    return child;
                
                Transform found = FindChildByName(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// プライベートフィールドの設定（リフレクション使用）
        /// </summary>
        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"フィールド '{fieldName}' が見つかりません: {target.GetType().Name}");
            }
        }

        /// <summary>
        /// セットアップをリセット
        /// </summary>
        [ContextMenu("Reset Setup")]
        public void ResetSetup()
        {
            if (m_SetupWheelchair != null)
            {
                DestroyImmediate(m_SetupWheelchair);
                m_SetupWheelchair = null;
                m_WheelchairController = null;
                Debug.Log("セットアップがリセットされました");
            }
        }
    }
}