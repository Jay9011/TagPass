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

        // <-- 검색 관련 -->
        private bool _searchBarVisible;
        private string _searchText;
        private List<int> _searchResults;
        private int _currentSearchIndex;
        private string _lastSearchTerm;

        public List<int> SearchResults => _searchResults ?? new List<int>();
        public int CurrentSearchIndex => _currentSearchIndex;
        public bool HasSearchResults => _searchResults?.Count > 0;
        public string SearchResultInfo => HasSearchResults ? $"{_currentSearchIndex + 1}/{_searchResults.Count}" : "";

        public string ConsoleText
        {
            get => _consoleText ?? string.Empty;
            set
            {
                _consoleText = value;
                OnPropertyChanged(nameof(ConsoleText));
                UpdateLineCount();

                // 검색 결과 무효화
                if (!string.IsNullOrEmpty(_lastSearchTerm))
                {
                    PerformSearch(_lastSearchTerm);
                }
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

        public bool SearchBarVisible
        {
            get => _searchBarVisible;
            set
            {
                _searchBarVisible = value;
                OnPropertyChanged(nameof(SearchBarVisible));
            }
        }

        public string SearchText
        {
            get => _searchText ?? string.Empty;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));

                // 검색어가 변경되면 자동으로 검색 수행
                if (!string.IsNullOrWhiteSpace(value))
                {
                    PerformSearch(value);
                }
                else
                {
                    ClearSearchResults();
                }
            }
        }

        public ConsoleModel()
        {
            ConsoleText = "========================================\n\n콘솔이 준비되었습니다.\n";
            AutoScrollEnabled = true;
            SearchBarVisible = false;
            SearchText = string.Empty;
            _searchResults = new List<int>();
            _currentSearchIndex = -1;
            _lastSearchTerm = string.Empty;
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
            ClearSearchResults();
        }

        /// <summary>
        /// 라인 수 업데이트 (줄바꿈 기준)
        /// </summary>
        private void UpdateLineCount()
        {
            LineCount = ConsoleText.Split('\n').Length;
        }

        #region 검색 기능

        /// <summary>
        /// 검색 수행
        /// </summary>
        public void PerformSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            // 새로운 검색어인 경우에만 재검색
            if (searchTerm != _lastSearchTerm)
            {
                FindAllOccurrences(searchTerm);
                _lastSearchTerm = searchTerm;
                _currentSearchIndex = -1;
            }

            // 최초 검색 결과로 이동
            MoveToNext();
        }

        /// <summary>
        /// 모든 검색 결과 찾기
        /// </summary>
        private void FindAllOccurrences(string searchTerm)
        {
            _searchResults.Clear();
            string text = ConsoleText;
            int index = 0;

            while (index < text.Length)
            {
                index = text.IndexOf(searchTerm, index, StringComparison.OrdinalIgnoreCase);
                if (index == -1) break;

                _searchResults.Add(index);
                index += searchTerm.Length;
            }

            OnPropertyChanged(nameof(SearchResults));
            OnPropertyChanged(nameof(HasSearchResults));
            OnPropertyChanged(nameof(SearchResultInfo));
        }

        /// <summary>
        /// 다음 검색 결과로 이동
        /// </summary>
        public void MoveToNext()
        {
            if (!HasSearchResults) return;

            _currentSearchIndex = (_currentSearchIndex + 1) % _searchResults.Count;
            OnPropertyChanged(nameof(CurrentSearchIndex));
            OnPropertyChanged(nameof(SearchResultInfo));
            OnSearchPositionChanged();
        }

        /// <summary>
        /// 이전 검색 결과로 이동
        /// </summary>
        public void MoveToPrevious()
        {
            if (!HasSearchResults) return;

            _currentSearchIndex = _currentSearchIndex <= 0 ? _searchResults.Count - 1 : _currentSearchIndex - 1;
            OnPropertyChanged(nameof(CurrentSearchIndex));
            OnPropertyChanged(nameof(SearchResultInfo));
            OnSearchPositionChanged();
        }

        /// <summary>
        /// 검색 결과 초기화
        /// </summary>
        public void ClearSearchResults()
        {
            if (_searchResults != null) _searchResults.Clear();

            _currentSearchIndex = -1;
            _lastSearchTerm = string.Empty;

            OnPropertyChanged(nameof(SearchResults));
            OnPropertyChanged(nameof(CurrentSearchIndex));
            OnPropertyChanged(nameof(HasSearchResults));
            OnPropertyChanged(nameof(SearchResultInfo));
        }

        /// <summary>
        /// 현재 검색 위치 정보 (UI에서 하이라이트에 사용)
        /// </summary>
        public int CurrentSearchPosition => HasSearchResults && _currentSearchIndex >= 0 && _currentSearchIndex < _searchResults.Count
            ? _searchResults[_currentSearchIndex] : -1;

        /// <summary>
        /// 검색 위치 변경 이벤트
        /// </summary>
        public event EventHandler? SearchPositionChanged;

        private void OnSearchPositionChanged()
        {
            OnPropertyChanged(nameof(CurrentSearchPosition));
            SearchPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}