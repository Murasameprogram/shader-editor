using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ShaderEditor
{
    public class EditorWindowViewModel : INotifyPropertyChanged
    {
        // 步骤集合（绑定到UI的步骤条）
        public ObservableCollection<ShaderStep> AllShaderSteps { get; } = [];

        // 当前步骤（可空类型，避免null错误）
        private ShaderStep? _currentStep;
        public ShaderStep? CurrentStep
        {
            get => _currentStep;
            set
            {
                if (_currentStep == value) return;
                // 取消上一步的高亮
                if (_currentStep != null)
                    _currentStep.IsCurrentStep = false;
                // 设置当前步骤并高亮
                _currentStep = value;
                if (_currentStep != null)
                    _currentStep.IsCurrentStep = true;
                OnPropertyChanged();
                // 通知按钮状态变化
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
                // 触发命令重新评估 CanExecute
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // 上一步/下一步按钮是否可用
        public bool CanGoPrevious => CurrentStep != null && CurrentStep.StepIndex > 0;
        public bool CanGoNext => CurrentStep != null && CurrentStep.StepIndex < AllShaderSteps.Count - 1;

        // 命令：上一步/下一步/选择步骤
        public ICommand GoPreviousCommand { get; }
        public ICommand GoNextCommand { get; }
        public ICommand SelectStepCommand { get; }

        // 构造函数（初始化数据和命令）
        public EditorWindowViewModel()
        {
            // 初始化命令，包含 CanExecute 判断
            GoPreviousCommand = new RelayCommand<object?>(_ => ExecuteGoPrevious(), _ => CanGoPrevious);
            GoNextCommand = new RelayCommand<object?>(_ => ExecuteGoNext(), _ => CanGoNext);
            SelectStepCommand = new RelayCommand<ShaderStep?>(ExecuteSelectStep);

            // 初始化步骤数据（图2的8个一级步骤）
            InitSteps();
        }

        // 初始化步骤数据（核心：按图2顺序添加步骤）
        private void InitSteps()
        {
            AllShaderSteps.Clear();

            // 按“英文名称|中文名称”的格式拆分，依次添加8个步骤
            var steps = new[]
            {
        "Includes|包含文件",
        "Samplers|纹理采样器",
        "Data Structure Definition|数据结构定义",
        "ConstantBuffer|常量缓冲区",
        "VertexShader|顶点着色器",
        "PixelShader|像素着色器",
        "BlendState|混合状态",
        "Effects|效果组合"
    };

            for (int i = 0; i < steps.Length; i++)
            {
                var parts = steps[i].Split('|'); // 按“|”拆分中英文
                AllShaderSteps.Add(new ShaderStep
                {
                    StepIndex = i,
                    EnglishName = parts[0],       // 英文部分
                    ChineseName = parts[1],       // 中文部分
                    IsCompleted = false,
                    IsCurrentStep = i == 0        // 默认选中第一个步骤
                });
            }

            CurrentStep = AllShaderSteps[0];
        }

        // 执行上一步
        private void ExecuteGoPrevious()
        {
            if (CanGoPrevious && CurrentStep != null)
            {
                // 切换到上一步
                var previousStep = AllShaderSteps[CurrentStep.StepIndex - 1];
                CurrentStep = previousStep;
                // 回退时，取消当前步骤的“已完成”状态（可选逻辑）
                CurrentStep.IsCompleted = false;
            }
        }

        // 执行下一步
        private void ExecuteGoNext()
        {
            if (CanGoNext && CurrentStep != null)
            {
                // 切换前，将当前步骤标记为“已完成”
                CurrentStep.IsCompleted = true;
                // 切换到下一步
                CurrentStep = AllShaderSteps[CurrentStep.StepIndex + 1];
            }
        }

        // 点击步骤直接切换
        private void ExecuteSelectStep(ShaderStep? step)
        {
            if (step != null)
                CurrentStep = step;
        }

        // 属性变更通知
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 通用命令类（解决接口参数匹配问题）
    public class RelayCommand<T>(Action<T?> execute, Func<T?, bool>? canExecute = null) : ICommand
    {
        private readonly Action<T?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<T?, bool> _canExecute = canExecute ?? (_ => true);

        public bool CanExecute(object? parameter) => _canExecute((T?)parameter);
        public void Execute(object? parameter) => _execute((T?)parameter);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
