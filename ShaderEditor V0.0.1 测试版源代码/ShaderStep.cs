using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShaderEditor
{
    public class ShaderStep : INotifyPropertyChanged
    {

        private string? _englishName;
        public string EnglishName
        {
            get => _englishName;
            set { _englishName = value; OnPropertyChanged(); }
        }

        // 新增：中文名称（如"包含文件"）
        private string? _chineseName;
        public string ChineseName
        {
            get => _chineseName;
            set { _chineseName = value; OnPropertyChanged(); }
        }

        // 步骤索引（0-7，对应8个步骤）
        private int _stepIndex;
        public int StepIndex
        {
            get => _stepIndex;
            set { _stepIndex = value; OnPropertyChanged(); }
        }

        // 步骤名称（对应图2的一级步骤）
        private string _stepName = string.Empty;
        public string StepName
        {
            get => _stepName;
            set { _stepName = value; OnPropertyChanged(); }
        }

        // 是否当前步骤（用于高亮显示）
        private bool _isCurrentStep;
        public bool IsCurrentStep
        {
            get => _isCurrentStep;
            set { _isCurrentStep = value; OnPropertyChanged(); }
        }

        // 新增：是否已完成（用于显示蓝色）
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        // 属性变更通知（必须实现）
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
