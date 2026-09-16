# RE World Maker

RE World Makerは、VRChatワールド用のギミックをコンポーネントの追加だけで作りやすくするツールです。

`REW Interact Trigger`、`REW Pickup Use Trigger`などのTriggerと、`REW Prop Toggle`、`REW Audio Action`などのActionを同じGameObjectへ追加するだけで動作します。Udon Behaviour同士を手動で配線する必要はありません。

## 特徴

- Add Componentで「REW」と検索して追加
- TriggerとActionを同じGameObjectへ置くだけの自動接続
- 初心者向けのEasy Settings（簡単設定）
- 日本語、英語、韓国語、中国語のInspector表示
- Prop ToggleとAudioの状態同期
- 途中参加者への同期状態反映
- VRC Object SyncとREW同期処理の自動分離
- 非破壊的な自動生成ランタイム

## 必要環境

- Unity 2022.3.22f1
- VRChat SDK - Worlds 3.10.4以上
- UdonSharp（VRChat Worlds SDKに含まれます）

## VCC / VPMからインストール

1. VCCのSettingsでCommunity Repositoriesを開きます。
2. `https://makaria122.github.io/RE-World-Maker/index.json`を追加します。
3. 対象のWorldプロジェクトでManage Projectを開きます。
4. `RE World Maker`を追加します。
5. Unityを開き、Add Componentで`REW`と検索します。

このURLは、最初のGitHub ReleaseとGitHub Pagesの公開後に利用可能になります。

## 基本的な使い方

1. GameObjectを選択します。
2. Add Componentで「REW」と検索します。
3. REW Triggerを追加します。
4. 同じGameObjectへREW Actionを追加します。
5. 全員へ同期する場合はREW Syncedも追加します。

TriggerとActionの手動接続は必要ありません。

## ドキュメント

詳しい日本語ガイドは、パッケージ内の`Documentation~/RE World Maker - 使い方.txt`に収録しています。

## ライセンス

MIT Licenseです。詳しくは`LICENSE.md`を確認してください。
