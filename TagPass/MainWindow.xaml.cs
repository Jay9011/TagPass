using SingletonManager;
using System.Windows;
using System.Windows.Input;
using TagPass.Common;
using TagPass.Services;
using TagPass.Views;
using TagPass.Models;
using S1SocketDataDTO.Models;
using System;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.IO;

namespace TagPass
{
    public partial class MainWindow : Window
    {
        private AccessMonitorViewModel _viewModel;
        private IMqttService _mqttService;

        // BasicCard 참조
        private Views.Components.BasicCard _basicCard;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new AccessMonitorViewModel();
            DataContext = _viewModel;

            WindowState = WindowState.Maximized;    // 전체화면
            WindowStyle = WindowStyle.None;         // 타이틀바 제거

            // 키보드 이벤트 핸들러 등록
            KeyDown += MainWindow_KeyDown;
            Focusable = true;

            _basicCard = FindName("BasicCard") as Views.Components.BasicCard;   // BasicCard 참조 가져오기

            InitializeMqttMessageHandling();    // MQTT 서비스 구독 및 메시지 처리 설정
        }

        public ConsoleView GetConsoleView()
        {
            return this.ConsoleView;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    ShowConsoleView();
                    e.Handled = true;
                    break;
                case Key.F11:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
                case Key.F12:
                    ShowSettingsView();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    // ESC 키로 오버레이 닫기
                    HideSettingsView();
                    HideConsoleView();
                    e.Handled = true;
                    break;
            }
        }

        #region 설정 뷰 관리

        private void ShowSettingsView()
        {
            this.SettingsView.Show();
        }

        private void HideSettingsView()
        {
            this.SettingsView.Hide();
            this.Focus();
        }

        private void SettingsView_SettingsClosed(object sender, RoutedEventArgs e)
        {
            HideSettingsView();
            GetConsoleView().LogInfo("설정 창이 닫혔습니다.");
        }

        #endregion

        #region 콘솔 뷰 관리

        private void ShowConsoleView()
        {
            this.ConsoleView.Show();
        }

        private void HideConsoleView()
        {
            this.ConsoleView.Hide();
            this.Focus();
        }

        private void ConsoleView_ConsoleClosed(object sender, RoutedEventArgs e)
        {
            HideConsoleView();
        }

        #endregion

        #region 이벤트 수신

        /// <summary>
        /// MQTT 메시지 처리 초기화
        /// </summary>
        private void InitializeMqttMessageHandling()
        {
            try
            {
                _mqttService = Singletons.Instance.GetKeyedSingleton<IMqttService>(Keys.MqttService);
                _mqttService.MessageReceived += OnMqttMessageReceived;

                GetConsoleView().LogInfo("MQTT 메시지 처리기가 초기화되었습니다.");
            }
            catch (Exception ex)
            {
                GetConsoleView().LogError($"MQTT 메시지 처리기 초기화 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// MQTT 메시지 수신 이벤트 핸들러
        /// </summary>
        private void OnMqttMessageReceived(object? sender, MqttMessageEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(e.Payload))
                {
                    var eventData = TryParseAlarmEventDto(e.Payload);   // XML 메시지를 AlarmEventDto로 변환 시도

                    if (eventData != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            ProcessAccessEvent(eventData);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                GetConsoleView().LogError($"MQTT 메시지 처리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// XML 페이로드를 AlarmEventDto로 변환 시도
        /// </summary>
        /// <param name="xmlPayload">XML 페이로드</param>
        /// <returns>변환된 AlarmEventDto 또는 null</returns>
        private AlarmEventDto? TryParseAlarmEventDto(string xmlPayload)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(AlarmEventDto));

                using (var stringReader = new StringReader(xmlPayload))
                {
                    var eventData = serializer.Deserialize(stringReader) as AlarmEventDto;

                    if (eventData != null)
                    {
                        return eventData;
                    }

                    return null;
                }
            }
            catch (InvalidOperationException xmlEx)
            {
                GetConsoleView().LogError($"[MQTT 처리] XML 파싱 오류: {xmlEx.Message}");
                return null;
            }
            catch (Exception parseEx)
            {
                GetConsoleView().LogError($"[MQTT 처리] 데이터 처리 오류: {parseEx.Message}");
                return null;
            }
        }

        /// <summary>
        /// MQTT로 수신된 출입 이벤트 데이터 처리
        /// </summary>
        /// <param name="eventData">출입 이벤트 데이터</param>
        public void ProcessAccessEvent(AlarmEventDto eventData)
        {
            if (eventData == null) return;

            try
            {
                _basicCard?.UpdateCard(eventData);
                _viewModel.AddAccessLog(eventData);

                GetConsoleView().LogInfo($"새로운 출입 이벤트: {eventData.Name} ({eventData.StateName})");
            }
            catch (Exception ex)
            {
                GetConsoleView().LogError($"출입 이벤트 처리 중 오류: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 풀스크린 토글
        /// </summary>
        private void ToggleFullscreen()
        {
            if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                GetConsoleView().LogInfo("창 모드로 전환되었습니다.");
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                GetConsoleView().LogInfo("전체화면 모드로 전환되었습니다.");
            }
        }

        /// <summary>
        /// 자식 요소를 재귀적으로 찾는 헬퍼 메서드
        /// </summary>
        private T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childOfChild = FindChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // MQTT 이벤트 구독 해제
            if (_mqttService != null)
            {
                _mqttService.MessageReceived -= OnMqttMessageReceived;
            }

            _viewModel?.Dispose();      // ViewModel 리소스 정리
            _basicCard?.Dispose();      // BasicCard 리소스 정리
            base.OnClosed(e);
        }
    }
}