using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections.Generic;
using FileIOHelper;
using FileIOHelper.Helpers;

namespace TagPass.Views
{
    public partial class SettingsView : UserControl
    {
        // INI 파일 경로
        private readonly string settingsFilePath;
        private IIOHelper iniHelper;

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

            // 경로 설정
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TagPass");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            settingsFilePath = Path.Combine(appDataPath, "settings.ini");

            // ini 파일 헬퍼 초기화
            iniHelper = new IniFileHelper(settingsFilePath);

            // 설정 로드
            LoadSettings();
        }

        public void OnApplySettings()
        {
            // 설정 저장
            SaveSettings();
            RaiseEvent(new RoutedEventArgs(ApplySettingsEvent, this));
        }

        public void OnResetToDefaults()
        {
            // 기본값으로 복원
            ResetToDefaultValues();
            RaiseEvent(new RoutedEventArgs(ResetToDefaultsEvent, this));
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                iniHelper.WriteValue("Broker", "IP", txtBrokerIP.Text);
                iniHelper.WriteValue("Broker", "Port", txtBrokerPort.Text);
                iniHelper.WriteValue("Broker", "Topic", txtBrokerTopic.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 설정 값 로드
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsFilePath))
                {
                    CreateDefaultSettings();
                    return;
                }

                string brokerIP = iniHelper.ReadValue("Broker", "IP");
                string brokerPort = iniHelper.ReadValue("Broker", "Port");
                string brokerTopic = iniHelper.ReadValue("Broker", "Topic");

                // UI에 값 설정
                if (!string.IsNullOrEmpty(brokerIP))
                    txtBrokerIP.Text = brokerIP;
                if (!string.IsNullOrEmpty(brokerPort))
                    txtBrokerPort.Text = brokerPort;
                if (!string.IsNullOrEmpty(brokerTopic))
                    txtBrokerTopic.Text = brokerTopic;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateDefaultSettings();
            }
        }

        /// <summary>
        /// 초기 설정 값으로 파일 생성
        /// </summary>
        private void CreateDefaultSettings()
        {
            try
            {
                // 기본값 설정
                txtBrokerIP.Text = "localhost";
                txtBrokerPort.Text = "1883";
                txtBrokerTopic.Text = "S1ACCESS/Normal";

                // 설정 저장
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"기본 설정 생성 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 설정 값을 기본 값으로 재설정
        /// </summary>
        public void ResetToDefaultValues()
        {
            try
            {
                CreateDefaultSettings();
                MessageBox.Show("설정이 기본값으로 복원되었습니다.", "기본값 복원", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"기본값 복원 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}