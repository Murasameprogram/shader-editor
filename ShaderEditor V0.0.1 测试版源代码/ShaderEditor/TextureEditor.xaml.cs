using System.Windows;

namespace ShaderEditor
{
    /// <summary>
    /// TextureEditor.xaml 的交互逻辑
    /// </summary>
    public partial class TextureEditor : Window
    {
        public TextureEditor()
        {
            InitializeComponent();
        }

        public static implicit operator TextureEditor(ConstantsEditor v)
        {
            throw new NotImplementedException();
        }
    }
}
