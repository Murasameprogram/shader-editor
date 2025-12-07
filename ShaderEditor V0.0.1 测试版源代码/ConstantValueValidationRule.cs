using System.Globalization;
using System.Windows.Controls;

namespace ShaderEditor
{
    public class ConstantValueValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // 处理null值情况
            if (value == null)
            {
                return new ValidationResult(false, "值不能为空");
            }

            // 处理空字符串情况
            string? valueStr = value.ToString()?.Trim();
            if (string.IsNullOrEmpty(valueStr))
            {
                return new ValidationResult(false, "值不能为空");
            }

            // 可根据常量类型添加更多验证（如数字、范围等）
            return ValidationResult.ValidResult;
        }


    }
}
