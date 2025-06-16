# VR車いすシステム

Unity XR Interaction Toolkitを使用したVR空間での車いす移動システムです。プレイヤーがVRコントローラーでタイヤを手回しして自由に移動できます。

## 🌟 特徴

- **リアルな手回し操作**: VRコントローラーを使った直感的な車いす操作
- **OpenXR対応**: VIVE、Meta Quest等の主要VRヘッドセットをサポート
- **物理シミュレーション**: Unity Physicsによるリアルな移動・回転
- **高精度検出**: 両手協調動作や複雑な回転パターンの検出
- **VR酔い軽減**: コンフォート機能による快適なVR体験

## 📋 システム要件

- **Unity**: 6000.0.41f1以上
- **XR Interaction Toolkit**: 3.0.8以上
- **OpenXR**: 1.14.3以上
- **対応ヘッドセット**: VIVE、Meta Quest、その他OpenXR対応機器

## 🚀 クイックスタート

### 1. プロジェクトを開く
```bash
Unity Hub で本プロジェクトフォルダを開く
```

### 2. 必要なパッケージの確認
- XR Interaction Toolkit
- OpenXR Plugin
- Input System

### 3. シーンの確認
- `Assets/Scenes/DemoScene.unity` を開く
- VRヘッドセットを接続してプレイモードで動作確認

## 📁 プロジェクト構成

```
Assets/
├── Scripts/                     # VR車いすシステム
│   ├── VRWheelchairController.cs       # メインコントローラー
│   ├── VRWheelController.cs             # タイヤ制御
│   ├── VRHandRotationDetector.cs        # 手回し検出
│   └── VRWheelchairOpenXRIntegration.cs # OpenXR統合
├── AK Studio Art/WhellChair/    # 車いす3Dアセット
├── Samples/XR Interaction Toolkit/ # XRITサンプル
└── Scenes/                      # テストシーン
docs/                            # 日本語ドキュメント
├── VR車いすシステム仕様書.md      # 詳細仕様
├── セットアップガイド.md          # 設定手順
└── README.md                    # このファイル
```

## 🎮 使用方法

### 基本操作
1. **VRヘッドセットを装着**
2. **車いすに近づく**: VR空間内の車いすに移動
3. **タイヤをつかむ**: VRコントローラーのグリップボタンでタイヤをグラブ
4. **手回し動作**: 円形の手回し動作で前進・後退
5. **方向転換**: 左右のタイヤを異なる速度で回して回転

### 高度な操作
- **両手での協調操作**: より滑らかで効率的な移動
- **片手操作**: 片手のみでの基本的な移動
- **微調整**: 小さな手の動きでの精密制御

## 🔧 カスタマイズ

### パラメータ調整
主要なパラメータは Inspector で調整可能：

- **移動速度**: 最大3.0m/s
- **回転感度**: 左右差による回転の強さ
- **検出精度**: 手回し動作の検出感度
- **物理特性**: 重心、減衰、摩擦など

### 機能拡張
- ブレーキシステム
- 速度制限機能
- 障害物検知
- 触覚フィードバック

## 📖 ドキュメント

詳細な情報は`docs/`フォルダを参照：

- **[VR車いすシステム仕様書](docs/VR車いすシステム仕様書.md)**: 技術仕様と設計詳細
- **[セットアップガイド](docs/セットアップガイド.md)**: 詳細な設定手順とトラブルシューティング

## 🛠️ 開発者向け情報

### API 概要

#### VRWheelchairController
```csharp
public class VRWheelchairController : MonoBehaviour
{
    public float leftWheelSpeed { get; }     // 左タイヤ回転速度
    public float rightWheelSpeed { get; }    // 右タイヤ回転速度
    public Vector3 currentVelocity { get; }  // 現在の移動速度
    public void ResetWheelchair();           // 状態リセット
}
```

#### VRWheelController
```csharp
public class VRWheelController : MonoBehaviour
{
    public float GetWheelRotationSpeed();                    // 回転速度取得
    public void Initialize(VRWheelchairController, bool);    // 初期化
    public void UpdateVisualRotation();                     // 視覚更新
}
```

### イベントシステム
```csharp
// 回転検出イベント
OnLeftWheelRotation?.Invoke(rotationData);
OnRightWheelRotation?.Invoke(rotationData);
OnCombinedRotation?.Invoke(rotationData);

// 車いす状態変化イベント
OnWheelchairActiveChanged?.Invoke(isActive);
```

## 🐛 トラブルシューティング

### よくある問題

**Q: 車いすが動かない**
A: 以下を確認：
- XRGrabInteractableが正しく設定されているか
- VRコントローラーが正常に動作しているか
- Rigidbodyの制約設定

**Q: 手回し検出が不安定**
A: 検出パラメータを調整：
- Rotation Sensitivity: 0.5～3.0
- Min Rotation Angle: 5～20度
- Detection Accuracy: 0.5～2.0

**Q: VRヘッドセットが認識されない**
A: OpenXR設定を確認：
- Project Settings > XR Plug-in Management
- 対応するFeature Groupsの有効化
- OpenXRランタイムの正常動作

## 📊 パフォーマンス

### 推奨スペック
- **CPU**: Intel i5-8400 / AMD Ryzen 5 2600以上
- **GPU**: GTX 1060 / RX 580以上
- **RAM**: 8GB以上
- **フレームレート**: 90FPS維持

### 最適化ポイント
- URP設定の軽量化
- テクスチャ解像度の調整
- 不要なPost-processingの無効化

## 🔮 今後の予定

- [ ] ハンドトラッキング対応
- [ ] 触覚フィードバック強化
- [ ] AI誘導システム
- [ ] マルチプレイヤー対応
- [ ] アクセシビリティ機能拡張

## 📄 ライセンス

このプロジェクトは教育・研究目的で作成されています。
- Unity XR Interaction Toolkit: Unity Technologies
- AK Studio Art車いすモデル: AK Studio Art

## 🤝 貢献

バグ報告や機能改善の提案を歓迎します：
1. Issue を作成
2. Fork してコード修正
3. Pull Request を送信

## 📞 サポート

技術的な質問やサポートが必要な場合は、プロジェクトのIssueを作成してください。