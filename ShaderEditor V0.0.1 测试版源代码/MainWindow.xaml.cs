using System.Windows;

namespace ShaderEditor  // 确保命名空间与XAML一致
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // 点击按钮后打开中间页
            ChooseWindow chooseWindow = new ChooseWindow();
            chooseWindow.Show();
            this.Close();  // 销毁主窗口
        }
    }
}