# Ateliers.Voice.Engines.VoicePeakTools

C# による VOICEPEAK CLI を使用した音声生成ラッパー。

---

## 概要

Ateliers.Voice.Engines.VoicePeakTools は、VOICEPEAK の CLI を呼び出し、C#から簡単に使用できる音声生成APIを提供します。

---

## 主な機能

- **VoicePeakVoiceGenerator**: VOICEPEAK音声生成の中核クラス
- **VoicePeakGenerateRequest**: 音声生成リクエスト（テキスト、ナレーター、速度など）
- **VoicePeakOptions**: VOICEPEAK実行ファイルのパス設定

---

## 対応しているナレーター

現在のところ、以下のナレーターに対応しています。

- フリモメン
- 夏色花梨
- ポロンちゃん

### ナレーターの追加について

各ナレーターの感情パラメーター名や識別名が、所有していないと不明なため
追加するのが困難な状況です。

ナレーターを追加したい場合、GitHubリポジトリのIssueにて
以下の情報を提供していただけると助かります。

#### 必要な情報①：--list-narratorの結果

(Windows環境の場合) 以下のコマンドを実行し、出力結果を提供してください。

```PowerShell
PS C:\Program Files\VOICEPEAK> .\voicepeak.exe --list-narrator
```

出力結果として、以下のようなナレーター識別名が得られます。

```
Frimomen
Poronchan
夏色花梨
```

#### 必要な情報②：--list-emotion ナレーター名 の結果


(Windows環境の場合) 以下のコマンドを実行し、出力結果を提供してください。

```PowerShell
PS C:\Program Files\VOICEPEAK> .\voicepeak.exe --list-emotion ナレーター名
```

出力結果として、以下のような感情表現名が得られます。

```
(--list-emotion Frimomen の場合)

happy
angry
sad
ochoushimono

```

---

## 依存関係

- Ateliers.Voice.Engines

---

## 注意事項

VOICEPEAK がインストールされている必要があります。

---

## ライセンス

MIT License
