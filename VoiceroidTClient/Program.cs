using System;
using System.Text;
using System.IO;
using System.IO.Pipes;

namespace VoiceroidTClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] simpleNames = { "Yukari", "Maki", "Zunko", "Akane", "Aoi", "Koh" };
            string[] simpleNamesA = { "yukari", "maki", "zunko", "akane", "aoi", "koh" };

            //引数のチェック
            if (args.Length < 2) {
                string mergeName = "";
                for (int i = 0; i < simpleNames.Length; i++)
                {
                    mergeName = mergeName + simpleNames[i];
                    //ワード間に", "をつける
                    if (i < simpleNames.Length - 1)
                        mergeName = mergeName + ", ";
                }
                Console.WriteLine("引数を指定してください: VoiceroidTClient"+ mergeName +" [会話内容];[音量0.0-2.0];[話速0.5-4.0];[高さ0.5-2.0];[抑揚0.0-2.0]");
                return;
            }
            //引数に設定されたボイスロイドの名前をチェック
            string selectedSimple = null;
            for (int i = 0; i < simpleNames.Length; i++) {
                if (args[0].CompareTo(simpleNames[i]) == 0 || args[0].CompareTo(simpleNamesA[i]) == 0) {
                    selectedSimple = simpleNames[i];
                }
            }
            if (selectedSimple == null) {
                string mergeName = "";
                for (int i = 0; i < simpleNames.Length; i++)
                {
                    mergeName = mergeName + simpleNames[i];
                    //ワード間に", "をつける
                    if (i < simpleNames.Length - 1)
                        mergeName = mergeName + ", ";
                }
                Console.WriteLine("第一引数に指定されたVOICEROIDの名前が正しくありません. 使用できる名前は次のとおりです: "+ mergeName);
                return;
            }
            //サーバーとの通信処理を開始
            string message = args[1];
            try {
                //サーバーのセッションを取得する
                using (NamedPipeClientStream client = new NamedPipeClientStream("voiceroid_talker" + selectedSimple)) {
                        client.Connect(1000);
                    //サーバにーメッセージを送信する
                    byte[] buffer = UnicodeEncoding.Unicode.GetBytes(message);
                    client.Write(buffer, 0, buffer.Length);
                    byte[] response = new byte[4];
                    client.Read(response, 0, response.Length);
                    client.Close();
                }
            } catch (Exception e) {
                //サーバーに接続できない時、通信エラーが発生した場合
                Console.WriteLine("VoiceroidTServerによるサーバー, [voiceroid_talker" + selectedSimple + "]が見つかりません.");
            }

        }
    }
}
