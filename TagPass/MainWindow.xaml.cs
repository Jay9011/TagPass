using SingletonManager;
using System.Windows;
using System.Windows.Input;
using TagPass.Common;
using TagPass.Services;
using TagPass.Views;
using TagPass.Models;
using System.Windows.Threading;
using S1SocketDataDTO.Models;
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TagPass
{
    public partial class MainWindow : Window
    {
        private AccessMonitorViewModel _viewModel;
        private DispatcherTimer _accessEventTimer;

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

            // BasicCard 참조 가져오기
            _basicCard = FindName("BasicCard") as Views.Components.BasicCard;

            #region 출입 이벤트 시뮬레이션

            _accessEventTimer = new DispatcherTimer();
            _accessEventTimer.Tick += (s, e) =>
            {
                SimulateNewAccessEvent();
                SetRandomInterval();
            };
            SetRandomInterval();
            _accessEventTimer.Start(); 

            #endregion
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

        #region 테스트

        /// <summary>
        /// 랜덤 간격으로 타이머 설정
        /// </summary>
        private void SetRandomInterval()
        {
            var random = new Random();
            double randomSeconds = (random.NextDouble() * 1.5) + 1.5; // 1.5 ~ 3.0초 사이 랜덤
            _accessEventTimer.Interval = TimeSpan.FromSeconds(randomSeconds);
        }

        /// <summary>
        /// 새로운 출입 이벤트 시뮬레이션
        /// </summary>
        private void SimulateNewAccessEvent()
        {
            var random = new Random();
            var names = new[] { "홍길동", "김철수", "이영희", "박민수", "정수연", "최동훈", "강지민", "윤서준" };
            var departments = new[] { "개발팀", "인사팀", "영업팀", "기획팀", "총무팀", "마케팅팀", "디자인팀" };
            var positions = new[] { "사원", "주임", "대리", "과장", "팀장", "부장" };
            var locations = new[] { "정문 출입구", "후문 출입구", "주차장 출입구", "비상구" };
            var states = new[] { "IN", "OUT" };

            var newEvent = new AlarmEventDto
            {
                AlarmID = random.Next(1000, 9999),
                FormattedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Sabun = $"2024{random.Next(100, 999):D3}",
                Name = names[random.Next(names.Length)],
                OrgName = departments[random.Next(departments.Length)],
                GradeName = positions[random.Next(positions.Length)],
                State = states[random.Next(states.Length)],
                StateName = states[random.Next(states.Length)] == "IN" ? "출입" : "퇴실",
                Photo = $"/Images/sample{random.Next(1, 6)}.jpg",
                EqName = locations[random.Next(locations.Length)],
                LocationName = "본사 1층"
            };

            // BasicCard에 새로운 출입 이벤트 표시 (자동으로 타이머도 관리됨)
            _basicCard?.UpdateCard(newEvent);

            // ViewModel에 새 이벤트 추가
            _viewModel.AddAccessLog(newEvent);
        }

        #endregion

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
            _accessEventTimer?.Stop();  // 타이머 정리
            _basicCard?.Dispose();      // BasicCard 리소스 정리

            base.OnClosed(e);
        }
    }
}