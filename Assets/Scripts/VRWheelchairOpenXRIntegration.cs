using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using System.Collections.Generic;
using UnityEngine.XR;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
  /// <summary>
  /// VR車いすシステムのOpenXR統合コンポーネント
  /// OpenXRの機能と車いすの移動を連携させる
  /// </summary>
  public class VRWheelchairOpenXRIntegration : MonoBehaviour
  {
    [Header("OpenXR設定")]
    [SerializeField]
    [Tooltip("XROriginの参照")]
    private XROrigin m_XROrigin;

    [SerializeField]
    [Tooltip("車いすコントローラーの参照")]
    private VRWheelchairController m_WheelchairController;

    [SerializeField]
    [Tooltip("標準の移動プロバイダー（必要に応じて無効化）")]
    private UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider m_StandardMovementProvider;

    [Header("統合設定")]
    [SerializeField]
    [Tooltip("車いす使用時に標準移動を無効化")]
    private bool m_DisableStandardMovementWhenUsingWheelchair = true;

    [SerializeField]
    [Tooltip("高度なトラッキング補正を使用")]
    private bool m_UseAdvancedTrackingCorrection = true;

    [SerializeField]
    [Tooltip("空間アンカーとの統合")]
    private bool m_UseSpatialAnchors = false;

    [Header("コンフォート設定")]
    [SerializeField]
    [Tooltip("移動時のフェードアウト効果")]
    private bool m_UseFadeOnMovement = false;

    [SerializeField]
    [Tooltip("フェード効果の強度")]
    [Range(0f, 1f)]
    private float m_FadeIntensity = 0.3f;

    [SerializeField]
    [Tooltip("回転時のビネット効果")]
    private bool m_UseVignetteOnTurn = true;

    [SerializeField]
    [Tooltip("ビネット効果の強度")]
    [Range(0f, 1f)]
    private float m_VignetteIntensity = 0.5f;

    [Header("境界設定")]
    [SerializeField]
    [Tooltip("プレイエリアの境界チェック")]
    private bool m_RespectPlayAreaBounds = true;

    [SerializeField]
    [Tooltip("境界接近時の警告")]
    private bool m_ShowBoundaryWarnings = true;

    [SerializeField]
    [Tooltip("境界に近づく距離")]
    [Range(0.1f, 2f)]
    private float m_BoundaryWarningDistance = 0.5f;

    // 内部状態
    private bool m_IsWheelchairActive;
    private Vector3 m_LastWheelchairPosition;
    private Quaternion m_LastWheelchairRotation;
    private Vector3 m_AccumulatedMovement;
    private float m_MovementSmoothing = 0.1f;

    // コンフォート効果用
    private Camera m_MainCamera;
    private Material m_FadeMaterial;
    private Material m_VignetteMaterial;

    // XRDisplaySubsystemのキャッシュ
    private List<XRDisplaySubsystem> m_DisplaySubsystems = new List<XRDisplaySubsystem>();

    // イベント
    public System.Action<bool> OnWheelchairActiveChanged;
    public System.Action<Vector3> OnBoundaryWarning;

    /// <summary>
    /// 車いすがアクティブかどうか
    /// </summary>
    public bool isWheelchairActive => m_IsWheelchairActive;

    void Awake()
    {
      // 自動参照の設定
      if (m_XROrigin == null)
        m_XROrigin = FindFirstObjectByType<XROrigin>();

      if (m_WheelchairController == null)
        m_WheelchairController = FindFirstObjectByType<VRWheelchairController>();

      if (m_StandardMovementProvider == null)
        m_StandardMovementProvider = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>();

      // メインカメラの取得
      if (m_XROrigin != null)
        m_MainCamera = m_XROrigin.Camera;
    }

    void Start()
    {
      // 初期化
      InitializeOpenXRIntegration();

      // コンフォート効果の準備
      if (m_UseFadeOnMovement || m_UseVignetteOnTurn)
        PrepareComfortEffects();
    }

    void Update()
    {
      // 車いすの使用状態を監視
      MonitorWheelchairUsage();

      // OpenXRトラッキングの補正
      if (m_UseAdvancedTrackingCorrection)
        ApplyTrackingCorrections();

      // 境界チェック
      if (m_RespectPlayAreaBounds)
        CheckPlayAreaBounds();

      // コンフォート効果の更新
      UpdateComfortEffects();
    }

    /// <summary>
    /// OpenXR統合の初期化
    /// </summary>
    private void InitializeOpenXRIntegration()
    {
      if (m_WheelchairController != null)
      {
        m_LastWheelchairPosition = m_WheelchairController.transform.position;
        m_LastWheelchairRotation = m_WheelchairController.transform.rotation;
      }

      // 標準移動プロバイダーの初期状態設定
      UpdateStandardMovementProvider(false);
    }

    /// <summary>
    /// 車いすの使用状態を監視
    /// </summary>
    private void MonitorWheelchairUsage()
    {
      bool wasActive = m_IsWheelchairActive;

      // 車いすの移動を検出
      if (m_WheelchairController != null)
      {
        Vector3 positionDelta = m_WheelchairController.transform.position - m_LastWheelchairPosition;
        Quaternion rotationDelta = m_WheelchairController.transform.rotation * Quaternion.Inverse(m_LastWheelchairRotation);

        float movementMagnitude = positionDelta.magnitude;
        float rotationMagnitude = Quaternion.Angle(Quaternion.identity, rotationDelta);

        // 車いすが使用されているかを判定
        m_IsWheelchairActive = movementMagnitude > 0.01f || rotationMagnitude > 1f ||
                             m_WheelchairController.leftWheelSpeed > 0.1f ||
                             m_WheelchairController.rightWheelSpeed > 0.1f;

        // 状態変化の通知
        if (wasActive != m_IsWheelchairActive)
        {
          OnWheelchairActiveChanged?.Invoke(m_IsWheelchairActive);
          UpdateStandardMovementProvider(!m_IsWheelchairActive);
        }

        // 位置・回転の記録
        m_LastWheelchairPosition = m_WheelchairController.transform.position;
        m_LastWheelchairRotation = m_WheelchairController.transform.rotation;
      }
    }

    /// <summary>
    /// 標準移動プロバイダーの制御
    /// </summary>
    private void UpdateStandardMovementProvider(bool enable)
    {
      if (m_StandardMovementProvider != null && m_DisableStandardMovementWhenUsingWheelchair)
      {
        m_StandardMovementProvider.enabled = enable;
      }
    }

    /// <summary>
    /// トラッキング補正の適用
    /// </summary>
    private void ApplyTrackingCorrections()
    {
      if (m_XROrigin == null || m_WheelchairController == null)
        return;

      // 車いすの移動に基づいてXROriginの位置を補正
      Vector3 wheelchairMovement = m_WheelchairController.currentVelocity * Time.deltaTime;
      m_AccumulatedMovement += wheelchairMovement;

      // 滑らかな移動の適用
      if (m_AccumulatedMovement.magnitude > 0.001f)
      {
        Vector3 smoothedMovement = Vector3.Lerp(Vector3.zero, m_AccumulatedMovement, m_MovementSmoothing);

        // XROriginの位置を更新（Y軸は除く）
        Vector3 newPosition = m_XROrigin.transform.position;
        newPosition.x += smoothedMovement.x;
        newPosition.z += smoothedMovement.z;
        m_XROrigin.transform.position = newPosition;

        m_AccumulatedMovement -= smoothedMovement;
      }
    }

    /// <summary>
    /// プレイエリア境界のチェック
    /// </summary>
    private void CheckPlayAreaBounds()
    {
      if (m_XROrigin == null)
        return;

      // XRDisplaySubsystemの取得（最新版）
      SubsystemManager.GetSubsystems(m_DisplaySubsystems);

      XRDisplaySubsystem xrDisplaySubsystem = null;

      // 実行中のDisplaySubsystemを探す
      for (int i = 0; i < m_DisplaySubsystems.Count; i++)
      {
        if (m_DisplaySubsystems[i].running)
        {
          xrDisplaySubsystem = m_DisplaySubsystems[i];
          break;
        }
      }

      if (xrDisplaySubsystem != null)
      {
        // 境界に近づいているかチェック
        Vector3 currentPosition = m_XROrigin.transform.position;

        // 簡易的な境界チェック（実際の実装では、より詳細な境界情報を使用）
        Vector3 playAreaCenter = Vector3.zero; // 実際のプレイエリア中心を取得
        float playAreaRadius = 2f; // 実際のプレイエリア半径を取得

        float distanceFromCenter = Vector3.Distance(currentPosition, playAreaCenter);
        float distanceFromEdge = playAreaRadius - distanceFromCenter;

        if (m_ShowBoundaryWarnings && distanceFromEdge < m_BoundaryWarningDistance)
        {
          OnBoundaryWarning?.Invoke(currentPosition);

          // 境界に近い場合は車いすの移動を制限
          if (distanceFromEdge < 0.1f && m_WheelchairController != null)
          {
            // 境界方向への移動を防ぐ
            Vector3 toCenterDirection = (playAreaCenter - currentPosition).normalized;
            Vector3 wheelchairForward = m_WheelchairController.transform.forward;

            if (Vector3.Dot(wheelchairForward, toCenterDirection) < 0)
            {
              // 境界から離れる方向のみ許可
              m_WheelchairController.ResetWheelchair();
            }
          }
        }
      }
    }

    /// <summary>
    /// コンフォート効果の準備
    /// </summary>
    private void PrepareComfortEffects()
    {
      if (m_MainCamera == null)
        return;

      // フェード用マテリアルの作成
      if (m_UseFadeOnMovement)
      {
        Shader fadeShader = Shader.Find("Hidden/VRWheelchair/Fade");
        if (fadeShader != null)
          m_FadeMaterial = new Material(fadeShader);
      }

      // ビネット用マテリアルの作成
      if (m_UseVignetteOnTurn)
      {
        Shader vignetteShader = Shader.Find("Hidden/VRWheelchair/Vignette");
        if (vignetteShader != null)
          m_VignetteMaterial = new Material(vignetteShader);
      }
    }

    /// <summary>
    /// コンフォート効果の更新
    /// </summary>
    private void UpdateComfortEffects()
    {
      if (m_WheelchairController == null)
        return;

      // 移動速度に基づくフェード効果
      if (m_UseFadeOnMovement && m_FadeMaterial != null)
      {
        float movementSpeed = m_WheelchairController.currentVelocity.magnitude;
        float fadeAmount = Mathf.Clamp01(movementSpeed / 5f) * m_FadeIntensity;
        m_FadeMaterial.SetFloat("_FadeAmount", fadeAmount);
      }

      // 回転速度に基づくビネット効果
      if (m_UseVignetteOnTurn && m_VignetteMaterial != null)
      {
        float turnSpeed = Mathf.Abs(m_WheelchairController.rightWheelSpeed - m_WheelchairController.leftWheelSpeed);
        float vignetteAmount = Mathf.Clamp01(turnSpeed / 3f) * m_VignetteIntensity;
        m_VignetteMaterial.SetFloat("_VignetteAmount", vignetteAmount);
      }
    }

    /// <summary>
    /// 手動で車いすモードを切り替え
    /// </summary>
    public void SetWheelchairMode(bool enabled)
    {
      m_IsWheelchairActive = enabled;
      UpdateStandardMovementProvider(!enabled);
      OnWheelchairActiveChanged?.Invoke(enabled);
    }

    /// <summary>
    /// プレイエリアの中心に車いすをリセット
    /// </summary>
    public void ResetToPlayAreaCenter()
    {
      if (m_XROrigin != null && m_WheelchairController != null)
      {
        // プレイエリアの中心に移動
        Vector3 centerPosition = Vector3.zero; // 実際のプレイエリア中心を取得
        m_WheelchairController.transform.position = centerPosition;
        m_XROrigin.transform.position = centerPosition;

        m_WheelchairController.ResetWheelchair();
      }
    }

    /// <summary>
    /// OpenXRセッションの状態変化に対応
    /// </summary>
    private void OnEnable()
    {
      // OpenXRセッションの開始/終了イベントを監視
      // Application.onBeforeRender += OnBeforeRender;
    }

    private void OnDisable()
    {
      // Application.onBeforeRender -= OnBeforeRender;
    }

    /// <summary>
    /// デバッグ情報の描画
    /// </summary>
    void OnDrawGizmosSelected()
    {
      if (Application.isPlaying && m_WheelchairController != null)
      {
        // 車いすの状態を可視化
        Gizmos.color = m_IsWheelchairActive ? Color.green : Color.gray;
        Gizmos.DrawWireCube(m_WheelchairController.transform.position, Vector3.one * 0.5f);

        // 移動ベクトルを表示
        if (m_IsWheelchairActive)
        {
          Gizmos.color = Color.blue;
          Gizmos.DrawRay(m_WheelchairController.transform.position, m_WheelchairController.currentVelocity);
        }

        // 境界警告エリアの表示
        if (m_RespectPlayAreaBounds && m_ShowBoundaryWarnings)
        {
          Gizmos.color = Color.red;
          Vector3 playAreaCenter = Vector3.zero;
          float playAreaRadius = 2f;
          Gizmos.DrawWireSphere(playAreaCenter, playAreaRadius - m_BoundaryWarningDistance);
        }
      }
    }
  }
}