using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagPass.Views;

namespace TagPass
{
    public partial class MainWindow : Window
    {
        private ConsoleView? consoleView;

        public MainWindow()
        {
            InitializeComponent();

            // 전체화면으로 시작
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None; // 타이틀바 제거 (선택사항)

            // 키보드 이벤트 핸들러 등록
            KeyDown += MainWindow_KeyDown;

            // 포커스를 설정하여 키보드 이벤트를 받을 수 있도록 함
            Focusable = true;

            // ConsoleView 인스턴스 생성
            consoleView = new ConsoleView();
        }

        private ConsoleView GetConsoleView()
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

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F12:
                    ShowSettingsOverlay();
                    e.Handled = true;
                    break;
                case Key.F5:
                    ShowConsoleOverlay();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    // ESC 키로 오버레이 닫기
                    HideSettingsOverlay();
                    HideConsoleOverlay();
                    e.Handled = true;
                    break;
                case Key.F11:
                    // F11로 전체화면 토글
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
            }
        }

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

        private void ShowConsoleOverlay()
        {
            // ConsoleView 인스턴스 설정
            if (ConsoleOverlay.PanelContent == null && consoleView != null)
            {
                ConsoleOverlay.PanelContent = consoleView;
            }

            ConsoleOverlay.Visibility = Visibility.Visible;
            ConsoleOverlay.Focus();

            // 콘솔 열릴 때 환영 메시지 추가
            GetConsoleView().LogInfo("콘솔이 열렸습니다.");
        }

        private void HideConsoleOverlay()
        {
            ConsoleOverlay.Visibility = Visibility.Collapsed;
            this.Focus();
        }

        private void ToggleFullscreen()
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        // 오버레이 패널 닫기 요청 이벤트
        private void SettingsOverlay_CloseRequested(object sender, RoutedEventArgs e)
        {
            HideSettingsOverlay();
        }

        // 콘솔 오버레이 닫기 요청 이벤트
        private void ConsoleOverlay_CloseRequested(object sender, RoutedEventArgs e)
        {
            HideConsoleOverlay();
        }

        // 설정 적용 버튼 클릭
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            // 직접 설정 적용 로직 처리
            MessageBox.Show("설정이 적용되었습니다!", "설정 적용", MessageBoxButton.OK, MessageBoxImage.Information);
            GetConsoleView().LogInfo("설정이 적용되었습니다.");
            HideSettingsOverlay();
        }

        // 기본값 복원 버튼 클릭
        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            // 직접 기본값 복원 로직 처리
            var result = MessageBox.Show("모든 설정을 기본값으로 복원하시겠습니까?", "기본값 복원",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("설정이 기본값으로 복원되었습니다!", "기본값 복원",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                GetConsoleView().LogInfo("설정이 기본값으로 복원되었습니다.");
            }
        }

        // 테스트 로그 버튼 클릭
        private void TestLog_Click(object sender, RoutedEventArgs e)
        {
            // 다양한 타입의 로그 메시지 테스트
            var console = GetConsoleView();
            console.LogInfo("이것은 정보 메시지입니다.");
            console.LogWarning("이것은 경고 메시지입니다.");
            console.LogError("이것은 오류 메시지입니다.");
            console.LogDebug("이것은 디버그 메시지입니다.");
            console.AppendLine("일반 메시지도 추가할 수 있습니다.");
        }

        // 콘솔 숨기기 버튼 클릭
        private void HideConsole_Click(object sender, RoutedEventArgs e)
        {
            GetConsoleView().LogInfo("콘솔을 숨깁니다.");
            HideConsoleOverlay();
        }

        // SettingsView에서 발생한 설정 적용 이벤트
        private void SettingsContent_ApplySettings(object sender, RoutedEventArgs e)
        {
            // 추가적인 설정 적용 로직이 필요한 경우 여기에 구현
            MessageBox.Show("SettingsView에서 설정 적용 요청!", "설정 적용", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // SettingsView에서 발생한 기본값 복원 이벤트
        private void SettingsContent_ResetToDefaults(object sender, RoutedEventArgs e)
        {
            // 추가적인 기본값 복원 로직이 필요한 경우 여기에 구현
            MessageBox.Show("SettingsView에서 기본값 복원 요청!", "기본값 복원", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}