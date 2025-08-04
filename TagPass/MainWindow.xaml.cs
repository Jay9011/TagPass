using SingletonManager;
using System.Windows;
using System.Windows.Input;
using TagPass.Common;
using TagPass.Services;
using TagPass.Views;
using TagPass.Models;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using S1SocketDataDTO.Models;
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace TagPass
{
    public partial class MainWindow : Window
    {
        private AccessMonitorViewModel _viewModel;
        private DispatcherTimer _accessEventTimer;

        // 카드 관리를 위한 변수들
        private readonly List<EmployeeCardInfo> _employeeCards;
        private int _currentCardIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            // ViewModel 초기화 및 DataContext 설정
            _viewModel = new AccessMonitorViewModel();
            DataContext = _viewModel;

            WindowState = WindowState.Maximized;    // 전체화면
            WindowStyle = WindowStyle.None;         // 타이틀바 제거

            // 키보드 이벤트 핸들러 등록
            KeyDown += MainWindow_KeyDown;
            Focusable = true;

            // 카드 정보 초기화
            _employeeCards = new List<EmployeeCardInfo>
            {
                new EmployeeCardInfo
                {
                    BorderElement = FindName("EmployeeCard1") as System.Windows.Controls.Border,
                    BasicCardElement = FindName("BasicCard1") as Views.Components.BasicCard,
                    IsInUse = false
                },
                new EmployeeCardInfo
                {
                    BorderElement = FindName("EmployeeCard2") as System.Windows.Controls.Border,
                    BasicCardElement = FindName("BasicCard2") as Views.Components.BasicCard,
                    IsInUse = false
                }
            };

            // 출입 이벤트 시뮬레이션 타이머 시작
            _accessEventTimer = new DispatcherTimer();
            _accessEventTimer.Tick += (s, e) =>
            {
                SimulateNewAccessEvent();
                // 다음 이벤트를 위한 랜덤 간격 설정
                SetRandomInterval();
            };
            SetRandomInterval(); // 첫 번째 간격 설정
            _accessEventTimer.Start();
        }

        /// <summary>
        /// 랜덤 간격으로 타이머 설정
        /// </summary>
        private void SetRandomInterval()
        {
            var random = new Random();
            double randomSeconds = (random.NextDouble() * 3.5) + 1.5; // 1.5 ~ 5.0초 사이 랜덤
            _accessEventTimer.Interval = TimeSpan.FromSeconds(randomSeconds);
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

        /// <summary>
        /// 사원 카드 정보를 담는 클래스
        /// </summary>
        private class EmployeeCardInfo
        {
            public System.Windows.Controls.Border BorderElement { get; set; }
            public Views.Components.BasicCard BasicCardElement { get; set; }
            public bool IsInUse { get; set; }
            public DispatcherTimer HideTimer { get; set; }
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

            // 애니메이션 시작
            ShowAccessNotification(newEvent);

            // ViewModel에 새 이벤트 추가
            _viewModel.AddAccessLog(newEvent);
        }

        /// <summary>
        /// 출입 알림 애니메이션 표시
        /// </summary>
        /// <param name="accessEvent">출입 이벤트 데이터</param>
        private void ShowAccessNotification(AlarmEventDto accessEvent)
        {
            // 새 이벤트 발생 시 현재 표시 중인 카드들을 즉시 숨기기
            var cardsInUse = _employeeCards.Where(c => c.IsInUse).ToList();
            foreach (var cardInUse in cardsInUse)
            {
                TriggerImmediateHide(cardInUse);
            }

            // 사용 가능한 카드 찾기 (즉시 숨김 처리된 카드 포함)
            var availableCard = _employeeCards.FirstOrDefault(c => !c.IsInUse);

            if (availableCard == null)
            {
                // 모든 카드가 사용 중이면 가장 오래된 카드를 강제로 숨기고 사용
                availableCard = _employeeCards[_currentCardIndex];
                ForceHideCard(availableCard);
            }

            // 카드 사용 중으로 표시
            availableCard.IsInUse = true;

            // ShowAnimation 실행 (데이터 바인딩은 애니메이션 시작과 동시에)
            if (availableCard.BorderElement != null)
            {
                var showTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(0.2) // 200ms 지연
                };

                showTimer.Tick += (s, e) =>
                {
                    showTimer.Stop();

                    // 애니메이션 시작과 동시에 데이터 설정
                    if (availableCard.BasicCardElement != null)
                    {
                        availableCard.BasicCardElement.DataContext = accessEvent;
                    }

                    var showStoryboard = FindResource("CardShowAnimation") as Storyboard;
                    if (showStoryboard != null)
                    {
                        // 타겟 설정
                        Storyboard.SetTarget(showStoryboard, availableCard.BorderElement);

                        // 애니메이션 완료 후 HideAnimation 예약
                        showStoryboard.Completed += (sender, args) => ScheduleHideAnimation(availableCard);

                        showStoryboard.Begin();
                    }
                };

                showTimer.Start();
            }

            // 다음 카드 인덱스로 이동
            _currentCardIndex = (_currentCardIndex + 1) % _employeeCards.Count;
        }

        /// <summary>
        /// HideAnimation을 예약하여 실행
        /// </summary>
        /// <param name="cardInfo">숨길 카드 정보</param>
        private void ScheduleHideAnimation(EmployeeCardInfo cardInfo)
        {
            // 기존 타이머가 있으면 정지
            cardInfo.HideTimer?.Stop();

            // 2.5초 후 HideAnimation 실행
            cardInfo.HideTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.5)
            };

            cardInfo.HideTimer.Tick += (s, e) =>
            {
                cardInfo.HideTimer.Stop();
                HideCard(cardInfo);
            };

            cardInfo.HideTimer.Start();
        }

        /// <summary>
        /// 카드 숨김 애니메이션 실행
        /// </summary>
        /// <param name="cardInfo">숨길 카드 정보</param>
        private void HideCard(EmployeeCardInfo cardInfo)
        {
            if (cardInfo.BorderElement != null)
            {
                var hideStoryboard = FindResource("CardHideAnimation") as Storyboard;
                if (hideStoryboard != null)
                {
                    // 타겟 설정
                    Storyboard.SetTarget(hideStoryboard, cardInfo.BorderElement);

                    // 애니메이션 완료 후 카드 리셋
                    hideStoryboard.Completed += (s, e) => ResetCard(cardInfo);

                    hideStoryboard.Begin();
                }
            }
        }

        /// <summary>
        /// 카드를 강제로 숨기기 (즉시 실행)
        /// </summary>
        /// <param name="cardInfo">강제로 숨길 카드 정보</param>
        private void ForceHideCard(EmployeeCardInfo cardInfo)
        {
            // 진행 중인 타이머 정지
            cardInfo.HideTimer?.Stop();

            // 즉시 초기 상태로 리셋
            ResetCard(cardInfo);
        }

        /// <summary>
        /// 카드를 초기 상태로 리셋
        /// </summary>
        /// <param name="cardInfo">리셋할 카드 정보</param>
        private void ResetCard(EmployeeCardInfo cardInfo)
        {
            if (cardInfo.BorderElement != null)
            {
                cardInfo.BorderElement.Opacity = 0;
                System.Windows.Controls.Canvas.SetLeft(cardInfo.BorderElement, -400);

                if (cardInfo.BorderElement.RenderTransform is ScaleTransform transform)
                {
                    transform.ScaleX = 0.8;
                    transform.ScaleY = 0.8;
                }
            }

            // 사용 중 상태 해제
            cardInfo.IsInUse = false;

            // 타이머 정리
            cardInfo.HideTimer?.Stop();
            cardInfo.HideTimer = null;
        }

        /// <summary>
        /// 현재 표시 중인 카드를 즉시 HideAnimation으로 숨기기
        /// </summary>
        /// <param name="cardInfo">즉시 숨길 카드 정보</param>
        private void TriggerImmediateHide(EmployeeCardInfo cardInfo)
        {
            // 기존 HideTimer 정지
            cardInfo.HideTimer?.Stop();
            cardInfo.HideTimer = null;

            // 즉시 사용 중 상태 해제 (다른 카드가 바로 사용 가능하도록)
            cardInfo.IsInUse = false;

            // 데이터 바인딩을 null로 설정하여 겹침 방지
            if (cardInfo.BasicCardElement != null)
            {
                cardInfo.BasicCardElement.DataContext = null;
            }

            // 즉시 HideAnimation 실행
            if (cardInfo.BorderElement != null)
            {
                var hideStoryboard = FindResource("CardImmediateHideAnimation") as Storyboard;
                if (hideStoryboard != null)
                {
                    // 타겟 설정
                    Storyboard.SetTarget(hideStoryboard, cardInfo.BorderElement);

                    // 애니메이션 완료 후 카드 완전 리셋 (이미 IsInUse는 false로 설정됨)
                    hideStoryboard.Completed += (s, e) =>
                    {
                        // 물리적 위치만 리셋 (IsInUse는 이미 false)
                        if (cardInfo.BorderElement != null)
                        {
                            cardInfo.BorderElement.Opacity = 0;
                            System.Windows.Controls.Canvas.SetLeft(cardInfo.BorderElement, -400);

                            if (cardInfo.BorderElement.RenderTransform is ScaleTransform transform)
                            {
                                transform.ScaleX = 0.8;
                                transform.ScaleY = 0.8;
                            }
                        }
                    };

                    hideStoryboard.Begin();
                }
            }
        }

        /// <summary>
        /// 자식 요소를 재귀적으로 찾는 헬퍼 메서드
        /// </summary>
        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
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
    }
}