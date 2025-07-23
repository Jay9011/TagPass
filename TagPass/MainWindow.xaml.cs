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
        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;    // 전체화면
            WindowStyle = WindowStyle.None;         // 타이틀바 제거

            // 키보드 이벤트 핸들러 등록
            KeyDown += MainWindow_KeyDown;
            Focusable = true;
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
    }
}