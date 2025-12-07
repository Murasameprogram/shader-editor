// 新建 Converters 文件夹，添加 NoviceEditConverter.cs
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShaderEditor.Converters
{
    // 控制新手模式下的编辑权限：仅当新手模式开启且IsEditableByNovice为true时可编辑
    public class NoviceEditConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. 检查输入值是否有效（避免UnsetValue）
            if (values == null || values.Length < 2)
            {
                return true; // 默认只读
            }

            // 2. 处理第一个值：Explanation.IsEditableByNovice（可能为UnsetValue或null）
            bool isEditableByNovice = false;
            if (values[0] != DependencyProperty.UnsetValue && values[0] is bool editable)
            {
                isEditableByNovice = editable;
            }
            else
            {
                return true; // 未初始化时默认只读
            }

            // 3. 处理第二个值：NoviceModeToggle.IsChecked（修正语法错误）
            // 3. 处理第二个值（NoviceModeToggle.IsChecked）的逻辑判断
            bool isNoviceMode = false;
            if (values[1] != DependencyProperty.UnsetValue)
            {
                // 直接使用as运算符和空值合并运算符
                isNoviceMode = (values[1] as bool?) ?? false;
            }

            // 4. 核心逻辑：新手模式下仅允许标记为可编辑的项修改
            return isNoviceMode ? !isEditableByNovice : false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 反转布尔值到可见性（用于隐藏原始名称）
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNoviceMode = (bool)value;
            // 新手模式隐藏原始名称，专业模式显示
            return isNoviceMode ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}