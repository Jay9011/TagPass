using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using Serilog;
using Serilog.Core;
using System.IO;

namespace TagPass.Models
{
    public enum LogLevel
    {
        Info,       // 정보
        Warning,    // 경고
        Error,      // 오류
        Debug       // 디버그
    }

    /// <summary>
    /// 콘솔 뷰 데이터 모델 (파일 로깅 기능 포함)
    /// </summary>
    public class ConsoleModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ConcurrentQueue<string> _logLines; // 로그 라인 큐 (최대 문자열 수 제한)
        private readonly StringBuilder _displayText;
        private readonly Logger _fileLogger;
        private readonly string _logDirectory;
        private int _maxLines;
        private bool _autoScrollEnabled; // 자동 스크롤 활성화 여부
        private bool _disposed = false;

        /// <summary>
        /// 라인 수
        /// </summary>
        public int LineCount => _logLines.Count;

        /// <summary>
        /// 최대 표시할 라인 수
        /// </summary>
        public int MaxLines
        {
            get => _maxLines;
            set
            {
                _maxLines = Math.Max(1, value);
                TrimLogLines();
                OnPropertyChanged(nameof(MaxLines));
            }
        }

        /// <summary>
        /// 표시된 텍스트 가져오기
        /// </summary>
        public string ConsoleText
        {
            get => _displayText.ToString();
            private set => OnPropertyChanged(nameof(ConsoleText));
        }

        /// <summary>
        /// 자동 스크롤 활성화 여부
        /// </summary>
        public bool AutoScrollEnabled
        {
            get => _autoScrollEnabled;
            set
            {
                _autoScrollEnabled = value;
                OnPropertyChanged(nameof(AutoScrollEnabled));
            }
        }

        public ConsoleModel(int maxLines = 1000, string logDirectory = "Logs")
        {
            _logLines = new ConcurrentQueue<string>();
            _displayText = new StringBuilder();
            _maxLines = maxLines;
            _autoScrollEnabled = true;
            _logDirectory = logDirectory;

            // 로그 디렉토리 생성
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _fileLogger = new LoggerConfiguration()
                .WriteTo.DateFormatPath(
                    $"{_logDirectory}\\{{date:format=yyyy-MM}}\\{{date:format=dd}}\\console-{{date:format=HH}}.log")
                .CreateLogger();

            AddLogLine("콘솔이 준비되었습니다.", false);
            LogToFileOnly(LogLevel.Info, "콘솔 시스템이 시작되었습니다.");
        }

        #region 로그 메서드들

        public void LogInfo(string message)
        {
            AppendLine($"[INFO] {message}");
            LogToFileOnly(LogLevel.Info, message);
        }

        public void LogWarning(string message)
        {
            AppendLine($"[WARNING] {message}");
            LogToFileOnly(LogLevel.Warning, message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            AppendLine($"[ERROR] {message}");
            if (exception != null)
            {
                _fileLogger.Error(exception, message);
            }
            else
            {
                LogToFileOnly(LogLevel.Error, message);
            }
        }

        public void LogDebug(string message)
        {
            AppendLine($"[DEBUG] {message}");
            LogToFileOnly(LogLevel.Debug, message);
        }

        /// <summary>
        /// 파일에만 로그 (콘솔 표시 안함)
        /// </summary>
        public void LogToFileOnly(string level, string message)
        {
            switch (level.ToUpper())
            {
                case "INFO":
                    LogToFileOnly(LogLevel.Info, message);
                    break;
                case "WARN":
                case "WARNING":
                    LogToFileOnly(LogLevel.Warning, message);
                    break;
                case "ERROR":
                    LogToFileOnly(LogLevel.Error, message);
                    break;
                case "DEBUG":
                    LogToFileOnly(LogLevel.Debug, message);
                    break;
                default:
                    LogToFileOnly(LogLevel.Info, message);
                    break;
            }
        }

        /// <summary>
        /// 파일에만 로그 (콘솔 표시 안함)
        /// </summary>
        public void LogToFileOnly(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info:
                    _fileLogger.Information(message);
                    break;
                case LogLevel.Warning:
                    _fileLogger.Warning(message);
                    break;
                case LogLevel.Error:
                    _fileLogger.Error(message);
                    break;
                case LogLevel.Debug:
                    _fileLogger.Debug(message);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 텍스트 추가
        /// </summary>
        public void AppendLine(string text, bool bTimestamp = true)
        {
            string logLine = bTimestamp
                ? $"[{DateTime.Now:HH:mm:ss}] {text}"
                : text;

            AddLogLine(logLine, true);
        }

        /// <summary>
        /// 콘솔 초기화
        /// </summary>
        public void Clear()
        {
            _logLines.Clear();
            _displayText.Clear();
            AddLogLine("콘솔이 초기화되었습니다.", false);
            LogToFileOnly(LogLevel.Info, "콘솔이 초기화되었습니다.");
            OnPropertyChanged(nameof(ConsoleText));
            OnPropertyChanged(nameof(LineCount));
        }

        /// <summary>
        /// 현재 표시된 텍스트 가져오기
        /// </summary>
        public string GetDisplayText() => ConsoleText;

        /// <summary>
        /// 오래된 로그 파일 정리
        /// </summary>
        public void CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                var allLogFiles = Directory.GetFiles(_logDirectory, "console-*.log", SearchOption.AllDirectories);
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in allLogFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        LogToFileOnly(LogLevel.Info, $"오래된 로그 파일 삭제: {file}");
                    }
                }

                // 빈 폴더 정리
                CleanupEmptyDirectories(_logDirectory);
            }
            catch (Exception ex)
            {
                LogToFileOnly(LogLevel.Error, $"로그 파일 정리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 빈 디렉토리 정리
        /// </summary>
        private void CleanupEmptyDirectories(string directory)
        {
            try
            {
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    CleanupEmptyDirectories(subDir);
                    if (!Directory.EnumerateFileSystemEntries(subDir).Any())
                    {
                        Directory.Delete(subDir);
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFileOnly(LogLevel.Warning, $"빈 디렉토리 정리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 로그 라인 추가 및 화면 업데이트
        /// </summary>
        private void AddLogLine(string line, bool notifyChange)
        {
            _logLines.Enqueue(line);

            TrimLogLines(); // 최대 라인 수 초과 시 오래된 라인 제거
            RebuildDisplayText();   // 표시 텍스트 재구성

            if (notifyChange)
            {
                OnPropertyChanged(nameof(ConsoleText));
                OnPropertyChanged(nameof(LineCount));
            }
        }

        /// <summary>
        /// 오래된 로그 라인 제거
        /// </summary>
        private void TrimLogLines()
        {
            while (_logLines.Count > _maxLines && _logLines.TryDequeue(out _))
            {
                // 큐에서 오래된 라인 제거
            }
        }

        /// <summary>
        /// 표시용 텍스트 재구성
        /// </summary>
        private void RebuildDisplayText()
        {
            _displayText.Clear();
            foreach (var line in _logLines)
            {
                _displayText.AppendLine(line);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                LogToFileOnly(LogLevel.Info, "콘솔 시스템이 종료됩니다.");
                _fileLogger?.Dispose();
                _disposed = true;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}