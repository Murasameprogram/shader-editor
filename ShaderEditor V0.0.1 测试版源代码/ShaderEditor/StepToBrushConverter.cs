using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ShaderEditor
{
    public class StepToBrushConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 强制判空，避免状态识别失败
            if (value is not ShaderStep step)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")); // 默认灰色
            }

            // 拆分颜色参数（容错：避免参数为空）
            string[] colorParams = parameter?.ToString()?.Split(',') ?? ["#2196F3", "#E0E0E0"];
            string targetColor = "#E0E0E0";

            // 核心逻辑：已完成 OR 当前步骤 → 蓝色，否则灰色
            if (step.IsCompleted || step.IsCurrentStep)
            {
                targetColor = colorParams.Length > 0 ? colorParams[0] : "#2196F3";
            }
            else
            {
                targetColor = colorParams.Length > 1 ? colorParams[1] : "#E0E0E0";
            }

            // 强制刷新画刷（避免缓存），返回未冻结的 SolidColorBrush 以允许实时更新
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(targetColor));
            // 不调用 brush.Freeze()，保持可变以便 UI 在对象属性变化时能刷新（前提是绑定系统触发转换）
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
