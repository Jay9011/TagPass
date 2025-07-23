using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Windows.Media;
using TagPass.Common;
using TagPass.Services;
using TagPass.Models;
using SingletonManager;

namespace TagPass.Views
{
    public partial class ConsoleView : UserControl
    {
        private ConsoleModel _consoleModel;
        public ConsoleModel ConsoleModel
        {
            get => _consoleModel;
            set
            {
                // 기존 이벤트 해제
                if (_consoleModel != null)
                {
                    _consoleModel.PropertyChanged -= OnConsoleModelPropertyChanged;
                    _consoleModel.SearchPositionChanged -= OnSearchPositionChanged;
                }

                _consoleModel = value;
                DataContext = _consoleModel;

                // 새 이벤트 설정
                if (_consoleModel != null)
                {
                    _consoleModel.PropertyChanged += OnConsoleModelPropertyChanged;
                    _consoleModel.SearchPositionChanged += OnSearchPositionChanged;
                }
            }
        }

        #region 이벤트

        // 콘솔 창 닫기 이벤트
        public static readonly RoutedEvent ConsoleClosedEvent =
            EventManager.RegisterRoutedEvent("ConsoleClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConsoleView));

        public event RoutedEventHandler ConsoleClosed
        {
            add { AddHandler(ConsoleClosedEvent, value); }
            remove { RemoveHandler(ConsoleClosedEvent, value); }
        }

        #endregion

        public ConsoleView()
        {
            InitializeComponent();
            ConsoleModel = new ConsoleModel();
        }

        /// <summary>
        /// 콘솔 표시
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
            this.Focus();
            ConsoleModel.LogInfo("콘솔이 열렸습니다.");
        }

        /// <summary>
        /// 콘솔 숨기기
        /// </summary>
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        public void LogInfo(string message) => ConsoleModel.LogInfo(message);
        public void LogWarning(string message) => ConsoleModel.LogWarning(message);
        public void LogError(string message) => ConsoleModel.LogError(message);
        public void LogDebug(string message) => ConsoleModel.LogDebug(message);
        public void AppendTimestamp(string text) => ConsoleModel.AppendLine(text);
        public void AppendLine(string text) => ConsoleModel.AppendLine(text, false);

        #region TextBox 찾기 관련 메서드들

        private TextBox? GetConsoleTextBox()
        {
            return FindTextBox(tb => tb.IsReadOnly);
        }

        private TextBox? GetSearchTextBox()
        {
            return FindTextBox(tb => !tb.IsReadOnly);
        }

        private TextBox? FindTextBox(Func<TextBox, bool> predicate)
        {
            return FindVisualChild<TextBox>(this, predicate);
        }

        private static T? FindVisualChild<T>(DependencyObject parent, Func<T, bool>? predicate = null) where T : class
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && (predicate == null || predicate(element)))
                {
                    return element;
                }

                var result = FindVisualChild<T>(child, predicate);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// 콘솔 오버레이 닫기 요청 이벤트
        /// </summary>
        private void ConsoleOverlayPanel_CloseRequested(object sender, RoutedEventArgs e)
        {
            ConsoleModel.LogInfo("콘솔을 닫습니다.");
            RaiseEvent(new RoutedEventArgs(ConsoleClosedEvent, this));
        }

