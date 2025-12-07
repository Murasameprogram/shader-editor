using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ShaderEditor
{
    // 继承MarkupExtension，允许在XAML中直接使用
    public class BoolToBrushConverter : MarkupExtension, IValueConverter
    {
        // 必须实现：返回转换器实例
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        // 转换逻辑：true→蓝色，false→灰色
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCurrent = (bool)value;
            // 参数格式："选中色,未选中色"（默认蓝色和浅灰）
            string[] colors = parameter?.ToString()?.Split(',') ?? ["#2196F3", "#E0E0E0"];
            string color = isCurrent ? colors[0] : colors[1];
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        // 反向转换（无需实现）
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
