using System.ComponentModel;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TagPass.Models
{
    /// <summary>
    /// 콘솔 뷰 데이터 모델
    /// </summary>
    public class ConsoleModel : INotifyPropertyChanged
    {
        private string _consoleText;
        private int _lineCount;
        private bool _autoScrollEnabled;

        public string ConsoleText
        {
            get => _consoleText ?? string.Empty;
            set
            {
                _consoleText = value;
                OnPropertyChanged(nameof(ConsoleText));
                UpdateLineCount();
            }
        }

        public int LineCount
        {
            get => _lineCount;
            private set
            {
                _lineCount = value;
                OnPropertyChanged(nameof(LineCount));
            }
        }

        public bool AutoScrollEnabled
        {
            get => _autoScrollEnabled;
            set
            {
                _autoScrollEnabled = value;
                OnPropertyChanged(nameof(AutoScrollEnabled));
            }
        }

        public ConsoleModel()
        {
            ConsoleText = "========================================\n\n콘솔이 준비되었습니다.\n";
            AutoScrollEnabled = true;
        }

        /// <summary>
        /// 텍스트 추가
        /// </summary>
        public void AppendLine(string text, bool bTimestamp = true)
        {
            if (bTimestamp)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                ConsoleText += $"[{timestamp}] {text}\n";
            }
            else
            {
                ConsoleText += $"{text}\n";
            }
        }

        /// <summary>
        /// 로그 메서드들
        /// </summary>
        public void LogInfo(string message) => AppendLine($"[INFO] {message}");
        public void LogWarning(string message) => AppendLine($"[WARNING] {message}");
        public void LogError(string message) => AppendLine($"[ERROR] {message}");
        public void LogDebug(string message) => AppendLine($"[DEBUG] {message}");

        /// <summary>
        /// 콘솔 초기화
        /// </summary>
        public void Clear()
        {
            ConsoleText = "========================================\n\n콘솔이 초기화되었습니다.\n";
        }

        /// <summary>
        /// 라인 수 업데이트 (줄바꿈 기준)
        /// </summary>
        private void UpdateLineCount()
        {
            LineCount = ConsoleText.Split('\n').Length;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}