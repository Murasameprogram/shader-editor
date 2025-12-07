namespace ShaderEditor
{
    // 解释文件数据模型（对应解释文件的字段）
    public class ConstantExplanation
    {
        // 显示名称（给新手看的友好名称）
        public string DisplayName { get; set; } = string.Empty;

        // 原始常量名（与常量文件中的Name匹配，用于关联）
        public string OriginalName { get; set; } = string.Empty;

        // 类型（如int/float/bool）
        public string Type { get; set; } = string.Empty;

        // 默认值（与常量文件中的DefaultValue对应）
        public string DefaultValue { get; set; } = string.Empty;

        // 描述（比常量文件更详细的说明）
        public string Description { get; set; } = string.Empty;

        // 取值范围（如"0-100"、"true/false"）
        public string ValueRange { get; set; } = string.Empty;

        // 状态（如"必填"、"可选"）
        public string Status { get; set; } = string.Empty;

        // 新手是否可编辑（true/false）
        public bool IsEditableByNovice { get; set; } = false;

        // 新增：所属类型（从解释文件的===xxx===行提取）
        public string Category { get; set; } = string.Empty;
    }
}
