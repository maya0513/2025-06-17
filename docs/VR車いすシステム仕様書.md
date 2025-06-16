# VR車いすシステム仕様書

## 概要

このドキュメントは、Unity XR Interaction Toolkitを使用したVR車いすシステムの詳細仕様を説明しています。プレイヤーがVR空間内で車いすのタイヤを手回しして自由に移動できるシステムです。

## システム構成

### 主要コンポーネント

#### 1. VRWheelchairController.cs
- **役割**: 車いすシステムの中核制御
- **機能**:
  - 左右タイヤの回転速度管理
  - 車いす全体の物理シミュレーション
  - XROriginとの位置同期
  - 移動・回転計算

#### 2. VRWheelController.cs
- **役割**: 個別タイヤの制御
- **機能**:
  - 手回し操作の検出
  - XRGrabInteractableとの連携
  - タイヤの視覚的回転
  - 回転速度の計算

#### 3. VRHandRotationDetector.cs
- **役割**: 高精度な手回し検出
- **機能**:
  - 両手の協調動作検出
  - ノイズフィルタリング
  - 回転データの統計処理
  - 信頼度評価

#### 4. VRWheelchairOpenXRIntegration.cs
- **役割**: OpenXRとの統合
- **機能**:
  - 標準移動システムとの切り替え
  - プレイエリア境界管理
  - コンフォート機能
  - トラッキング補正

## 技術仕様

### 物理設定

| パラメータ | 推奨値 | 説明 |
|-----------|--------|------|
| 最大速度 | 3.0 m/s | 車いすの最大移動速度 |
| 加速度 | 5.0 m/s² | 前進時の加速度 |
| 減速度 | 2.0 m/s² | 停止時の減速度 |
| 回転感度 | 1.0 | 左右タイヤ差による回転感度 |
| タイヤ半径 | 0.3 m | 物理計算用のタイヤ半径 |

### 手回し検出設定

| パラメータ | 推奨値 | 説明 |
|-----------|--------|------|
| 最小回転角度 | 10° | 検出する最小の回転角度 |
| 検出時間窓 | 0.3秒 | 連続回転検出の時間範囲 |
| 回転感度 | 2.0 | 手回し動作の感度 |
| 信頼度閾値 | 0.5 | 有効とみなす検出信頼度 |

### OpenXR統合設定

| 機能 | 既定値 | 説明 |
|------|--------|------|
| 標準移動無効化 | true | 車いす使用時の通常移動制御 |
| 境界チェック | true | プレイエリア境界の尊重 |
| トラッキング補正 | true | 高度なトラッキング補正 |
| コンフォート機能 | false | VR酔い軽減効果 |

## セットアップ手順

### 1. 必要なアセット

- **車いすモデル**: `Assets/AK Studio Art/WhellChair/Prefabs/WhellChair.prefab`
- **XR Interaction Toolkit**: バージョン 3.0.8以上
- **OpenXR Plugin**: 最新版

### 2. Prefab設定

1. **車いすPrefabの配置**:
   ```
   WhellChair (Root)
   ├── [VRWheelchairController]
   ├── [Rigidbody]
   ├── Left_Wheel
   │   ├── [VRWheelController]
   │   ├── [XRGrabInteractable]
   │   └── [Collider]
   └── Right_Wheel
       ├── [VRWheelController]
       ├── [XRGrabInteractable]
       └── [Collider]
   ```

2. **必要なコンポーネント追加**:
   - VRWheelchairController（メインオブジェクト）
   - VRWheelController（各タイヤ）
   - XRGrabInteractable（各タイヤ）
   - VRHandRotationDetector（オプション）
   - VRWheelchairOpenXRIntegration（OpenXR機能用）

### 3. インスペクター設定

#### VRWheelchairController設定
- **Wheelchair Rigidbody**: 車いすのRigidbody
- **Left Wheel**: 左タイヤのTransform
- **Right Wheel**: 右タイヤのTransform
- **Seat Position**: 座席位置のTransform
- **XR Origin**: XROriginの参照

#### VRWheelController設定
- **Wheel Radius**: 0.3m
- **Rotation Sensitivity**: 2.0
- **Rotation Damping**: 3.0
- **Min Rotation Angle**: 10°

## 使用方法

### 基本操作

