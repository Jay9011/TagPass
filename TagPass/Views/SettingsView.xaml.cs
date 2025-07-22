using System.Windows;
using System.Windows.Controls;
using SingletonManager;
using TagPass.Common;
using TagPass.Services;
using TagPass.Models;
using System.ComponentModel;

namespace TagPass.Views
{
    public partial class SettingsView : UserControl
    {
        private ISettingsService? _settingsService;

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

        #endregion

        public SettingsView()
        {
            InitializeComponent();

            // 디자인 타임이 아닐 때만 싱글톤 서비스 초기화
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // 싱글톤에서 서비스 가져오기
                _settingsService = Singletons.Instance.GetKeyedSingleton<ISettingsService>(Keys.SettingsService);

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
            if (txtBrokerIP != null) txtBrokerIP.Text = "localhost";
            if (txtBrokerPort != null) txtBrokerPort.Text = "1883";
            if (txtBrokerTopic != null) txtBrokerTopic.Text = "S1ACCESS/Normal";

            if (chkStartFullscreen != null) chkStartFullscreen.IsChecked = true;
            if (chkHideTitleBar != null) chkHideTitleBar.IsChecked = true;

            if (chkRegisterStartup != null) chkRegisterStartup.IsChecked = false;
            if (chkAutoCheckUpdates != null) chkAutoCheckUpdates.IsChecked = true;
            if (chkSendUsageStats != null) chkSendUsageStats.IsChecked = false;
        }

        public async void OnApplySettings()
        {
            if (_settingsService == null) return;

            try
            {
                var settings = GetCurrentUISettings();
                await _settingsService.SaveSettingsAsync(settings);
                RaiseEvent(new RoutedEventArgs(ApplySettingsEvent, this));

                MessageBox.Show("설정이 저장되었습니다.", "설정 저장", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void OnResetToDefaults()
        {
            if (_settingsService == null) return;

            try
            {
                await _settingsService.ResetToDefaultsAsync();
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
            if (_settingsService == null) return;

            try
            {
                var settings = await _settingsService.LoadSettingsAsync();
                UpdateUIFromSettings(settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);

                // 기본값으로 UI 설정
                var defaultSettings = _settingsService.CreateDefaultSettings();
                UpdateUIFromSettings(defaultSettings);
            }
        }

        /// <summary>
        /// 현재 UI에서 설정 값 가져오기
        /// </summary>
        private ApplicationSettings GetCurrentUISettings()
        {
            // 브로커 설정
            var brokerSettings = new BrokerSettings(
                txtBrokerIP.Text,
                txtBrokerPort.Text,
                txtBrokerTopic.Text
            );

            // 디스플레이 설정
            var displaySettings = new DisplaySettings(
                chkStartFullscreen.IsChecked ?? true,
                chkHideTitleBar.IsChecked ?? true
            );

            // 일반 설정
            var generalSettings = new GeneralSettings(
                chkRegisterStartup.IsChecked ?? false,
                chkAutoCheckUpdates.IsChecked ?? true,
                chkSendUsageStats.IsChecked ?? false
            );

            return new ApplicationSettings(brokerSettings, displaySettings, generalSettings);
        }

        /// <summary>
        /// 설정 값으로 UI 업데이트
        /// </summary>
        private void UpdateUIFromSettings(ApplicationSettings settings)
        {
            if (settings != null)
            {
                // 브로커 설정 업데이트
                txtBrokerIP.Text = settings.Broker.IP;
                txtBrokerPort.Text = settings.Broker.Port;
                txtBrokerTopic.Text = settings.Broker.Topic;

                // 디스플레이 설정 업데이트
                chkStartFullscreen.IsChecked = settings.Display.StartFullscreen;
                chkHideTitleBar.IsChecked = settings.Display.HideTitleBar;

                // 일반 설정 업데이트
                chkRegisterStartup.IsChecked = settings.General.RegisterStartupProgram;
                chkAutoCheckUpdates.IsChecked = settings.General.AutoCheckUpdates;
                chkSendUsageStats.IsChecked = settings.General.SendUsageStatistics;
            }
        }
    }
}