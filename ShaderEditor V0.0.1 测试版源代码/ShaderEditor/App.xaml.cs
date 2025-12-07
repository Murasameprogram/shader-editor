using System.Windows;

namespace ShaderEditor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            // 监听所有窗口关闭事件
            this.MainWindow = new MainWindow();
            this.MainWindow.Show();
        }

        // 当最后一个窗口关闭时，强制退出应用
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // 强制终止所有残留线程（极端情况）
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

    }
}
