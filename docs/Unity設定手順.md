# Unity側での設定手順

## 1. プロジェクト設定

### XR設定
1. **Edit > Project Settings > XR Plug-in Management**
   - `OpenXR` にチェックを入れる
   - Android用の設定も有効にする（VIVE Focus Vision対応）

2. **XR Plug-in Management > OpenXR**
   - `VIVE Focus Vision` プロファイルを追加
   - `Hand Tracking` を有効化
   - `Tracker` サポートを有効化

### ビルド設定
3. **File > Build Settings**
   - Platform: `Android` に変更
   - `Switch Platform` をクリック

4. **Player Settings > Android Settings**
   - Target API Level: API Level 29 以上
   - Scripting Backend: `IL2CPP`
   - Target Architectures: `ARM64` のみ

## 2. シーン設定

### 基本シーン構成
1. 新しいシーンを作成するか、`SampleScene.unity` を開く

2. **Main Camera** の設定:
   - `Main Camera` を削除
   - `XR Origin` をシーンに追加（XR Interaction Toolkit から）

3. **キャンバス設定**:
   - Canvas Render Mode: `World Space`
   - Event Camera: XR Origin の Camera を設定
   - Canvas位置: ユーザーの前方約3m の位置

## 3. UI構成設定

### 被験者選択UI
1. **Main Menu Canvas** を作成:
   ```
   MainMenuCanvas (Canvas - World Space)
   ├── SubjectSelectionPanel
   │   ├── NewSubjectButton (Button)
   │   ├── ExistingSubjectButton (Button)
   │   ├── SubjectIdPanel (Panel)
   │   │   ├── SubjectIdInputField (TMP_InputField)
   │   │   └── ConfirmIdButton (Button)
   │   └── GroupSelectionPanel (Panel)
   │       ├── VisualFeedbackButton (Button)
   │       ├── AudioFeedbackButton (Button)
   │       └── NoFeedbackButton (Button)
   └── ExperimentPhasePanel
       ├── PreTestButton (Button)
       ├── TrainingButton (Button)
       ├── PostTestButton (Button)
       ├── RetentionTestButton (Button)
       ├── TaskSelectionPanel (Panel)
       │   ├── ZigzagRunButton (Button)
       │   ├── FigureEightRunButton (Button)
       │   └── BasketballFigureEightRunButton (Button)
       └── ExperimentStartPanel (Panel)
           ├── ExperimentInfoText (TextMeshPro)
           └── StartExperimentButton (Button)
   ```

### 視覚フィードバックUI
2. **Feedback Canvas** を作成:
   ```
   FeedbackCanvas (Canvas - World Space)
   └── VisualFeedbackPanel
       ├── LeftWheelBar (Slider)
       │   ├── Background
       │   ├── Fill Area
       │   │   └── Fill (緑色に設定)
       │   └── LeftWheelLabel (TextMeshPro)
       └── RightWheelBar (Slider)
           ├── Background
           ├── Fill Area
           │   └── Fill (緑色に設定)
           └── RightWheelLabel (TextMeshPro)
   ```

## 4. スクリプトアタッチ

### GameObjectの作成と設定
1. **Experiment Manager** 空のGameObjectを作成:
   - `SubjectData.cs` をアタッチ

2. **Subject Selection Manager** 空のGameObjectを作成:
   - `SubjectSelectionManager.cs` をアタッチ
   - UIの参照を設定:
     - New Subject Button
     - Existing Subject Button
     - Subject Id Panel
     - Subject Id Input Field
     - Confirm Id Button
     - Group Selection Panel
     - Visual/Audio/No Feedback Buttons
     - Experiment Phase Manager参照

3. **Experiment Phase Manager** 空のGameObjectを作成:
   - `ExperimentPhaseManager.cs` をアタッチ
   - UIの参照を設定:
     - Phase選択ボタン群
     - Task選択パネルとボタン群
     - 実験開始パネルと情報テキスト
     - Visual Feedback System参照

4. **Visual Feedback System** 空のGameObjectを作成:
   - `VisualFeedbackSystem.cs` をアタッチ
   - UI Sliderの参照を設定:
     - Left Wheel Bar
     - Right Wheel Bar
   - トラッカー参照（後で設定）:
     - Left Wheel Tracker
     - Right Wheel Tracker

## 5. VIVE Tracker設定（後で実装予定）

### トラッカーGameObjectの準備
1. 各トラッカー用の空のGameObjectを作成:
   - LeftWristTracker
   - RightWristTracker
   - LeftWheelTracker
   - RightWheelTracker

2. 各トラッカーGameObjectに追加する予定のコンポーネント:
   - `SteamVR_Behaviour_Pose`（レガシーサポート用）
   - OpenXR Tracker Component（新実装用）

## 6. テスト実行

### エディター内テスト
1. Play Mode に入る
2. UI操作をテスト:
   - 被験者選択フロー
   - フェーズ・タスク選択
   - 視覚フィードバックバーの表示

### ビルドテスト
1. **File > Build Settings > Build**
2. APKをVIVE Focus Visionにインストール
3. AR環境での動作確認

## 7. UI配置の推奨設定

### Canvas設定
- **World Space Canvas** のPositions:
  - Z: 3.0（ユーザーから3m前方）
  - Rotation: (0, 0, 0)
  - Scale: (0.01, 0.01, 0.01)

### フィードバックバーの配置
- **Left Wheel Bar**:
  - Position: (-200, 0, 0) （画面左側）
  - Size: (150, 30)
  - Direction: Bottom to Top

- **Right Wheel Bar**:
  - Position: (200, 0, 0) （画面右側）
  - Size: (150, 30)
  - Direction: Bottom to Top

## 注意事項
- **TextMeshPro**: 初回使用時にパッケージのインポートが必要
- **AR Foundation**: パススルー機能使用時は追加設定が必要
- **Performance**: UI要素はできるだけ軽量に保つ（60FPS維持のため）