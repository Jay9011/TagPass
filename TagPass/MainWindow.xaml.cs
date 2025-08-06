using S1SocketDataDTO.Models;
using SingletonManager;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using TagPass.Common;
using TagPass.Services;
using TagPass.Views;

namespace TagPass
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AlarmEventDto _selectedEmployee;    // 현재 선택된 직원 정보

        private IMqttService _mqttService;
        private ITimeManager _timeManager;

        #region 컴포넌트

        private Views.Components.BasicCard _basicCard;
        private Views.Components.AlarmEventList _alarmEventList;

        #endregion
        
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            WindowState = WindowState.Maximized;    // 전체화면
            WindowStyle = WindowStyle.None;         // 타이틀바 제거

            KeyDown += MainWindow_KeyDown;
            Focusable = true;

            #region 서비스 및 컴포넌트

            _timeManager = Singletons.Instance.GetKeyedSingleton<ITimeManager>(Keys.TimeManager);
            _timeManager.PropertyChanged += TimeManager_PropertyChanged;

            _basicCard = FindName("BasicCard") as Views.Components.BasicCard;

            _alarmEventList = FindName("AlarmEventListComponent") as Views.Components.AlarmEventList;
            if (_alarmEventList != null)
            {
                _alarmEventList.SelectedEmployeeChanged += OnSelectedEmployeeChanged;
            }

            InitializeMqttMessageHandling();    // MQTT 서비스 구독 및 메시지 처리 설정

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
                _alarmEventList?.AddAccessLog(eventData);

                GetConsoleView().LogInfo($"새로운 출입 이벤트: {eventData.Name} ({eventData.StateName})");
            }
            catch (Exception ex)
            {
                GetConsoleView().LogError($"출입 이벤트 처리 중 오류: {ex.Message}");
            }
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

        #region Properties

        /// <summary>
        /// 현재 시간 (TimeManager에서 가져옴)
        /// </summary>
        public DateTime CurrentTime => _timeManager?.CurrentTime ?? DateTime.Now;

        /// <summary>
        /// TimeManager의 PropertyChanged 이벤트 처리
        /// </summary>
        private void TimeManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ITimeManager.CurrentTime))
            {
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        /// <summary>
        /// 선택된 직원 정보
        /// </summary>
        public AlarmEventDto SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 선택된 직원이 변경되었을 때 처리
        /// </summary>
        private void OnSelectedEmployeeChanged(AlarmEventDto selectedEmployee)
        {
            SelectedEmployee = selectedEmployee;
            // BasicCard 업데이트
            _basicCard?.UpdateCard(selectedEmployee);
        }

        #endregion

        #region IDispose

        /// <summary>
        /// 리소스 정리
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            if (_mqttService != null)
            {
                _mqttService.MessageReceived -= OnMqttMessageReceived;
            }

            if (_alarmEventList != null)
            {
                _alarmEventList.SelectedEmployeeChanged -= OnSelectedEmployeeChanged;
            }

            if (_timeManager != null)
            {
                _timeManager.PropertyChanged -= TimeManager_PropertyChanged;
            }

            _basicCard?.Dispose();
            base.OnClosed(e);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}