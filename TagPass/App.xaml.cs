using FileIOHelper;
using FileIOHelper.Helpers;
using SingletonManager;
using System.IO;
using System.Windows;
using TagPass.Common;
using TagPass.Services;

namespace TagPass
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 서비스 초기화
            if (RegistIniSettingsFile() && RegistSettingsService())
            {
                RegistMqttService();
                InitializeMqttConnection();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {

            #region MQTT 해제
            try
            {
                if (Singletons.Instance.TryGetKeyedSingleton<IMqttService>(Keys.MqttService, out var mqttService))
                {
                    mqttService.ConnectionStatusChanged -= OnMqttConnectionStatusChanged;
                    mqttService.MessageReceived -= OnMqttMessageReceived;

                    if (mqttService.IsConnected)
                    {
                        _ = mqttService.DisconnectAsync(); // Fire-and-forget
                    }

                    if (mqttService is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MQTT 서비스 정리 중 오류: {ex.Message}");
            } 
            #endregion

            base.OnExit(e);
        }

        /// <summary>
        /// iniFileHelper 생성
        /// </summary>
        private bool RegistIniSettingsFile()
        {
            try
            {
                string directoryPath = Environment.CurrentDirectory;
                string settingsPath = Path.Combine(directoryPath, "settings.ini");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var iniHelper = new IniFileHelper(settingsPath);

                Singletons.Instance.AddKeyedSingleton<IIOHelper>(Keys.IniSettings, iniHelper);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"IIOHelper 초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Settings Service 인스턴스 생성 및 등록
        /// </summary>
        private bool RegistSettingsService()
        {
            try
            {
                // IIOHelper를 가져와서 SettingsService 생성
                var iniHelper = Singletons.Instance.GetKeyedSingleton<IIOHelper>(Keys.IniSettings);
                var settingsService = SettingsService.CreateInstance(iniHelper);

                Singletons.Instance.AddKeyedSingleton<ISettingsService>(Keys.SettingsService, settingsService);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SettingsService 초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// MQTT Service 인스턴스 생성 및 등록
        /// </summary>
        private bool RegistMqttService()
        {
            try
            {
                var mqttService = MqttService.CreateInstance();

                // 연결 상태 변경 이벤트 구독
                mqttService.ConnectionStatusChanged += OnMqttConnectionStatusChanged;

                // 메시지 수신 이벤트 구독 (필요시)
                mqttService.MessageReceived += OnMqttMessageReceived;

                Singletons.Instance.AddKeyedSingleton<IMqttService>(Keys.MqttService, mqttService);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MqttService 초기화 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// 설정 파일에서 브로커 설정을 로드하여 MQTT 연결 초기화
        /// </summary>
        private async void InitializeMqttConnection()
        {
            try
            {
                var settingsService = Singletons.Instance.GetKeyedSingleton<ISettingsService>(Keys.SettingsService);
                var mqttService = Singletons.Instance.GetKeyedSingleton<IMqttService>(Keys.MqttService);

                // 설정 로드
                var applicationSettings = await settingsService.LoadSettingsAsync();

                // MQTT 브로커에 연결
                await mqttService.ConnectAsync(applicationSettings.Broker);

                System.Diagnostics.Debug.WriteLine($"MQTT 브로커 연결 시도: {applicationSettings.Broker.IP}:{applicationSettings.Broker.Port}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MQTT 초기 연결 실패: {ex.Message}");
                // 초기 연결 실패는 치명적이지 않으므로 애플리케이션 계속 실행
            }
        }

        /// <summary>
        /// MQTT 연결 상태 변경 이벤트 핸들러
        /// </summary>
        private void OnMqttConnectionStatusChanged(object? sender, bool isConnected)
        {
            var status = isConnected ? "연결됨" : "연결 해제됨";
            System.Diagnostics.Debug.WriteLine($"MQTT 브로커 상태: {status}");

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (Current.MainWindow is MainWindow mainWindow)
                    {
                        var consoleView = mainWindow.GetConsoleView();

                        if (isConnected)
                        {
                            consoleView.LogInfo("MQTT 브로커에 연결되었습니다.");

                            // 현재 연결 정보 표시
                            if (sender is IMqttService mqttService && mqttService.CurrentSettings != null)
                            {
                                consoleView.LogInfo($"브로커: {mqttService.CurrentSettings.IP}:{mqttService.CurrentSettings.Port}");
                                consoleView.LogInfo($"토픽: {mqttService.CurrentSettings.Topic}");
                            }
                        }
                        else
                        {
                            consoleView.LogWarning("MQTT 브로커 연결이 해제되었습니다.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"콘솔 로그 출력 중 오류: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// MQTT 메시지 수신 이벤트 핸들러
        /// </summary>
        private void OnMqttMessageReceived(object? sender, MqttMessageEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"MQTT 메시지 수신 - 토픽: {e.Topic}, 내용: {e.Payload}");

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (Current.MainWindow is MainWindow mainWindow)
                    {
                        var consoleView = mainWindow.GetConsoleView();

                        consoleView.LogInfo($"[MQTT 수신] 토픽: {e.Topic}");
                        consoleView.LogInfo($"[MQTT 수신] 내용: {e.Payload}");
                        if (e.Retain)
                        {
                            consoleView.LogInfo("[MQTT 수신] Retained 메시지");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"콘솔 로그 출력 중 오류: {ex.Message}");
                }
            });
        }
    }
}