1. **車いすに乗る**: VR空間内で車いすに近づき、座席位置に移動
2. **タイヤを掴む**: VRコントローラーでタイヤをグラブ
3. **手回し動作**: 円形の手回し動作で車いすを前進・後退
4. **方向転換**: 左右のタイヤを異なる速度で回すことで回転

### 高度な操作

- **両手での協調**: 両手を使った滑らかな操作
- **片手操作**: 片手のみでの基本的な移動
- **微調整**: 小さな手の動きでの精密制御

## 物理的な動作原理

### 移動計算

1. **タイヤ回転速度検出**:
   ```
   左タイヤ速度 = 手回し角速度 × 感度係数
   右タイヤ速度 = 手回し角速度 × 感度係数
   ```

2. **車いす移動計算**:
   ```
   前進速度 = (左タイヤ速度 + 右タイヤ速度) / 2
   回転速度 = (右タイヤ速度 - 左タイヤ速度) × 回転感度
   ```

3. **物理力の適用**:
   ```
   線形速度 = 前進方向 × 前進速度
   角速度 = Y軸 × 回転速度
   ```

### 手回し検出アルゴリズム

1. **位置追跡**: VRコントローラーの3D位置を連続監視
2. **円形動作分析**: タイヤ中心を基準とした円形軌道の検出
3. **角度変化計算**: 連続する位置から回転角度を算出
4. **ノイズフィルタリング**: 移動平均とバリアンス分析でノイズ除去
5. **信頼度評価**: 検出精度に基づく信頼度スコア算出

## トラブルシューティング

### よくある問題

#### 1. 車いすが動かない
- **確認項目**:
  - XRGrabInteractableが正しく設定されているか
  - VRWheelControllerが初期化されているか
  - Rigidbodyの制約設定を確認

#### 2. 手回し検出が不安定
- **対策**:
  - 検出感度の調整（Rotation Sensitivity）
  - フィルタ強度の調整（Noise Filter Strength）
  - 最小回転角度の見直し

#### 3. OpenXRとの統合問題
- **確認項目**:
  - XROriginの参照が正しく設定されているか
  - OpenXRプラグインが有効になっているか
  - プレイエリアの設定を確認

### パフォーマンス最適化

#### CPU最適化
- 手回し検出の更新頻度調整
- 古いトラッキングデータの適切な削除
- フィルタリング処理の軽量化

#### GPU最適化
- 車いすモデルのLOD設定
- テクスチャ解像度の調整
- シェーダーの最適化

## カスタマイズ

### パラメータ調整

#### 移動感度の調整
```csharp
// VRWheelchairController内
public float customSensitivity = 1.5f;
m_TurnSensitivity = customSensitivity;
```

#### 物理特性の変更
```csharp
// カスタム重心設定
m_CenterOfMassOffset = new Vector3(0, -0.3f, 0.1f);
```

### 機能拡張

#### 追加機能の実装例
- ブレーキシステム
- 速度制限機能
- 障害物検知
- 音響フィードバック

## API リファレンス

### VRWheelchairController

| メソッド/プロパティ | 説明 |
|-------------------|------|
| `leftWheelSpeed` | 左タイヤの回転速度（読み取り専用） |
| `rightWheelSpeed` | 右タイヤの回転速度（読み取り専用） |
| `currentVelocity` | 現在の移動速度ベクトル |
| `ResetWheelchair()` | 車いすの状態をリセット |

### VRWheelController

| メソッド/プロパティ | 説明 |
|-------------------|------|
| `GetWheelRotationSpeed()` | タイヤの回転速度を取得 |
| `Initialize(controller, isLeft)` | コントローラーの初期化 |
| `UpdateVisualRotation()` | 視覚的回転の更新 |

### VRWheelchairOpenXRIntegration

| メソッド/プロパティ | 説明 |
|-------------------|------|
| `isWheelchairActive` | 車いすがアクティブか |
| `SetWheelchairMode(bool)` | 車いすモードの手動切り替え |
| `ResetToPlayAreaCenter()` | プレイエリア中心にリセット |

## 更新履歴

| バージョン | 日付 | 変更内容 |
|-----------|------|----------|
| 1.0.0 | 2025-06-17 | 初回リリース |

## ライセンス

このVR車いすシステムは、Unity XR Interaction Toolkitのサンプルアセットとして提供されています。