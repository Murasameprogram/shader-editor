using System.Windows;
using System.Windows.Controls;

namespace ShaderEditor
{
    public partial class FolderCard : UserControl
    {
        // 标题属性（依赖属性，支持XAML绑定）
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FolderCard),
                new PropertyMetadata((s, e) => ((FolderCard)s).TitleText.Text = e.NewValue.ToString()));

        // 副标题属性
        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register("Subtitle", typeof(string), typeof(FolderCard),
                new PropertyMetadata((s, e) => ((FolderCard)s).SubtitleText.Text = e.NewValue.ToString()));

        // 自定义图标属性（允许传入任何UI元素，如PackIcon、Image等）
        public UIElement Icon
        {
            get => (UIElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(UIElement), typeof(FolderCard),
                new PropertyMetadata((s, e) =>
                {
                    var card = (FolderCard)s;
                    card.IconContainer.Content = e.NewValue; // 将图标放入容器
                }));

        public FolderCard()
        {
            InitializeComponent();
        }
    }
}