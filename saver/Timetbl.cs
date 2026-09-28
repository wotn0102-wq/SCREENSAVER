// 대훈이 시간표 화면보호기 (.scr)
// - 연결된 모든 모니터에 같은 폴더의 index.html 을 띄움
// - 마우스가 살짝 떨리는 정도(10px 미만)는 무시하고, 크게 움직이거나 키/버튼을 누르면 종료
// build.bat 이 윈도우 기본 컴파일러(csc.exe, C# 5)로 빌드하므로 최신 C# 문법은 쓰지 않음.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

static class Program
{
    const int MOVE_THRESHOLD = 10;   // 이 픽셀 이상 움직여야 종료
    const int GRACE_MS = 2000;       // 시작 직후 이 시간 동안은 입력 무시

    static readonly List<Form> forms = new List<Form>();
    static Point startPos;
    static DateTime startTime;

    [STAThread]
    static void Main(string[] args)
    {
        string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "/c";
        if (mode.Length > 2) mode = mode.Substring(0, 2);

        if (mode == "/p") return;   // 설정창의 작은 미리보기는 사용하지 않음
        if (mode != "/s")
        {
            MessageBox.Show(
                "대훈이 시간표 화면보호기\n\n" +
                "이 화면보호기 파일과 같은 폴더에 있는 index.html 을 모든 모니터에 보여줍니다.\n" +
                "시간표를 바꾸려면 index.html 을 수정하세요.\n\n" +
                "폴더: " + AppDir(),
                "설정", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string html = Path.Combine(AppDir(), "index.html");
        if (!File.Exists(html))
        {
            MessageBox.Show("index.html 파일을 찾을 수 없습니다:\n" + html, "시간표 화면보호기");
            return;
        }

        UseIE11Mode();
        Application.EnableVisualStyles();

        startPos = Cursor.Position;
        startTime = DateTime.Now;
        Application.AddMessageFilter(new InputFilter());

        Uri uri = new Uri(html);
        foreach (Screen screen in Screen.AllScreens)
        {
            Form f = new Form();
            f.FormBorderStyle = FormBorderStyle.None;
            f.StartPosition = FormStartPosition.Manual;
            f.Bounds = screen.Bounds;
            f.TopMost = true;
            f.ShowInTaskbar = false;
            f.BackColor = Color.FromArgb(0x14, 0x20, 0x1e);

            WebBrowser wb = new WebBrowser();
            wb.Dock = DockStyle.Fill;
            wb.ScrollBarsEnabled = false;
            wb.ScriptErrorsSuppressed = true;
            wb.IsWebBrowserContextMenuEnabled = false;
            wb.WebBrowserShortcutsEnabled = false;
            wb.AllowWebBrowserDrop = false;
            f.Controls.Add(wb);
            wb.Navigate(uri);

            f.FormClosed += delegate { Application.Exit(); };
            forms.Add(f);
            f.Show();
        }
        Cursor.Hide();

        // 마우스 위치를 직접 확인 (WebBrowser 가 마우스 메시지를 가져가도 감지되도록)
        Timer t = new Timer();
        t.Interval = 200;
        t.Tick += delegate
        {
            Point p = Cursor.Position;
            if ((DateTime.Now - startTime).TotalMilliseconds < GRACE_MS) { startPos = p; return; }
            if (Math.Abs(p.X - startPos.X) >= MOVE_THRESHOLD || Math.Abs(p.Y - startPos.Y) >= MOVE_THRESHOLD)
                Quit();
        };
        t.Start();

        Application.Run();
    }

    static string AppDir()
    {
        return Path.GetDirectoryName(Application.ExecutablePath);
    }

    // WebBrowser 컨트롤이 IE7 모드가 아니라 IE11 모드로 동작하도록 설정
    static void UseIE11Mode()
    {
        try
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION");
            key.SetValue(Path.GetFileName(Application.ExecutablePath), 11001, RegistryValueKind.DWord);
            key.Close();
        }
        catch { }
    }

    static void Quit()
    {
        if ((DateTime.Now - startTime).TotalMilliseconds < GRACE_MS) return;
        Application.Exit();
    }

    class InputFilter : IMessageFilter
    {
        const int WM_KEYDOWN = 0x100, WM_SYSKEYDOWN = 0x104;
        const int WM_LBUTTONDOWN = 0x201, WM_RBUTTONDOWN = 0x204, WM_MBUTTONDOWN = 0x207;

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                    Quit();
                    break;
            }
            return false;
        }
    }
}
