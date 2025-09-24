# Unity ARシーンでの実験UI設定手順

## 前提条件
- `Assets/Scenes/ARDemoScene.unity` を開く
- 既存のスクリプトが `Assets/Scripts/` に配置済み
- XR Interaction Toolkit のAR Starter Assetsが導入済み

## 1. 基本UI Canvas の作成

### MainMenuCanvas（被験者選択・実験フェーズ選択用）
1. **GameObject > UI > Canvas** で新しいCanvasを作成
2. Canvas名を `MainMenuCanvas` に変更
3. Canvasコンポーネントの設定：
   - Render Mode: `World Space`
   - Position: (0, 1.5, 3) ※ユーザーから3m前方、1.5m上
   - Rotation: (0, 0, 0)
   - Scale: (0.01, 0.01, 0.01)

4. CanvasScaler の設定：
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: (800, 600)
   - Match: 0.5

### FeedbackCanvas（視覚フィードバック用）
1. 別の Canvas を作成して `FeedbackCanvas` に命名
2. 同様の World Space 設定を適用
3. Position: (0, 0, 3) ※画面中央
4. 初期状態では非アクティブに設定

## 2. 被験者選択UI の構築

MainMenuCanvas の子要素として作成：

```
MainMenuCanvas
├── SubjectSelectionPanel (Panel)
│   ├── NewSubjectButton (Button)
│   │   └── Text: "新規被験者"
│   ├── ExistingSubjectButton (Button)
│   │   └── Text: "既存被験者"
│   ├── SubjectIdPanel (Panel)
│   │   ├── SubjectIdInputField (TMP_InputField)
│   │   │   └── Placeholder: "被験者ID入力"
│   │   └── ConfirmIdButton (Button)
│   │       └── Text: "確定"
│   └── GroupSelectionPanel (Panel)
│       ├── VisualFeedbackButton (Button)
│       │   └── Text: "視覚フィードバック群"
│       ├── AudioFeedbackButton (Button)
│       │   └── Text: "聴覚フィードバック群"
│       └── NoFeedbackButton (Button)
│           └── Text: "フィードバックなし群"
```

### レイアウト設定：
- **SubjectSelectionPanel**: Vertical Layout Group, Content Size Fitter
- **GroupSelectionPanel**: 初期非表示（Active = false）
- **SubjectIdPanel**: 初期非表示（Active = false）

## 3. 実験フェーズ選択UI の構築

```
MainMenuCanvas
├── ExperimentPhasePanel (Panel)
│   ├── PreTestButton (Button) → "事前テスト"
│   ├── TrainingButton (Button) → "トレーニング"
│   ├── PostTestButton (Button) → "事後テスト"
│   ├── RetentionTestButton (Button) → "保持テスト"
│   ├── TaskSelectionPanel (Panel)
│   │   ├── ZigzagRunButton (Button) → "ジグザグ走"
│   │   ├── FigureEightRunButton (Button) → "8の字走"
│   │   └── BasketballFigureEightRunButton (Button) → "バスケットボール8の字走"
│   └── ExperimentStartPanel (Panel)
│       ├── ExperimentInfoText (TextMeshPro)
│       └── StartExperimentButton (Button) → "実験開始"
```

### レイアウト設定：
- **ExperimentPhasePanel**: 初期非表示（Active = false）
- **TaskSelectionPanel**: 初期非表示（Active = false）
- **ExperimentStartPanel**: 初期非表示（Active = false）

## 4. 視覚フィードバックUI の構築

FeedbackCanvas の子要素として作成：

```
FeedbackCanvas
└── VisualFeedbackPanel (Panel)
    ├── LeftWheelBar (Slider)
    │   ├── Background
    │   ├── Fill Area
    │   │   └── Fill (Image) → 色: 緑 (0, 1, 0, 1)
    │   └── LeftWheelLabel (TextMeshPro) → "左車輪"
    └── RightWheelBar (Slider)
        ├── Background
        ├── Fill Area
        │   └── Fill (Image) → 色: 緑 (0, 1, 0, 1)
        └── RightWheelLabel (TextMeshPro) → "右車輪"
```

### Slider設定：
- **Min Value**: 0
- **Max Value**: 1
- **Value**: 0
- **Direction**: Bottom To Top

### 配置設定：
- **LeftWheelBar**: Position (-200, 0, 0), Size (150, 300)
- **RightWheelBar**: Position (200, 0, 0), Size (150, 300)

## 5. Manager GameObject の作成と設定

### ExperimentManager（空のGameObject）
1. 空のGameObject作成 → `ExperimentManager`
2. `SubjectData.cs` スクリプトをアタッチ

### SubjectSelectionManager（空のGameObject）
1. 空のGameObject作成 → `SubjectSelectionManager`
2. `SubjectSelectionManager.cs` スクリプトをアタッチ
3. Inspector で UI 参照を設定：
   - New Subject Button
   - Existing Subject Button
   - Subject Id Panel
   - Subject Id Input Field
   - Confirm Id Button
   - Group Selection Panel
   - Visual/Audio/No Feedback Buttons
   - Experiment Phase Manager

### ExperimentPhaseManager（空のGameObject）
1. 空のGameObject作成 → `ExperimentPhaseManager`
2. `ExperimentPhaseManager.cs` スクリプトをアタッチ
3. Inspector で UI 参照を設定：
   - Phase 選択ボタン群
   - Task Selection Panel とボタン群
   - Experiment Start Panel と Info Text
   - Visual Feedback System

### VisualFeedbackSystem（空のGameObject）
1. 空のGameObject作成 → `VisualFeedbackSystem`
2. `VisualFeedbackSystem.cs` スクリプトをアタッチ
3. Inspector で参照を設定：
   - Left Wheel Bar (Slider)
   - Right Wheel Bar (Slider)
   - Left/Right Wheel Tracker（後で設定）

## 6. AR Camera 設定

1. 既存の `XR Origin` を確認
2. XR Origin の Camera Reference を FeedbackCanvas の Event Camera に設定
3. XR Origin の Camera を MainMenuCanvas の Event Camera にも設定

## 7. 初期表示設定

### アクティブ状態の設定：
- **MainMenuCanvas**: Active
- **SubjectSelectionPanel**: Active
- **ExperimentPhasePanel**: Inactive
- **FeedbackCanvas**: Inactive
- **GroupSelectionPanel**: Inactive
- **SubjectIdPanel**: Inactive
- **TaskSelectionPanel**: Inactive
- **ExperimentStartPanel**: Inactive

## 8. テスト手順

### エディターでのテスト：
1. Play Mode に入る
2. 被験者選択フローをテスト：
   - "新規被験者" → フィードバック群選択
   - "既存被験者" → ID入力
3. 実験フェーズ選択をテスト
4. Visual Feedback System の表示確認

### デバッグ用：
- Consoleで `VisualFeedbackSystem.SetTestValues(0.5f, 0.8f)` を実行してバー動作確認
- SubjectData.Instance の状態をConsoleで確認

## 注意点

- **TextMeshPro**: 初回使用時は Import TMP Essentials
- **World Space Canvas**: UI要素の位置は実世界座標で設定
- **Performance**: UI描画負荷を考慮してSliderのUpdate頻度を調整
- **XR Interaction**: UI要素にはXR Raycastが必要（XR UI Input Module使用）