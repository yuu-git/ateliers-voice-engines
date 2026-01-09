# VoicePeak コマンドラインオプションメモ

以下は VoicePeak のコマンドラインオプションに関するメモです。

## .\voicepeak.exe -h の結果

Voicepeak Command Line Options

CLI オプションは以下の通りです。

```
Voicepeak
Usage:
  C:\Program Files\VOICEPEAK\voicepeak.exe [OPTION...]

  -s, --say Text               Text to say
  -t, --text File              Text file to say
  -o, --out File               Path of output file
  -n, --narrator Name          Name of voice, check --list-narrator
  -e, --emotion Expr           Emotion expression, for example:
                               happy=50,sad=50. Also check --list-emotion
      --list-narrator          Print voice list
      --list-emotion Narrator  Print emotion list for given voice
  -h, --help                   Print help
      --speed Value            Speed (50 - 200)
      --pitch Value            Pitch (-300 - 300)
```

### --list-narrator の結果

以下は `--list-narrator` の結果例です。

```
PS C:\Program Files\VOICEPEAK> .\voicepeak.exe --list-narrator
PS C:\Program Files\VOICEPEAK> Frimomen
Poronchan
夏色花梨
```

### --list-emotion Narrator の結果

以下は `--list-emotion 夏色花梨` の結果例です。

```
PS C:\Program Files\VOICEPEAK> .\voicepeak.exe --list-emotion 夏色花梨
PS C:\Program Files\VOICEPEAK> hightension
buchigire
nageki
sagesumi
sasayaki
```

## 各ナレーターの感情表現一覧

### Frimomen

- happy
- angry
- sad
- ochoushimono

### Poronchan

- robot
- mellow
- punpun
- genius
- teary

### 夏色花梨

- hightension
- buchigire
- nageki
- sagesumi
- sasayaki

## 悩み事

- ナレーター情報や感情パラメーター名が、VOICEPEAKを所有していないと不明なため
  新しいナレーターの追加が困難。
- ナレーターの識別名が英字だったり日本語だったりするので予測して追加が難しい。
- 加えて、感情パラメーター名も同様に予測が難しい。
- イシューを立てて情報提供を募るしかなさそう。