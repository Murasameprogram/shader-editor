using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShaderEditor
{
    public partial class ConstantsEditor : Window
    {
        private string? _currentFilePath;
        private readonly ObservableCollection<ConstantItem> _constants = new ObservableCollection<ConstantItem>();
        private readonly ICollectionView _constantsView;
        private List<ConstantExplanation> _explanations = new List<ConstantExplanation>();

        public ConstantsEditor()
        {
            InitializeComponent();

            // 检查所有关键控件是否初始化
            if (ConstantsGrid == null)
                throw new InvalidOperationException("ConstantsGrid控件未初始化，请检查XAML命名是否正确");
            if (NoviceHintBar == null)
                throw new InvalidOperationException("NoviceHintBar控件未初始化，请检查XAML命名是否正确");
            if (SearchBox == null)
                throw new InvalidOperationException("SearchBox控件未初始化，请检查XAML命名是否正确");

            // 后续初始化逻辑...
            _constantsView = CollectionViewSource.GetDefaultView(_constants);
            _constantsView.Filter = FilterConstants;
            ConstantsGrid.ItemsSource = _constantsView;

            UpdateClearButtonVisibility();
            UpdateStatusText("就绪");
            EditPermissionText.Text = "编辑模式: 只读";
        }

        private void LoadFileContent(string filePath)
        {
            FilePathTextBox.Text = filePath;
            var content = File.ReadAllText(filePath);
            DocumentContentTextBox.Text = content;

            // 解析文件内容到表格
            _constants.Clear();
            ParseFileContent(content, Path.GetExtension(filePath).ToLower());

            // 新增：自动加载对应的解释文件
            AutoLoadExplanationFile(filePath);

            // 切换显示状态
            InitialHintPanel.Visibility = Visibility.Collapsed;
            ConstantsGrid.Visibility = Visibility.Visible;

            UpdateStatusText($"已加载文件: {Path.GetFileName(filePath)}");
        }

        private void AutoLoadExplanationFile(string constantFilePath)
        {
            try
            {
                // 获取常量文件的文件名（不含扩展名）
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(constantFilePath);
                // 只检查程序运行目录（编辑器所在目录），符合"解释文件与编辑器在同一目录"的需求
                string[] searchDirectories = new[]
                {
            AppDomain.CurrentDomain.BaseDirectory    // 程序运行目录（编辑器目录）
        };

                // 可能的解释文件扩展名（优先检查.txt，也保留.expl作为备选）
                string[] explanationExtensions = { ".txt", ".expl" };

                // 查找匹配的解释文件
                foreach (var dir in searchDirectories)
                {
                    if (string.IsNullOrEmpty(dir)) continue;

                    foreach (var ext in explanationExtensions)
                    {
                        string explanationFilePath = Path.Combine(dir, $"{fileNameWithoutExt}{ext}");
                        if (File.Exists(explanationFilePath))
                        {
                            string content = File.ReadAllText(explanationFilePath);
                            _explanations = ConstantsParser.ParseExplanationFile(content);
                            LinkExplanationsToConstants();
                            UpdateStatusText($"已自动加载解释文件：{Path.GetFileName(explanationFilePath)}（{_explanations.Count}条解释）");
                            return; // 找到后立即返回，优先使用.txt
                        }
                    }
                }

                // 未找到时提示
                UpdateStatusText($"未找到对应的解释文件（{fileNameWithoutExt}.txt 或 {fileNameWithoutExt}.expl）");
            }
            catch (Exception ex)
            {
                UpdateStatusText($"自动加载解释文件失败：{ex.Message}");
            }
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClearButtonVisibility();

            var searchText = SearchBox.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                UpdateStatusText($"搜索: {searchText}");
            }
            else
            {
                UpdateStatusText("就绪");
            }
            _constantsView.Refresh();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                MessageBox.Show("未加载有效的文件，无法保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 根据原文件格式生成内容（使用原文件扩展名判断）
                string extension = Path.GetExtension(_currentFilePath).ToLower();
                string content = extension switch
                {
                    ".json" => GenerateJsonContent(),
                    ".txt" => GenerateTxtContent(),
                    _ => GenerateFxhContent() // 默认.fxh
                };

                File.WriteAllText(_currentFilePath, content);
                UpdateStatusText($"已保存到原文件: {Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要恢复默认参数吗？当前未保存的更改将会丢失。",
                "确认刷新", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
                {
                    LoadFileContent(_currentFilePath);
                    UpdateStatusText("已恢复默认参数");
                }
                else
                {
                    ResetEditor();
                }
                SearchBox.Clear();
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                // 只允许选择名为constants.fxh的文件
                Filter = "常量文件 (constants.fxh)|constants.fxh",
                Title = "选择常量文件（仅支持constants.fxh）"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 额外验证文件名是否为constants.fxh（防止通过修改扩展名绕过过滤）
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    if (fileName != "constants.fxh")
                    {
                        MessageBox.Show("仅支持导入名为constants.fxh的文件", "文件名称错误",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _currentFilePath = openFileDialog.FileName;
                    LoadFileContent(_currentFilePath);

                    var fileInfo = new FileInfo(_currentFilePath);
                    var isReadOnly = fileInfo.IsReadOnly;
                    EditPermissionText.Text = $"编辑模式: {(isReadOnly ? "只读" : "可编辑")}";
                    DocumentContentTextBox.IsReadOnly = isReadOnly;

                    // 设置表格编辑权限
                    SetGridEditPermission(isReadOnly);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            UpdateClearButtonVisibility();
            UpdateStatusText("就绪");
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_constants.Count == 0 && string.IsNullOrEmpty(DocumentContentTextBox.Text))
            {
                MessageBox.Show("没有可导出的内容", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedFormat = (ExportFormatCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "FXH格式";
            var extension = selectedFormat switch
            {
                "JSON格式" => "json",
                "TXT格式" => "txt",
                _ => "fxh"
            };

            var saveFileDialog = new SaveFileDialog
            {
                Filter = $"{selectedFormat} (*.{extension})|*.{extension}|所有文件 (*.*)|*.*",
                DefaultExt = extension,
                Title = "导出常量配置"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string content = selectedFormat switch
                    {
                        "JSON格式" => GenerateJsonContent(),
                        "TXT格式" => GenerateTxtContent(),
                        _ => GenerateFxhContent()
                    };

                    File.WriteAllText(saveFileDialog.FileName, content);
                    UpdateStatusText($"已导出到: {saveFileDialog.FileName}");
                    MessageBox.Show($"成功导出到 {saveFileDialog.FileName}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowDescriptionCheck_Checked(object sender, RoutedEventArgs e)
        {
            // 检查 ConstantsGrid 和 Columns 均不为 null，且索引有效
            if (ConstantsGrid != null && ConstantsGrid.Columns != null && ConstantsGrid.Columns.Count > 3)
                ConstantsGrid.Columns[3].Visibility = Visibility.Visible;
            UpdateStatusText("已启用描述显示");
        }

        private void ShowDescriptionCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ConstantsGrid != null && ConstantsGrid.Columns != null && ConstantsGrid.Columns.Count > 3)
                ConstantsGrid.Columns[3].Visibility = Visibility.Collapsed;
            UpdateStatusText("已禁用描述显示");
        }

        private void WordWrapCheck_Checked(object sender, RoutedEventArgs e)
        {
            DocumentContentTextBox.TextWrapping = TextWrapping.Wrap;
            UpdateStatusText("已启用自动换行");
        }

        private void WordWrapCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            DocumentContentTextBox.TextWrapping = TextWrapping.NoWrap;
            UpdateStatusText("已禁用自动换行");
        }

        private void ResetConstant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ConstantItem constant)
            {
                constant.Value = constant.DefaultValue;
                // 检查 Items 不为 null
                ConstantsGrid?.Items?.Refresh();
                UpdateStatusText($"已重置常量: {constant.Name}");
            }
        }


        private void ParseFileContent(string content, string extension)
        {
            // 根据文件格式解析（示例实现）
            switch (extension)
            {
                case ".fxh":
                    ParseFxhContent(content);
                    break;
                case ".json":
                    ParseJsonContent(content);
                    break;
                default: // .txt
                    ParseTxtContent(content);
                    break;
            }
        }

        private void ParseFxhContent(string content)
        {
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            // 新增：匹配 static/const 格式的正则表达式
            var cStyleRegex = new Regex(
                @"^\s*" +
                @"(static\s+)?" +       // 可选的 static 关键字
                @"(const\s+)?" +        // 可选的 const 关键字
                @"(\w+)\s+" +           // 类型（如 float/int）
                @"(\w+)\s*=\s*" +       // 常量名 + =
                @"([^;]+?)\s*" +        // 值（到分号前结束）
                @"(?:;|//.*)?$",        // 忽略分号或注释
                RegexOptions.IgnoreCase
            );

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                // 先处理原有的 #define 格式
                if (trimmedLine.StartsWith("#define"))
                {
                    var parts = trimmedLine.Split(new[] { "//" }, 2, StringSplitOptions.None);
                    string definePart = parts[0].Trim();
                    string descPart = parts.Length > 1 ? parts[1].Trim() : "无描述";

                    var defineParts = definePart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (defineParts.Length >= 3)
                    {
                        _constants.Add(new ConstantItem
                        {
                            Name = defineParts[1],
                            Value = defineParts[2],
                            DefaultValue = defineParts[2],
                            Type = "unknown", // #define 无显式类型
                            Description = descPart,
                            IsStatic = false,
                            IsConst = false
                        });
                    }
                }
                // 新增：处理 static/const 格式
                else if (cStyleRegex.IsMatch(trimmedLine))
                {
                    var match = cStyleRegex.Match(trimmedLine);
                    // 提取关键字（是否包含 static/const）
                    bool hasStatic = !string.IsNullOrEmpty(match.Groups[1].Value);
                    bool hasConst = !string.IsNullOrEmpty(match.Groups[2].Value);
                    // 提取类型、名称、值
                    string type = match.Groups[3].Value.Trim();
                    string name = match.Groups[4].Value.Trim();
                    string value = match.Groups[5].Value.Trim();
                    // 提取注释
                    string descPart = trimmedLine.Contains("//")
                        ? trimmedLine.Split(new[] { "//" }, 2, StringSplitOptions.None)[1].Trim()
                        : "无描述";

                    // 添加到常量列表
                    _constants.Add(new ConstantItem
                    {
                        Name = name,
                        Value = value,
                        DefaultValue = value,
                        Type = type,
                        Description = descPart,
                        IsStatic = hasStatic,  // 记录是否包含 static
                        IsConst = hasConst     // 记录是否包含 const
                    });
                }
            }
        }

        private void ParseJsonContent(string content)
        {
            try
            {
                var items = JsonSerializer.Deserialize<ObservableCollection<ConstantItem>>(content);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        item.DefaultValue = item.Value; // 保存初始值作为默认值
                        _constants.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JSON解析失败: {ex.Message}", "解析错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ParseTxtContent(string content)
        {
            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains('=') && !line.StartsWith('#'))
                {
                    var parts = line.Split(new[] { '=' }, 2);
                    _constants.Add(new ConstantItem
                    {
                        Name = parts[0].Trim(),
                        Value = parts[1].Split("//")[0].Trim(),
                        DefaultValue = parts[1].Split("//")[0].Trim(),
                        Type = "unknown",
                        Description = parts.Length > 1 && parts[1].Contains("//")
                            ? parts[1].Split(new[] { "//" }, 2, StringSplitOptions.None)[1].Trim()
                            : ""
                    });
                }
            }
        }

        private bool FilterConstants(object item)
        {
            if (item is not ConstantItem constant) return false;
            var searchText = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchText)) return true;

            return constant.Name.ToLower().Contains(searchText) ||
                   constant.Value.ToLower().Contains(searchText) ||
                   constant.Description.ToLower().Contains(searchText) ||
                   constant.Type.ToLower().Contains(searchText);
        }

        private void SetGridEditPermission(bool isReadOnly)
        {
            // if (ConstantsGrid == null || ConstantsGrid.Columns == null)
            //     return;
            // 
            // foreach (var column in ConstantsGrid.Columns)
            // {
            //     if (column is DataGridTextColumn textColumn &&
            //         textColumn.Header != null &&
            //         textColumn.Header.ToString() == "当前值")
            //     {
            //         textColumn.IsReadOnly = false;
            //         break;
            //     }
            // }
        }

        private void ResetEditor()
        {
            DocumentContentTextBox.Clear();
            FilePathTextBox.Clear();
            _currentFilePath = null;
            _constants.Clear();
            InitialHintPanel.Visibility = Visibility.Visible;
            ConstantsGrid.Visibility = Visibility.Collapsed;
            UpdateStatusText("已重置编辑器");
        }

        private string GenerateFxhContent()
        {
            return string.Join(Environment.NewLine,
                _constants.Select(c => $"#define {c.Name} {c.Value} // {c.Description}"));
        }

        private string GenerateJsonContent()
        {
            return JsonSerializer.Serialize(_constants, new JsonSerializerOptions { WriteIndented = true });
        }

        private string GenerateTxtContent()
        {
            return string.Join(Environment.NewLine,
                _constants.Select(c => $"{c.Name} = {c.Value} // {c.Description}"));
        }

        private void UpdateClearButtonVisibility()
        {
            ClearSearchBtn.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void UpdateStatusText(string message)
        {
            // 先检查控件是否已初始化
            if (StatusText != null)
            {
                StatusText.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
            }
            else
            {
                // 可选：输出调试信息，帮助定位问题
                System.Diagnostics.Debug.WriteLine("警告：StatusText控件未初始化，无法更新状态文本");
            }
        }

        private void LoadExplanationButton_Click(object sender, RoutedEventArgs e)
        {
            // 校验1：是否已加载常量文件
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                MessageBox.Show("请先加载常量文件（.fxh/.json/.txt），再加载解释文件", "提示",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "解释文件 (*.txt;*.expl)|*.txt;*.expl|所有文件 (*.*)|*.*",
                Title = "选择解释文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 校验2：解释文件与常量文件是否同名（不含扩展名）
                    string constantFileName = Path.GetFileNameWithoutExtension(_currentFilePath);
                    string explanationFileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                    if (constantFileName != explanationFileName)
                    {
                        MessageBox.Show($"解释文件必须与常量文件同名（当前常量文件：{constantFileName}）",
                                        "文件名不匹配", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 校验通过，加载解释文件
                    string content = File.ReadAllText(openFileDialog.FileName);
                    _explanations = ConstantsParser.ParseExplanationFile(content);
                    // 关联解释信息到常量（通过OriginalName匹配）
                    LinkExplanationsToConstants();
                    UpdateStatusText($"已加载解释文件：{Path.GetFileName(openFileDialog.FileName)}（{_explanations.Count}条解释）");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载解释文件失败：{ex.Message}", "错误",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 关联解释信息到常量（通过原始常量名匹配）
        private void LinkExplanationsToConstants()
        {
            foreach (var constant in _constants)
            {
                // 查找原始名称匹配的解释
                var explanation = _explanations.FirstOrDefault(e => e.OriginalName == constant.Name);
                if (explanation != null)
                {
                    constant.Explanation = explanation;
                    // 用解释文件的类型覆盖常量的类型（更准确）
                    constant.Type = explanation.Type;
                }
            }
            // 刷新表格显示
            ConstantsGrid?.Items.Refresh();
        }
    }

    public class ConstantItem : INotifyPropertyChanged
    {
        // 初始化为空字符串，避免 null
        private string _value = "";

        public string Name { get; set; } = "";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        // 新增：变量状态文本（静态/非静态）
        public string StaticStatus => IsStatic ? "静态" : "非静态";

        // 新增：变量类型文本（常量/变量）
        public string ConstType => IsConst ? "常量" : "变量";
        // 新增：所属类型文本
        public string Category { get; set; } = string.Empty;

        private ConstantExplanation? _explanation;
        public ConstantExplanation? Explanation
        {
            get => _explanation;
            set
            {
                _explanation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Explanation)));
            }
        }
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public bool IsStatic { get; set; }  // 是否包含 static 关键字
        public bool IsConst { get; set; }   // 是否包含 const 关键字

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}