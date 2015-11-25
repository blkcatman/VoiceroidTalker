using System;
using System.Collections.Generic;
using System.Windows.Automation;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Windows.Interop;

using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace VoiceroidTServer
{
    class Program
    {
        //ハンドル操作用の関数
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);
        /*
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        protected static extern int SendMessageStr(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);
        */
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        protected static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        protected static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        /*
        [DllImport("user32.dll")]
        static extern bool TranslateMessage([In] ref MSG lpMsg);
        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref MSG lpmsg);
        */
        //ハンドル操作用定数
        private const int WM_SETTEXT = 0x0c;
        private const int WM_KEYDOWN = 0x0100;     
        //private const int WM_GETTEXTLENGTH = 0x0e;
        
        private const int WM_CLICK = 0xf5;
        private const int WM_NULL = 0x00;

        private const int VK_RETURN = 0x0D;
        
        //private const int ALLSELECT = 60;
        //private const int CUT = 52;

        static void Main(string[] args)
        {
            string[] simpleNames = { "Yukari", "Maki", "Zunko", "Akane", "Aoi", "Koh" };
            string[] voiceroidNames = {
                                          "VOICEROID＋ 結月ゆかり EX",
                                          "VOICEROID＋ 民安ともえ EX",
                                          "VOICEROID＋ 東北ずん子 EX",
                                          "VOICEROID＋ 琴葉茜",
                                          "VOICEROID＋ 琴葉葵",
                                          "VOICEROID＋ 水奈瀬コウ EX"
                                      };
            Console.WriteLine("");

            //引数のチェック
            if (args.Length < 1) {
                string mergeName = "";
                for (int i = 0; i < simpleNames.Length; i++) {
                    mergeName = mergeName + simpleNames[i];
                    if (i < simpleNames.Length - 1)
                        mergeName = mergeName + ", ";
                }
                Console.WriteLine("使用するVOICEROIDを引数に追加してください: " + mergeName);
                return;
            }
            //引数に設定されたボイスロイドの名前をチェック
            string selectedSimple = null;
            int selectedIndex = 0;
            for (int i = 0; i < simpleNames.Length; i++) {
                if (args[0].CompareTo(simpleNames[i]) == 0) {
                    selectedSimple = simpleNames[i];
                    selectedIndex = i;
                    break;
                }
            }
            if (selectedSimple == null) {
                string mergeName = "";
                for (int i = 0; i < simpleNames.Length; i++) {
                    mergeName = mergeName + simpleNames[i];
                    if (i < simpleNames.Length - 1)
                        mergeName = mergeName + ", ";
                }
                Console.WriteLine("引数に指定されたVOICEROIDの名前が正しくありません. 使用できる名前は次のとおりです: " + mergeName);
                return;
            }
            //VOICEROID.exeの起動チェック
            Process[] apps = Process.GetProcessesByName("VOICEROID");
            if (apps.Length < 1)
            {
                Console.WriteLine("プロセスに" + voiceroidNames[selectedIndex] + "のVOICEROID.exeがありません. " + voiceroidNames[selectedIndex] + "を起動してください.");
                return;
            }
            //VOICEROID.exeのプロセス取得
            AutomationElement ae = null;
            foreach (Process app in apps)
            {
                AutomationElement rootHandle = AutomationElement.FromHandle(app.MainWindowHandle);
                foreach(string voiceroidName in voiceroidNames) {
                    string name = rootHandle.Current.Name;
                    if (name.CompareTo(voiceroidNames[selectedIndex]) == 0 || name.CompareTo(voiceroidNames[selectedIndex]+"*") == 0) ae = rootHandle;
                }
            }
            //起動しているVOICEROID.exeと指定されたキャラクターのVOICEROID.exeが一致しなかった時
            if(ae == null) {
                string mergeName = "";
                for (int i = 0; i < simpleNames.Length; i++) {
                    mergeName = mergeName + simpleNames[i];
                    if (i < simpleNames.Length - 1)
                        mergeName = mergeName + ", ";
                }
                Console.WriteLine("引数に指定された名前のVOICEROIDが起動していません. 他の名前を指定するか, 指定した名前のVOICEROID.exeを起動してください: " + mergeName);
                return;
            }
            Console.Clear();
            //ウィンドウにフォーカスをあわせる
            ae.SetFocus();

            //テキストボックス、再生ボタン、停止ボタンのGUIコントロール取得
            AutomationElement txtForm = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtMain", PropertyConditionFlags.IgnoreCase));
            IntPtr txtFormHandle = IntPtr.Zero;
            try {
                txtFormHandle = (IntPtr)txtForm.Current.NativeWindowHandle;
            }
            catch (NullReferenceException e) {
                //ハンドルが取得できなかった場合、ウィンドウが見つかっていない
                Console.WriteLine(voiceroidNames[selectedIndex] + "のウィンドウが取得できませんでした. 最小化されているか, ほかのプロセスによってブロックされています.");
                return;
            }
            //テキストボックスのハンドルを取得
            IntPtr txtControl = FindWindowEx(txtFormHandle, IntPtr.Zero, null, null);
            //再生ボタンのハンドルを取得
            AutomationElement btnPlay = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnPlay", PropertyConditionFlags.IgnoreCase));
            IntPtr btnControl = (IntPtr)btnPlay.Current.NativeWindowHandle;
            //停止ボタンのハンドルを取得
            AutomationElement btnStop = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnStop", PropertyConditionFlags.IgnoreCase));
            IntPtr stpControl = (IntPtr)btnStop.Current.NativeWindowHandle;

            //音量、速度、高さ、抑揚のテキストボックスのコントロールを取得
            AutomationElement txtVol = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtVolume", PropertyConditionFlags.IgnoreCase));
            AutomationElement txtSpd = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtSpeed", PropertyConditionFlags.IgnoreCase));
            AutomationElement txtPit = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtPitch", PropertyConditionFlags.IgnoreCase));
            AutomationElement txtEmp = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtEmphasis", PropertyConditionFlags.IgnoreCase));

            //音声効果の画面が表示されていない時の処理
            if (txtVol == null || txtSpd == null || txtPit == null || txtEmp == null) {
                Console.WriteLine("音声効果の画面を展開しています...");
                Thread.Sleep(100);

                //音声チューニングのタブを取得
                AutomationElementCollection tabs = ae.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "タブ項目", PropertyConditionFlags.IgnoreCase));

                //音声チューニングのタブが取得できない時の処理
                if (tabs.Count < 1) {
                    AutomationElementCollection menues = ae.FindAll(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "メニュー項目", PropertyConditionFlags.IgnoreCase));

                    //音声チューニングのウィンドウを表示する
                    foreach (AutomationElement menu in menues) {
                        if (menu.Current.Name.CompareTo("音声チューニング(T)") == 0) {
                            object ipShowTuning;
                            if (menu.TryGetCurrentPattern(InvokePattern.Pattern, out ipShowTuning)) {
                                ((InvokePattern)ipShowTuning).Invoke();
                                Thread.Sleep(1000);
                            }
                        }
                    }

                    //再度音声チューニング
                    tabs = ae.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "タブ項目", PropertyConditionFlags.IgnoreCase));
                }

                //音声効果のタブを探す
                foreach (AutomationElement tab in tabs) {
                    if (tab.Current.Name.CompareTo("音声効果") == 0) {
                        object ipShowSoundEffect;
                        if (tab.TryGetCurrentPattern(SelectionItemPattern.Pattern, out ipShowSoundEffect)) {
                            ((SelectionItemPattern)ipShowSoundEffect).Select();
                            Thread.Sleep(1000);
                        }
                    }
                }

                //再度、音量、速度、高さ、抑揚のテキストボックスのコントロールを取得
                txtVol = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtVolume", PropertyConditionFlags.IgnoreCase));
                txtSpd = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtSpeed", PropertyConditionFlags.IgnoreCase));
                txtPit = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtPitch", PropertyConditionFlags.IgnoreCase));
                txtEmp = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtEmphasis", PropertyConditionFlags.IgnoreCase));
            }

            //再度、音量、速度、高さ、抑揚のハンドルを取得
            IntPtr txtVolControl = (IntPtr)txtVol.Current.NativeWindowHandle;
            IntPtr txtSpdControl = (IntPtr)txtSpd.Current.NativeWindowHandle;
            IntPtr txtPitControl = (IntPtr)txtPit.Current.NativeWindowHandle;
            IntPtr txtEmpControl = (IntPtr)txtEmp.Current.NativeWindowHandle;

            //InvokePattern ipBtnPlay = (InvokePattern)btnPlay.GetCurrentPattern(InvokePattern.Pattern);
            //ValuePattern vpTxtVol = (ValuePattern)txtVol.GetCurrentPattern(ValuePattern.Pattern);
            //ValuePattern vpTxtSpd = (ValuePattern)txtSpd.GetCurrentPattern(ValuePattern.Pattern);
            //ValuePattern vpTxtPit = (ValuePattern)txtPit.GetCurrentPattern(ValuePattern.Pattern);
            //ValuePattern vpTxtEmp = (ValuePattern)txtEmp.GetCurrentPattern(ValuePattern.Pattern);

            string btnName = btnPlay.Current.Name;
            string message = "";
            Console.WriteLine("[voiceroid_talker" +selectedSimple + "]のサーバーを開始しています...");

            //メインループ
            while (true) {
                string clientMessage;
                string[] messages;
                string messageVol = "1.0";
                string messageSpd = "1.0";
                string messagePit = "1.0";
                string messageEmp = "1.0";
                //通信セッションの開始
                try
                {
                    using (NamedPipeServerStream server = new NamedPipeServerStream("voiceroid_talker" + selectedSimple))
                    {
                        Console.WriteLine("クライアントからのメッセージを待っています...");
                        server.WaitForConnection();

                        byte[] buffer = new byte[1024];
                        server.Read(buffer, 0, buffer.Length);
                        string messageRaw = UnicodeEncoding.Unicode.GetString(buffer);
                        clientMessage = messageRaw.Trim('\0');
                        //通信メッセージの内容をパースする
                        messages = clientMessage.Split(';');
                        if (messages.Length == 1)
                        {
                            message = clientMessage;
                            if (message.CompareTo("exit") == 0) break;
                        }
                        else
                        {
                            message = messages[0];
                        }
                        //音量のパラメータを取得する
                        if (messages.Length >= 2)
                        {
                            float val = 0.0f;
                            if (float.TryParse(messages[1], out val)) messageVol = val.ToString();
                        }
                        //速度のパラメータを取得する
                        if (messages.Length >= 3)
                        {
                            float val = 0.0f;
                            if (float.TryParse(messages[2], out val)) messageSpd = val.ToString();
                        }
                        //高さのパラメータを取得する
                        if (messages.Length >= 4)
                        {
                            float val = 0.0f;
                            if (float.TryParse(messages[3], out val)) messagePit = val.ToString();
                        }
                        //抑揚のパラメータを取得する
                        if (messages.Length >= 5)
                        {
                            float val = 0.0f;
                            if (float.TryParse(messages[4], out val)) messageEmp = val.ToString();
                        }

                        //音量、速度、高さ、抑揚のテキストボックスに値を入力
                        SendMessage(txtVolControl, WM_SETTEXT, IntPtr.Zero, messageVol);
                        PostMessage(txtVolControl, WM_KEYDOWN, (IntPtr)VK_RETURN, null);

                        SendMessage(txtSpdControl, WM_SETTEXT, IntPtr.Zero, messageSpd);
                        PostMessage(txtSpdControl, WM_KEYDOWN, (IntPtr)VK_RETURN, null);

                        SendMessage(txtPitControl, WM_SETTEXT, IntPtr.Zero, messagePit);
                        PostMessage(txtPitControl, WM_KEYDOWN, (IntPtr)VK_RETURN, null);

                        SendMessage(txtEmpControl, WM_SETTEXT, IntPtr.Zero, messageEmp);
                        PostMessage(txtEmpControl, WM_KEYDOWN, (IntPtr)VK_RETURN, null);

                        Thread.Sleep(10);

                        //テキストボックスにメッセージを入れ、再生する
                        SendMessage(txtControl, WM_SETTEXT, IntPtr.Zero, message);
                        PostMessage(stpControl, WM_CLICK, IntPtr.Zero, null);
                        PostMessage(btnControl, WM_CLICK, IntPtr.Zero, null);

                        //ipBtnPlay.Invoke();
                        Thread.Sleep(100);

                        //レスポンス用メッセージをクライアントに送信
                        byte[] response = UnicodeEncoding.Unicode.GetBytes("OK");
                        server.Write(response, 0, 2);

                        //セッションを終了する
                        server.Close();
                    }
                } catch(IOException e) {
                    //セッション作成時にエラーが発生した場合
                    Console.WriteLine("通信セッションの作成に失敗しました. 他のサーバーによって, 同名のセッションが開始されている可能性があります.");
                    break;
                }
            }

        } //メインループ終了


    }
}
