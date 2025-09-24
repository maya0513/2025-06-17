# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## プロジェクト概要

このプロジェクトは、HTC VIVE Focus VisionとVIVE Trackerを使用したAR実験アプリケーションです。車椅子運動実験における視覚・聴覚フィードバック研究用のUnityアプリケーションを開発しています。

## Unity環境設定

- **Unity Version**: 6000.0.41f1
- **Target Platform**: Android (VIVE Focus Vision)
- **XR Framework**: OpenXR with VIVE OpenXR plugin
- **Required Packages**:
  - XR Interaction Toolkit (3.0.8)
  - OpenXR (1.14.3)
  - VIVE OpenXR (GitHub integration)
  - Universal Render Pipeline (17.0.4)
  - Input System (1.13.1)
  - TextMeshPro

## 開発コマンド

Unity プロジェクトのため、主要な操作は Unity Editor で実行します：

### Unity Editor 操作
- **Play Testing**: Unity Editor の Play Mode で実行（推奨解像度：1920x1080）
- **Build**: File > Build Settings → Platform: Android → Build
- **Package Management**: Window > Package Manager でVIVE OpenXRプラグイン等を管理
- **XR Settings**: Edit > Project Settings > XR Plug-in Management でOpenXR設定

### デバッグ・テスト
- **Visual Feedback Test**: `VisualFeedbackSystem.SetTestValues(left, right)` でバー動作確認
- **Subject Data Debug**: Console でSubjectData.Instanceの状態確認
- **Scene Navigation**: SubjectSelectionManager → ExperimentPhaseManager → VisualFeedbackSystem の順で画面遷移

## プロジェクト構造

### Core Scripts (`Assets/Scripts/`)
- `SubjectData.cs`: 被験者データ管理（Singleton、実験設定保持）
- `SubjectSelectionManager.cs`: 被験者選択・群分け管理
- `ExperimentPhaseManager.cs`: 実験フェーズ・タスク選択管理
- `VisualFeedbackSystem.cs`: 車輪トラッカー連動の視覚フィードバック

### 主要ディレクトリ
- `Assets/Scenes/`: Unity シーンファイル
- `Assets/Prefabs/`: 再利用可能なGameObject
- `Assets/XR/`: XR Interaction Toolkit設定
- `Assets/VIVE/`: VIVE OpenXR関連アセット
- `docs/`: 設定手順とタスク説明（日本語）

## アーキテクチャ設計

### フロー設計
1. **被験者選択**: 新規 or 既存被験者 → ID割り当て → フィードバック群選択
2. **実験フェーズ選択**: 事前テスト/トレーニング/事後テスト/保持テスト
3. **タスク選択**: ジグザグ走/8の字走/バスケットボール8の字走
4. **実験実行**: リアルタイムフィードバック + データ記録

### データフロー・スクリプト間連携
- `SubjectData` (Singleton): 被験者ID、フィードバック群、実験フェーズ・タスクを保持
- `SubjectSelectionManager`: 被験者選択後、`ExperimentPhaseManager`を有効化
- `ExperimentPhaseManager`: フェーズ・タスク選択後、`VisualFeedbackSystem.StartFeedback()`を呼び出し
- `VisualFeedbackSystem`:
  - `SubjectData.feedbackGroup`でフィードバック有効/無効を判定
  - VIVE Trackerデータ（ハリボテ実装中）でUI Sliderを更新
  - `SetTestValues(left, right)` でデバッグ用に手動値設定可能

### ハードウェア連携
- **VIVE Trackers**: 左右手首・左右車輪の4台でモーション取得
- **AR Display**: VIVE Focus Visionのパススルー表示にUI重畳
- **Audio**: 内蔵スピーカーで聴覚フィードバック（未実装）

## 重要な設定ファイル

- `ProjectSettings/`: Unity プロジェクト設定
- `Packages/manifest.json`: パッケージ依存関係
- `docs/Unity設定手順.md`: 詳細な Unity 設定手順（日本語）

## 実験要件

このアプリケーションは学術研究用途であり、以下の要件を満たす必要があります：
- リアルタイムモーション処理（60FPS維持）
- 被験者データの安全な管理
- 実験条件の正確な制御
- データの CSV 出力機能（今後実装予定）

詳細な実装要件は `docs/task1.md` を参照してください。