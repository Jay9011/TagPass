using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TagPass.Views
{
    public partial class ConsoleView : UserControl
    {
        private List<int> searchResults = new List<int>();
        private int currentSearchIndex = -1;
        private string lastSearchTerm = "";

        public ConsoleView()
        {
            InitializeComponent();
            UpdateLineCount();
        }

        // 콘솔에 텍스트 추가
        public void AppendLine(string text)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                ConsoleTextBox.AppendText($"[{timestamp}] {text}\n");

                UpdateLineCount();

                // 자동 스크롤
                if (AutoScrollCheckBox.IsChecked == true)
                {
                    ConsoleTextBox.ScrollToEnd();
                }
            });
        }

        // 콘솔에 여러 줄 텍스트 추가
        public void AppendText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                ConsoleTextBox.AppendText(text);
                UpdateLineCount();

                if (AutoScrollCheckBox.IsChecked == true)
                {
                    ConsoleTextBox.ScrollToEnd();
                }
            });
        }

        // 라인 수 업데이트
        private void UpdateLineCount()
        {
            var lines = ConsoleTextBox.Text.Split('\n').Length;
            LineCountText.Text = lines.ToString();
        }

        // 콘솔 지우기
        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("콘솔 내용을 모두 지우시겠습니까?", "콘솔 지우기",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ConsoleTextBox.Clear();
                ConsoleTextBox.Text = "TagPass 콘솔 창\n========================================\n\n콘솔이 초기화되었습니다.\n";
                UpdateLineCount();
            }
        }

        // 콘솔 내용 복사
        private void CopyConsole_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ConsoleTextBox.SelectedText))
            {
                Clipboard.SetText(ConsoleTextBox.SelectedText);
                MessageBox.Show("선택된 텍스트가 클립보드에 복사되었습니다.", "복사 완료",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Clipboard.SetText(ConsoleTextBox.Text);
                MessageBox.Show("전체 콘솔 내용이 클립보드에 복사되었습니다.", "복사 완료",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // 검색 바 토글
        private void SearchConsole_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBar.Visibility == Visibility.Collapsed)
            {
                SearchBar.Visibility = Visibility.Visible;
                SearchTextBox.Focus();
            }
            else
            {
                SearchBar.Visibility = Visibility.Collapsed;
                ConsoleTextBox.Focus();
            }
        }

        // 검색 바 닫기
        private void CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBar.Visibility = Visibility.Collapsed;
            ClearSearchHighlight();
            ConsoleTextBox.Focus();
        }

        // 검색 텍스트박스 키 이벤트
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CloseSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        // 검색 수행
        private void PerformSearch()
        {
            string searchTerm = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            // 새로운 검색어인 경우 검색 결과 재계산
            if (searchTerm != lastSearchTerm)
            {
                FindAllOccurrences(searchTerm);
                lastSearchTerm = searchTerm;
                currentSearchIndex = -1;
            }

            // 다음 결과로 이동
            FindNext_Click(this, new RoutedEventArgs());
        }

        // 모든 검색 결과 찾기
        private void FindAllOccurrences(string searchTerm)
        {
            searchResults.Clear();
            string text = ConsoleTextBox.Text;
            int index = 0;

            while (index < text.Length)
            {
                index = text.IndexOf(searchTerm, index, StringComparison.OrdinalIgnoreCase);
                if (index == -1) break;

                searchResults.Add(index);
                index += searchTerm.Length;
            }
        }

        // 다음 검색 결과
        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            if (searchResults.Count == 0) return;

            currentSearchIndex = (currentSearchIndex + 1) % searchResults.Count;
            HighlightSearchResult();
        }

        // 이전 검색 결과
        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (searchResults.Count == 0) return;

            currentSearchIndex = currentSearchIndex <= 0 ? searchResults.Count - 1 : currentSearchIndex - 1;
            HighlightSearchResult();
        }

        // 검색 결과 하이라이트
        private void HighlightSearchResult()
        {
            if (currentSearchIndex < 0 || currentSearchIndex >= searchResults.Count) return;

            int startIndex = searchResults[currentSearchIndex];
            int length = SearchTextBox.Text.Length;

            ConsoleTextBox.Select(startIndex, length);
            ConsoleTextBox.ScrollToLine(ConsoleTextBox.GetLineIndexFromCharacterIndex(startIndex));
        }

        // 검색 하이라이트 제거
        private void ClearSearchHighlight()
        {
            ConsoleTextBox.Select(0, 0);
            searchResults.Clear();
            currentSearchIndex = -1;
            lastSearchTerm = "";
        }

        // 외부에서 호출할 수 있는 공개 메서드들
        public void LogInfo(string message)
        {
            AppendLine($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            AppendLine($"[WARNING] {message}");
        }

        public void LogError(string message)
        {
            AppendLine($"[ERROR] {message}");
        }

        public void LogDebug(string message)
        {
            AppendLine($"[DEBUG] {message}");
        }
    }
}