        /// <summary>
        /// 콘솔 숨기기 버튼 클릭 이벤트
        /// </summary>
        private void HideConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleModel.LogInfo("콘솔을 숨깁니다.");
            RaiseEvent(new RoutedEventArgs(ConsoleClosedEvent, this));
        }

        /// <summary>
        /// 테스트 로그 버튼 클릭 이벤트
        /// </summary>
        private void TestLog_Click(object sender, RoutedEventArgs e)
        {
            ConsoleModel.LogInfo("이것은 정보 메시지입니다.");
            ConsoleModel.LogWarning("이것은 경고 메시지입니다.");
            ConsoleModel.LogError("이것은 오류 메시지입니다.");
            ConsoleModel.LogDebug("이것은 디버그 메시지입니다.");
            ConsoleModel.AppendLine("일반 메시지도 추가할 수 있습니다.");

            // MQTT 연결 상태 확인 및 테스트 메시지 전송
            TestMqttConnection();
        }

        /// <summary>
        /// 콘솔 지우기
        /// </summary>
        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("콘솔 내용을 모두 지우시겠습니까?", "콘솔 지우기", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ConsoleModel.Clear();
            }
        }

        /// <summary>
        /// 콘솔 내용 복사
        /// </summary>
        private void CopyConsole_Click(object sender, RoutedEventArgs e)
        {
            var consoleTextBox = GetConsoleTextBox();
            if (consoleTextBox == null) return;

            if (!string.IsNullOrEmpty(consoleTextBox.SelectedText))
            {
                Clipboard.SetText(consoleTextBox.SelectedText);
                MessageBox.Show("선택된 텍스트가 클립보드에 복사되었습니다.", "복사 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Clipboard.SetText(ConsoleModel.ConsoleText);
                MessageBox.Show("전체 콘솔 내용이 클립보드에 복사되었습니다.", "복사 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 검색 토글
        /// </summary>
        private void SearchConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleModel.SearchBarVisible = !ConsoleModel.SearchBarVisible;
        }

        /// <summary>
        /// 검색 바 닫기
        /// </summary>
        private void CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            ConsoleModel.SearchBarVisible = false;
            ConsoleModel.ClearSearchResults();
        }

        /// <summary>
        /// 검색 텍스트박스 키 이벤트
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrWhiteSpace(ConsoleModel.SearchText))
                {
                    ConsoleModel.MoveToNext();
                }
            }
            else if (e.Key == Key.Escape)
            {
                ConsoleModel.SearchBarVisible = false;
                ConsoleModel.ClearSearchResults();
            }
        }

        /// <summary>
        /// 다음 검색 결과
        /// </summary>
        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            ConsoleModel.MoveToNext();
        }

        /// <summary>
        /// 이전 검색 결과
        /// </summary>
        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            ConsoleModel.MoveToPrevious();
        }

        /// <summary>
        /// 현재 검색 결과 하이라이트
        /// </summary>
        private void HighlightCurrentSearchResult()
        {
            var consoleTextBox = GetConsoleTextBox();
            if (consoleTextBox == null || !ConsoleModel.HasSearchResults) return;

            int position = ConsoleModel.CurrentSearchPosition;
            if (position >= 0)
            {
                int length = ConsoleModel.SearchText.Length;
                consoleTextBox.Select(position, length);
                consoleTextBox.ScrollToLine(consoleTextBox.GetLineIndexFromCharacterIndex(position));
            }
            else
            {
                consoleTextBox.Select(0, 0);
            }
        }

        /// <summary>
        /// 콘솔 모델 속성 변경 이벤트
        /// </summary>
        private void OnConsoleModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 자동 스크롤 처리 (내용 추가 시 자동 스크롤이 활성화 되어있는 경우)
            if (e.PropertyName == nameof(ConsoleModel.ConsoleText) && _consoleModel.AutoScrollEnabled)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    var textBox = GetConsoleTextBox();
                    textBox?.ScrollToEnd();
                });
            }
            // 검색바 표시/숨김 시 포커스 처리
            else if (e.PropertyName == nameof(ConsoleModel.SearchBarVisible))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (_consoleModel.SearchBarVisible)
                    {
                        var searchBox = GetSearchTextBox();
                        searchBox?.Focus();
                    }
                    else
                    {
                        var consoleBox = GetConsoleTextBox();
                        consoleBox?.Focus();
                    }
                });
            }
        }

        /// <summary>
        /// 검색 위치 변경 이벤트
        /// </summary>
        private void OnSearchPositionChanged(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => HighlightCurrentSearchResult());
        }

        /// <summary>
        /// MQTT 연결 상태 확인 및 테스트
        /// </summary>
        private async void TestMqttConnection()
        {
            try
            {
                if (Singletons.Instance.TryGetKeyedSingleton<IMqttService>(Keys.MqttService, out var mqttService))
                {
                    ConsoleModel.LogInfo($"MQTT 연결 상태: {(mqttService.IsConnected ? "연결됨" : "연결 안됨")}");

                    if (mqttService.CurrentSettings != null)
                    {
                        ConsoleModel.LogInfo($"MQTT 브로커: {mqttService.CurrentSettings.IP}:{mqttService.CurrentSettings.Port}");
                        ConsoleModel.LogInfo($"MQTT 토픽: {mqttService.CurrentSettings.Topic}");
                    }

                    if (mqttService.IsConnected && mqttService.CurrentSettings != null)
                    {
                        var testMessage = $"TagPass 테스트 메시지 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        await mqttService.PublishAsync(mqttService.CurrentSettings.Topic, testMessage);
                        ConsoleModel.LogInfo($"테스트 메시지 전송: {testMessage}");
                    }
                    else
                    {
                        ConsoleModel.LogWarning("MQTT 브로커에 연결되지 않았습니다.");
                    }
                }
                else
                {
                    ConsoleModel.LogError("MQTT 서비스를 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                ConsoleModel.LogError($"MQTT 테스트 중 오류: {ex.Message}");
            }
        }
    }
}