# VoiceroidTalker

## VoiceroidTalkerについて

VoiceroidTalkerはコマンドプロンプトの実行形式ファイルを介して、VOICEROIDへメッセージを送信し、音声再生を操作することができます。  
同時に、VOICEROID＋ EXシリーズ以降の音量、速度、高さ、抑揚のパラメータ調節を行うことができます。


##動作環境・必要なパッケージ等

* Windows 7  
※Windows8以降はUIコントロールの仕組みが変わっているので、まず動かないと思ってください。

* .NET Framework 4.5  
※ない場合は http://www.microsoft.com/ja-jp/download/details.aspx?id=30653 からダウンロードしてください。

* 以下のいずれかのVOICEROIDのインストールが必要です。

	* VOICEROID＋ 結月ゆかり EX
	* VOICEROID＋ 民安ともえ EX
	* VOICEROID＋ 東北ずん子 EX (テスト報告待ち)
	* VOICEROID＋ 琴葉茜 (テスト報告待ち)
	* VOICEROID＋ 琴葉葵 (テスト報告待ち)
	* VOICEROID＋ 水奈瀬コウ EX (テスト報告待ち)

また、VisualStudio 2013のプロジェクトを使用しているため、VisualStudio 2010などだと動作しない可能性があります。  
もしVisualStudio 2010でのプロジェクトがほしい方がおられましたら下記の連絡まで、お願いします。


##おことわり

このアプリケーション、バッチなどを使用してPCに発生した不具合、故障などは一切保障しません。自己責任での実行をお願いします。


##実行ファイルの使い方
1. コマンドプロンプトからbinフォルダに移動し、  
\>VoiceroidTServer.exe [ボイスロイド名:Yukari, Maki, Zunko, Akane, Aoi, Koh]  
(例: >VoiceroidTServer.exe Yukari)  
と実行します。

2. コマンドプロンプトからbinフォルダに移動し、  
\>VoiceroidTClient.exe [ボイスロイド名:Yukari, Maki, Zunko, Akane, Aoi, Koh] [メッセージ部分]  
(例:> VoiceroidTClient.exe Yukari こんにちは;1.0;1.2;1.0;2.0)  
と実行します。

メッセージ部分は、  
[メッセージ];[音量(0.0-2.0)];[話速(0.5-4.0)];[高さ(0.5-2.0)];[抑揚(0.0-2.0)]  
という風に、セミコロンで分けます。メッセージ以降は省略可能です。


##ライセンスについて

copyright (c) 2015 Tatsuro Matsubara  
Released under the MIT license  
http://opensource.org/licenses/mit-license.php

##その他、仕組みなど

VoiceroidTServer.exeが実質的にVOICEROID.exeのGUIをコントロールしています。

VoiceroidTServer.exeはそれぞれ別の名前のプロセス通信名を持たせられるので、他のVOICEROIDがあっても使用できます。ただ、同じVOICEROIDを同時起動させるのは無理です。

VoiceroidTClient.exeは引数をつかって、VoiceroidTServer.exeにコントロールメッセージを送ってます。直接的に呼び出しているのではなく、プロセス間通信をクッションに使ってます。  
Unityでは.NETが2.0までしか対応していないため、プロセス間通信がかなり怪しく、こうせざるを得ませんでした。

ずん子と琴葉姉妹はEXと同じ構造なのですが、自分が持っていないため、テストができておりません。動作報告をいただけると助かります。

他、質問などがありましたら、Twitterの @blkcatman までお願いします。

