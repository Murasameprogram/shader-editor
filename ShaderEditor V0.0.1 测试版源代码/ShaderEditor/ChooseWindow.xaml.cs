using System.Windows;

namespace ShaderEditor  // 确保命名空间与其他窗口一致
{
    public partial class ChooseWindow : Window
    {
        // 保存当前打开的编辑器实例（用于确保唯一性）
        private Window? _currentEditor;

        public ChooseWindow()
        {
            InitializeComponent();
        }

        // 打开着色器编辑器（EditorWindow）
        private void OpenEditorWindow_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentEditor(); // 关闭已打开的编辑器（确保唯一性）

            // 实例化编辑器
            _currentEditor = new EditorWindow();
            // 绑定编辑器的Closed事件：编辑器关闭时触发回调
            _currentEditor.Closed += Editor_Closed;
            _currentEditor.Show();

            this.Hide(); // 隐藏中间页（不销毁，保持监听）
        }

        // 打开常量编辑器（ConstantsEditor）
        private void OpenConstantsEditor_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentEditor();

            _currentEditor = new ConstantsEditor();
            _currentEditor.Closed += Editor_Closed;
            _currentEditor.Show();

            this.Hide();
        }

        // 打开纹理库编辑器（TextureEditor）
        private void OpenTextureEditor_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentEditor();

            _currentEditor = new TextureEditor();
            _currentEditor.Closed += Editor_Closed;
            _currentEditor.Show();

            this.Hide();
        }

        // 关闭当前已打开的编辑器（确保唯一性）
        private void CloseCurrentEditor()
        {
            if (_currentEditor != null && _currentEditor.IsLoaded)
            {
                _currentEditor.Closed -= Editor_Closed; // 先取消事件绑定
                _currentEditor.Close(); // 关闭编辑器
                _currentEditor = null;
            }
        }

        // 编辑器关闭时的回调：重新实例化主窗口
        private void Editor_Closed(object? sender, EventArgs e)
        {
            // 编辑器已销毁，重新创建并显示主窗口
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // 中间页完成使命，自身销毁
            this.Close();
        }
    }
}