using SingletonManager;
using System.Windows;
using System.Windows.Input;
using TagPass.Common;
using TagPass.Services;
using TagPass.Views;

namespace TagPass
{
    public partial class MainWindow : Window
    {
        private ConsoleView? consoleView;

        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;    // 전체화면
            WindowStyle = WindowStyle.None;         // 타이틀바 제거

            // 키보드 이벤트 핸들러 등록
            KeyDown += MainWindow_KeyDown;
            Focusable = true;

            // View 인스턴스
            consoleView = new ConsoleView();
        }

        public ConsoleView GetConsoleView()
        {
            // ConsoleOverlay에서 ConsoleView를 찾아서 반환
            if (ConsoleOverlay.PanelContent is ConsoleView console)
            {
                return console;
            }

            // 찾지 못한 경우 새로 생성
            if (consoleView == null)
            {
                consoleView = new ConsoleView();
                ConsoleOverlay.PanelContent = consoleView;
            }

            return consoleView;
        }

        /// <summary>
        /// SettingsView 인스턴스 가져오기
        /// </summary>
        private SettingsView? GetSettingsView()
        {
            if (SettingsOverlay.PanelContent is SettingsView settingsView)
            {
                return settingsView;
            }
            return null;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    ShowConsoleOverlay();   // 콘솔 오버레이
                    e.Handled = true;
                    break;
                case Key.F11:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
                case Key.F12:
                    ShowSettingsOverlay();  // 세팅 오버레이
                    e.Handled = true;
                    break;
                case Key.Escape:
                    // ESC 키로 오버레이 닫기
                    HideSettingsOverlay();
                    HideConsoleOverlay();
                    e.Handled = true;
                    break;
            }
        }

        #region 설정 오버레이

        private void ShowSettingsOverlay()
        {
            SettingsOverlay.Visibility = Visibility.Visible;
            SettingsOverlay.Focus();
        }

        private void HideSettingsOverlay()
        {
            SettingsOverlay.Visibility = Visibility.Collapsed;
            this.Focus();
        }

        private void SettingsOverlay_CloseRequested(object sender, RoutedEventArgs e)
        {
            HideSettingsOverlay();
            GetConsoleView().LogInfo("설정 창이 닫혔습니다.");
        }

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsView = GetSettingsView();
            if (settingsView != null)
            {
                settingsView.OnApplySettings();
                GetConsoleView().LogInfo("설정 적용이 요청되었습니다.");
            }
            else
            {
                MessageBox.Show("설정 뷰를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("설정을 기본값으로 복원하시겠습니까?", "기본값 복원", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var settingsView = GetSettingsView();
                if (settingsView != null)
                {
                    settingsView.OnResetToDefaults();
                    GetConsoleView().LogInfo("설정 기본값 복원이 요청되었습니다.");
                }
                else
                {
                    MessageBox.Show("설정 뷰를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SettingsContent_ApplySettings(object sender, RoutedEventArgs e)
        {
            GetConsoleView().LogInfo("설정이 성공적으로 적용되었습니다.");
            HideSettingsOverlay(); // 설정 적용 후 오버레이 닫기
        }

        private void SettingsContent_ResetToDefaults(object sender, RoutedEventArgs e)
        {
            GetConsoleView().LogInfo("설정이 기본값으로 복원되었습니다.");
        }

        #endregion

        #region 콘솔 오버레이 표시

        private void ShowConsoleOverlay()
        {
            if (ConsoleOverlay.PanelContent == null && consoleView != null)
            {
                ConsoleOverlay.PanelContent = consoleView;
            }

            ConsoleOverlay.Visibility = Visibility.Visible;
            ConsoleOverlay.Focus();

            GetConsoleView().LogInfo("콘솔이 열렸습니다.");
        }

        private void HideConsoleOverlay()
        {
            ConsoleOverlay.Visibility = Visibility.Collapsed;
            this.Focus();
        }

        private void HideConsole_Click(object sender, RoutedEventArgs e)
        {
            GetConsoleView().LogInfo("콘솔을 숨깁니다.");
            HideConsoleOverlay();
        }

        /// <summary>
        /// 테스트 로그
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestLog_Click(object sender, RoutedEventArgs e)
        {
            // 다양한 타입의 로그 메시지 테스트
            var console = GetConsoleView();
            console.LogInfo("이것은 정보 메시지입니다.");
            console.LogWarning("이것은 경고 메시지입니다.");
            console.LogError("이것은 오류 메시지입니다.");
            console.LogDebug("이것은 디버그 메시지입니다.");
            console.AppendLine("일반 메시지도 추가할 수 있습니다.");

            // MQTT 연결 상태 확인 및 테스트 메시지 전송
            TestMqttConnection();
        }

        private void ConsoleOverlay_CloseRequested(object sender, RoutedEventArgs e)
        {
            GetConsoleView().LogInfo("콘솔을 닫습니다.");
            HideConsoleOverlay();
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
        /// MQTT 연결 상태 확인 및 테스트
        /// </summary>
        private async void TestMqttConnection()
        {
            try
            {
                var console = GetConsoleView();

                if (Singletons.Instance.TryGetKeyedSingleton<IMqttService>(Keys.MqttService, out var mqttService))
                {
                    console.LogInfo($"MQTT 연결 상태: {(mqttService.IsConnected ? "연결됨" : "연결 안됨")}");

                    if (mqttService.CurrentSettings != null)
                    {
                        console.LogInfo($"MQTT 브로커: {mqttService.CurrentSettings.IP}:{mqttService.CurrentSettings.Port}");
                        console.LogInfo($"MQTT 토픽: {mqttService.CurrentSettings.Topic}");
                    }

                    if (mqttService.IsConnected && mqttService.CurrentSettings != null)
                    {
                        var testMessage = $"TagPass 테스트 메시지 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                        await mqttService.PublishAsync(mqttService.CurrentSettings.Topic, testMessage);
                        console.LogInfo($"테스트 메시지 전송: {testMessage}");
                    }
                    else
                    {
                        console.LogWarning("MQTT 브로커에 연결되지 않았습니다.");
                    }
                }
                else
                {
                    console.LogError("MQTT 서비스를 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                GetConsoleView().LogError($"MQTT 테스트 중 오류: {ex.Message}");
            }
        }

    }
}