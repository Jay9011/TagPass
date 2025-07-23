using System.Windows;
using System.Windows.Controls;
using SingletonManager;
using TagPass.Common;
using TagPass.Services;
using TagPass.Models;
using System.ComponentModel;
using System.Collections.Generic;

namespace TagPass.Views
{
    public partial class SettingsView : UserControl
    {
        private ISettingsService? settingsService;

        // ApplicationSettings를 DataContext로 사용
        private ApplicationSettings _settings;
        public ApplicationSettings Settings
        {
            get => _settings ?? new ApplicationSettings();
            set
            {
                _settings = value;
                DataContext = _settings;
            }
        }

        #region 이벤트

        // 설정 적용 이벤트
        public static readonly RoutedEvent ApplySettingsEvent =
            EventManager.RegisterRoutedEvent("ApplySettings", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsView));

        public event RoutedEventHandler ApplySettings
        {
            add { AddHandler(ApplySettingsEvent, value); }
            remove { RemoveHandler(ApplySettingsEvent, value); }
        }

        // 기본값 복원 이벤트
        public static readonly RoutedEvent ResetToDefaultsEvent =
            EventManager.RegisterRoutedEvent("ResetToDefaults", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsView));

        public event RoutedEventHandler ResetToDefaults
        {
            add { AddHandler(ResetToDefaultsEvent, value); }
            remove { RemoveHandler(ResetToDefaultsEvent, value); }
        }

        // 설정 창 닫기 이벤트
        public static readonly RoutedEvent SettingsClosedEvent =
            EventManager.RegisterRoutedEvent("SettingsClosed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsView));

        public event RoutedEventHandler SettingsClosed
        {
            add { AddHandler(SettingsClosedEvent, value); }
            remove { RemoveHandler(SettingsClosedEvent, value); }
        }

        #endregion

        public SettingsView()
        {
            InitializeComponent();

            // 디자인 타임이 아닐 때만 싱글톤 서비스 초기화
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // 싱글톤에서 서비스 가져오기
                settingsService = Singletons.Instance.GetKeyedSingleton<ISettingsService>(Keys.SettingsService);

                // 설정 로드 (비동기 호출을 동기적으로 처리)
                Loaded += async (s, e) => await LoadSettingsAsync();
            }
            else
            {
                // 디자인 타임에는 기본값으로 UI 설정
                SetDesignTimeDefaults();
            }
        }

        /// <summary>
        /// 디자인 타임용 기본값 설정
        /// </summary>
        private void SetDesignTimeDefaults()
        {
            Settings = new ApplicationSettings();
        }

        public async void OnApplySettings()
        {
            if (settingsService == null || Settings == null) return;

            try
            {
                await settingsService.SaveSettingsAsync(Settings);

                // MQTT 서비스에 새로운 설정 적용
                await UpdateMqttConnection(Settings);

                RaiseEvent(new RoutedEventArgs(ApplySettingsEvent, this));

                MessageBox.Show("설정이 저장되었습니다.", "설정 저장", MessageBoxButton.OK, MessageBoxImage.Information);

                // 설정 적용 후 창 닫기
                RaiseEvent(new RoutedEventArgs(SettingsClosedEvent, this));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// MQTT 서비스 연결 업데이트
        /// </summary>
        private async Task UpdateMqttConnection(ApplicationSettings settings)
        {
            try
            {
                // 싱글톤에서 MQTT 서비스 가져오기
                if (Singletons.Instance.TryGetKeyedSingleton<IMqttService>(Keys.MqttService, out var mqttService))
                {
                    // 기존 연결이 있다면 해제
                    if (mqttService.IsConnected)
                    {
                        await mqttService.DisconnectAsync();
                    }

                    // 새로운 설정으로 연결
                    await mqttService.ConnectAsync(settings.Broker);
                }
            }
            catch (Exception ex)
            {
                // MQTT 연결 오류는 로그만 출력하고 설정 저장은 계속 진행
                System.Diagnostics.Debug.WriteLine($"MQTT 재연결 실패: {ex.Message}");
            }
        }

        public async void OnResetToDefaults()
        {
            if (settingsService == null) return;

            try
            {
                await settingsService.ResetToDefaultsAsync();
                await LoadSettingsAsync();
                RaiseEvent(new RoutedEventArgs(ResetToDefaultsEvent, this));

                MessageBox.Show("설정이 기본값으로 복원되었습니다.", "기본값 복원", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"기본값 복원 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 설정 값 로드
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            if (settingsService == null) return;

            try
            {
                var settings = await settingsService.LoadSettingsAsync();
                Settings = settings; // DataContext 자동 설정됨
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);

                // 기본값으로 UI 설정
                var defaultSettings = settingsService.CreateDefaultSettings();
                Settings = defaultSettings;
            }
        }

        #region 이벤트 핸들러

        /// <summary>
        /// 설정 오버레이 닫기 요청 이벤트
        /// </summary>
        private void SettingsOverlayPanel_CloseRequested(object sender, RoutedEventArgs e)
        {
            // 상위로 이벤트 전파
            RaiseEvent(new RoutedEventArgs(SettingsClosedEvent, this));
        }

        /// <summary>
        /// 적용 버튼 클릭 이벤트
        /// </summary>
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            OnApplySettings();
        }

        /// <summary>
        /// 기본값 복원 버튼 클릭 이벤트
        /// </summary>
        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("설정을 기본값으로 복원하시겠습니까?", "기본값 복원",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OnResetToDefaults();
            }
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 설정 표시
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
            SettingsOverlayPanel.Focus();
        }

        /// <summary>
        /// 설정 숨기기
        /// </summary>
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}