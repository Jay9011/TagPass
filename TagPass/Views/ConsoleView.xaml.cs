using System.Windows;
using System.Windows.Controls;
using System;
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
                }

                _consoleModel = value;
                DataContext = _consoleModel;

                // 새 이벤트 설정 (필요에 따라)
                if (_consoleModel != null)
                {
                    _consoleModel.PropertyChanged += OnConsoleModelPropertyChanged;
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
            ConsoleModel = new ConsoleModel(1000);  // 기본 1000줄짜리 콘솔 객체 생성

            // 리소스 정리를 위한 이벤트 구독
            this.Unloaded += (sender, e) => ConsoleModel?.Dispose();
        }

        /// <summary>
        /// 콘솔 표시
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
            this.Focus();
        }

        /// <summary>
        /// 콘솔 숨기기
        /// </summary>
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        // ConsoleModel의 로깅 메서드들을 직접 호출
        public void LogInfo(string message) => ConsoleModel.LogInfo(message);
        public void LogWarning(string message) => ConsoleModel.LogWarning(message);
        public void LogError(string message, Exception? exception = null) => ConsoleModel.LogError(message, exception);
        public void LogDebug(string message) => ConsoleModel.LogDebug(message);
        public void AppendTimestamp(string text) => ConsoleModel.AppendLine(text);
        public void AppendLine(string text) => ConsoleModel.AppendLine(text, false);

        /// <summary>
        /// 콘솔 모델 속성 변경 이벤트
        /// </summary>
        private void OnConsoleModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ScrollBehavior Attached Property가 자동 스크롤을 처리하므로 별도 처리 불필요
            // 필요시 추가 로직 구현 가능
        }

        /// <summary>
        /// 콘솔 오버레이 닫기 요청 이벤트
        /// </summary>
        private void ConsoleOverlayPanel_CloseRequested(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ConsoleClosedEvent, this));
        }

        /// <summary>
        /// 콘솔 숨기기 버튼 클릭 이벤트
        /// </summary>
        private void HideConsole_Click(object sender, RoutedEventArgs e)
        {
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
            ConsoleModel.AppendLine("일반 메시지도 추가할 수 있습니다.", false);

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
            // 전체 콘솔 내용을 클립보드에 복사
            Clipboard.SetText(ConsoleModel.GetDisplayText());
            MessageBox.Show("콘솔 내용이 클립보드에 복사되었습니다.", "복사 완료", MessageBoxButton.OK, MessageBoxImage.Information);
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
                ConsoleModel.LogError($"MQTT 테스트 중 오류: {ex.Message}", ex);
            }
        }
    }
}