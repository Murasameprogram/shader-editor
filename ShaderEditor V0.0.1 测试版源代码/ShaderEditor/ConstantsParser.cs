using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShaderEditor
{
    // 解析常量文件的工具类
    public static class ConstantsParser
    {
        // 解析FXH文件（HLSL头文件，格式如：#define MAX_LIGHTS 8 // 最大灯光数量）
        public static List<ConstantItem> ParseFxh(string content)
        {
            var constants = new List<ConstantItem>();
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // 正则匹配#define指令（支持带注释的格式）
            var regex = new Regex(@"#define\s+(\w+)\s+(\S+)(\s*//\s*(.*))?");

            foreach (var line in lines)
            {
                var match = regex.Match(line.Trim());
                if (match.Success)
                {
                    constants.Add(new ConstantItem
                    {
                        Name = match.Groups[1].Value,       // 常量名
                        Value = match.Groups[2].Value,     // 值
                        DefaultValue = match.Groups[2].Value,  // 默认值（初始值）
                        Type = GuessType(match.Groups[2].Value), // 自动推测类型
                        Description = match.Groups[4].Value   // 注释作为描述
                    });
                }
            }
            return constants;
        }

        // 简单推测常量类型（可根据需求扩展）
        private static string GuessType(string value)
        {
            if (int.TryParse(value, out _)) return "int";
            if (float.TryParse(value, out _) || value.Contains("f")) return "float";
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("false", StringComparison.OrdinalIgnoreCase)) return "bool";
            return "unknown";
        }

        // 解析JSON文件（简化版，实际可使用Newtonsoft.Json库）
        public static List<ConstantItem> ParseJson(string content)
        {
            // 注意：实际项目建议用Newtonsoft.Json解析，这里为了演示用字符串分割（简易版）
            var constants = new List<ConstantItem>();
            // 假设JSON格式：{"MAX_LIGHTS": 8, "AMBIENT_STRENGTH": 0.2}
            var pairs = Regex.Matches(content, @"""(\w+)"":\s*(\S+)");
            foreach (Match match in pairs)
            {
                constants.Add(new ConstantItem
                {
                    Name = match.Groups[1].Value,
                    Value = match.Groups[2].Value.Replace(",", ""), // 移除逗号
                    DefaultValue = match.Groups[2].Value.Replace(",", ""),
                    Type = GuessType(match.Groups[2].Value),
                    Description = "从JSON解析"
                });
            }
            return constants;
        }
        // ConstantsParser.cs
        public static List<ConstantExplanation> ParseExplanationFile(string content)
        {
            var explanations = new List<ConstantExplanation>();
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string currentCategory = string.Empty; // 用于记录当前分类

            // 正则表达式：匹配===分类名称===格式（支持括号内容，如===Specular（高光相关）===）
            var categoryRegex = new Regex(@"^={3}(.*?)(?:\(.*?\))?={3}$");

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // 1. 先判断是否是分类行（===xxx===）
                var categoryMatch = categoryRegex.Match(trimmedLine);
                if (categoryMatch.Success)
                {
                    // 提取分类名称（如“Specular”或“高光相关”，根据实际需求选择）
                    // 此处取括号内的内容（如“高光相关”），若没有则取===中间的内容
                    currentCategory = ExtractCategoryName(categoryMatch.Groups[1].Value);
                    continue; // 跳过分类行，不解析为参数
                }

                // 2. 忽略注释行（//开头）
                if (trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                // 3. 解析参数行（原有逻辑）
                var parts = line.Split('|');
                if (parts.Length != 8)
                {
                    System.Diagnostics.Debug.WriteLine($"解释文件格式错误：{line}（需8个字段，实际{parts.Length}个）");
                    continue;
                }

                explanations.Add(new ConstantExplanation
                {
                    Category = currentCategory, // 关联当前分类
                    DisplayName = parts[0].Trim(),
                    OriginalName = parts[1].Trim(),
                    Type = parts[2].Trim(),
                    DefaultValue = parts[3].Trim(),
                    Description = parts[4].Trim(),
                    ValueRange = parts[5].Trim(),
                    Status = parts[6].Trim(),
                    IsEditableByNovice = bool.TryParse(parts[7].Trim(), out bool editable) && editable
                });
            }

            return explanations;
        }

        // 辅助方法：提取分类名称（优先取括号内内容，如“高光相关”）
        private static string ExtractCategoryName(string input)
        {
            var bracketMatch = Regex.Match(input, @"\((.*?)\)");
            if (bracketMatch.Success)
            {
                return bracketMatch.Groups[1].Value.Trim();
            }
            return input.Trim(); // 若没有括号，直接返回===中间的内容
        }


    }
}
