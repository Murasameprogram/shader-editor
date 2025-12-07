using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ShaderEditor  // 必须与XAML的local命名空间一致
{
    // 1. 常量项模型（ShaderConstant）
    public class ShaderConstant : DependencyObject
    {
        // 依赖属性（用于XAML绑定）
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(ShaderConstant));
        public static readonly DependencyProperty OriginalNameProperty =
            DependencyProperty.Register("OriginalName", typeof(string), typeof(ShaderConstant));
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(ShaderConstant));
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(string), typeof(ShaderConstant));
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(ShaderConstant));
        public static readonly DependencyProperty RangeProperty =
            DependencyProperty.Register("Range", typeof(string), typeof(ShaderConstant));
        public static readonly DependencyProperty IsDisabledProperty =
            DependencyProperty.Register("IsDisabled", typeof(bool), typeof(ShaderConstant));
        public static readonly DependencyProperty CanEditInNoviceProperty =
            DependencyProperty.Register("CanEditInNovice", typeof(bool), typeof(ShaderConstant));
        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", typeof(string), typeof(ShaderConstant));

        // 属性包装器（供代码访问）
        public string DisplayName { get => (string)GetValue(DisplayNameProperty); set => SetValue(DisplayNameProperty, value); }
        public string OriginalName { get => (string)GetValue(OriginalNameProperty); set => SetValue(OriginalNameProperty, value); }
        public string Type { get => (string)GetValue(TypeProperty); set => SetValue(TypeProperty, value); }
        public string DefaultValue { get => (string)GetValue(DefaultValueProperty); set => SetValue(DefaultValueProperty, value); }
        public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
        public string Range { get => (string)GetValue(RangeProperty); set => SetValue(RangeProperty, value); }
        public bool IsDisabled { get => (bool)GetValue(IsDisabledProperty); set => SetValue(IsDisabledProperty, value); }
        public bool CanEditInNovice { get => (bool)GetValue(CanEditInNoviceProperty); set => SetValue(CanEditInNoviceProperty, value); }
        public string CurrentValue { get => (string)GetValue(CurrentValueProperty); set => SetValue(CurrentValueProperty, value); }

        // 输入控件（根据类型生成）
        public FrameworkElement InputControl
        {
            get
            {
                switch (Type)
                {
                    case "float3":
                        var panel = new StackPanel { Orientation = Orientation.Horizontal, };
                        panel.Children.Add(CreateTextBox(45)); // 三个小输入框
                        panel.Children.Add(CreateTextBox(45));
                        panel.Children.Add(CreateTextBox(45));
                        return panel;
                    default:
                        return CreateTextBox(150); // 单个输入框
                }
            }
        }

        // 创建输入框
        private TextBox CreateTextBox(int width)
        {
            var tb = new TextBox
            {
                Width = width,
                Height = 25,
                Text = CurrentValue ?? DefaultValue,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            // 绑定只读状态（关联IsReadOnly属性）
            tb.SetBinding(TextBox.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });

            // 失去焦点时更新值
            tb.LostFocus += (s, e) => CurrentValue = tb.Text;
            return tb;
        }

        // 是否只读（根据模式和权限判断）
        public bool IsReadOnly =>
            IsDisabled ||
            (!LeftPanelViewModel.IsExpertModeStatic && !CanEditInNovice);
    }

    // 2. 分组模型（ConstantGroup）
    public class ConstantGroup : DependencyObject
    {
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(ConstantGroup));
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ConstantGroup), new PropertyMetadata(false));

        public string GroupName { get => (string)GetValue(GroupNameProperty); set => SetValue(GroupNameProperty, value); }
        public bool IsExpanded { get => (bool)GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }
        public ObservableCollection<ShaderConstant> Constants { get; } = new();
        public ObservableCollection<ShaderConstant> FilteredConstants { get; } = new();

        // 过滤分组内的常量（搜索功能）
        public void Filter(string searchText)
        {
            FilteredConstants.Clear();
            var filtered = string.IsNullOrEmpty(searchText)
                ? Constants
                : Constants.Where(c =>
                    c.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    c.OriginalName.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            foreach (var c in filtered) FilteredConstants.Add(c);
        }
    }

    // 3. 视图模型（LeftPanelViewModel）
    public class LeftPanelViewModel
    {
        public static bool IsExpertModeStatic { get; private set; } // 供常量项判断模式
        public ObservableCollection<ConstantGroup> AllGroups { get; } = new();
        public ObservableCollection<ConstantGroup> FilteredGroups { get; } = new();
        public bool IsExpertMode { get => IsExpertModeStatic; set => IsExpertModeStatic = value; }
        public string SearchText { get; set; } = "";

        public LeftPanelViewModel()
        {
            LoadSampleData(); // 加载示例数据（实际项目替换为文件解析）
            FilterConstants();
        }

        // 加载示例数据
        private void LoadSampleData()
        {
            var waterGroup = new ConstantGroup { GroupName = "水体相关" };
            waterGroup.Constants.Add(new ShaderConstant
            {
                DisplayName = "水体基准高度",
                OriginalName = "WATER_HEIGHT",
                Type = "float",
                DefaultValue = "9.5f",
                Description = "水体在场景中的基础高度位置，影响与地形的衔接",
                Range = "≥0",
                IsDisabled = false,
                CanEditInNovice = true
            });
            AllGroups.Add(waterGroup);
        }

        // 过滤常量（搜索时调用）
        public void FilterConstants()
        {
            FilteredGroups.Clear();
            foreach (var group in AllGroups)
            {
                group.Filter(SearchText);
                if (group.FilteredConstants.Any()) FilteredGroups.Add(group);
            }
        }

        // 更新编辑状态（模式切换时调用）
        public void UpdateEditStatus()
        {
            foreach (var group in AllGroups)
                foreach (var c in group.Constants)
                    c.SetValue(ShaderConstant.CurrentValueProperty, c.CurrentValue); // 触发UI更新
        }

        // 刷新（恢复默认值）
        public void Refresh()
        {
            foreach (var group in AllGroups)
                foreach (var c in group.Constants)
                    c.CurrentValue = c.DefaultValue;
        }

        // 统计过滤后的项数
        public int GetTotalFilteredCount() => FilteredGroups.Sum(g => g.FilteredConstants.Count);

        // 统计可编辑项数（新手模式）
        public int GetEditableCount() => AllGroups.Sum(g => g.Constants.Count(c => !c.IsDisabled && c.CanEditInNovice));
    }
}