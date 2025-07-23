using MQTTnet;
using TagPass.Models;

namespace TagPass.Services
{
    public class MqttService : IMqttService, IDisposable
    {
        public BrokerSettings? CurrentSettings => currentSettings;
        public bool IsConnected => mqttClient?.IsConnected ?? false;

        private IMqttClient? mqttClient;
        private BrokerSettings? currentSettings;

        private bool disposed = false;
        private Timer? reconnectTimer;

        /// <summary>
        /// 연결 상태 변경 이벤트
        /// </summary>
        public event EventHandler<bool>? ConnectionStatusChanged;

        /// <summary>
        /// 메시지 수신 이벤트
        /// </summary>
        public event EventHandler<MqttMessageEventArgs>? MessageReceived;

        public MqttService()
        {
            InitializeMqttClient();
        }

        /// <summary>
        /// 정적 팩토리 메서드
        /// </summary>
        public static IMqttService CreateInstance()
        {
            return new MqttService();
        }

        ~MqttService()
        {
            Dispose(false);
        }

        /// <summary>
        /// MQTT 클라이언트 초기화
        /// </summary>
        private void InitializeMqttClient()
        {
            var factory = new MqttClientFactory();
            mqttClient = factory.CreateMqttClient();

            // 이벤트 구독
            mqttClient.ConnectedAsync += OnConnectedAsync;
            mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        /// <summary>
        /// MQTT 브로커에 연결
        /// </summary>
        public async Task<bool> ConnectAsync(BrokerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (!settings.IsValid(out string errorMessage))
                throw new ArgumentException($"잘못된 브로커 설정: {errorMessage}");

            try
            {
                currentSettings = settings;

                // MQTT 클라이언트 옵션 설정
                var clientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(settings.IP, int.Parse(settings.Port))
                    .WithClientId($"TagPass_{Environment.MachineName}_{Guid.NewGuid():N}")
                    .WithCleanSession(true)
                    .Build();

                var result = await mqttClient!.ConnectAsync(clientOptions);

                if (result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    // 기본 토픽 구독
                    await SubscribeAsync(settings.Topic);

                    StartReconnectTimer(); // 자동 재연결 타이머
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"MQTT 브로커 연결 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// MQTT 브로커 연결 해제
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                StopReconnectTimer();

                if (mqttClient != null && mqttClient.IsConnected)
                {
                    await mqttClient.DisconnectAsync();
                }
                currentSettings = null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"MQTT 브로커 연결 해제 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 메시지 발행
        /// </summary>
        public async Task PublishAsync(string topic, string payload, bool retain = false)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("토픽은 필수입니다.", nameof(topic));

            if (!IsConnected)
                throw new InvalidOperationException("MQTT 브로커에 연결되지 않았습니다.");

            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload ?? string.Empty)
                    .WithRetainFlag(retain)
                    .Build();

                await mqttClient!.PublishAsync(message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"메시지 발행 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 토픽 구독
        /// </summary>
        public async Task SubscribeAsync(string topic)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("토픽은 필수입니다.", nameof(topic));

            if (!IsConnected)
                throw new InvalidOperationException("MQTT 브로커에 연결되지 않았습니다.");

            try
            {
                var subscribeOptions = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .Build();

                await mqttClient!.SubscribeAsync(subscribeOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"토픽 구독 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 토픽 구독 해제
        /// </summary>
        public async Task UnsubscribeAsync(string topic)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("토픽은 필수입니다.", nameof(topic));

            if (!IsConnected)
                throw new InvalidOperationException("MQTT 브로커에 연결되지 않았습니다.");

            try
            {
                await mqttClient!.UnsubscribeAsync(topic);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"토픽 구독 해제 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 자동 재연결 타이머 시작
        /// </summary>
        private void StartReconnectTimer()
        {
            reconnectTimer = new Timer(async _ => await TryReconnectAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// 자동 재연결 타이머 정지
        /// </summary>
        private void StopReconnectTimer()
        {
            reconnectTimer?.Dispose();
            reconnectTimer = null;
        }

        /// <summary>
        /// 재연결 시도
        /// </summary>
        private async Task TryReconnectAsync()
        {
            if (currentSettings == null || IsConnected)
                return;

            try
            {
                await ConnectAsync(currentSettings);
            }
            catch
            {
                // 재연결 실패는 무시 (다음에 다시 시도)
            }
        }

        /// <summary>
        /// 연결됨 이벤트 처리
        /// </summary>
        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this, true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 연결 해제됨 이벤트 처리
        /// </summary>
        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this, false);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 메시지 수신 이벤트 처리
        /// </summary>
        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var payload = e.ApplicationMessage.ConvertPayloadToString();

                var messageArgs = new MqttMessageEventArgs
                {
                    Topic = e.ApplicationMessage.Topic,
                    Payload = payload ?? string.Empty,
                    Retain = e.ApplicationMessage.Retain,
                    Timestamp = DateTime.Now
                };

                MessageReceived?.Invoke(this, messageArgs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MQTT 메시지 처리 중 오류: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                StopReconnectTimer();

                if (mqttClient != null)
                {
                    if (mqttClient.IsConnected)
                    {
                        mqttClient.DisconnectAsync().Wait(TimeSpan.FromSeconds(5));
                    }
                    mqttClient.Dispose();
                }
                disposed = true;
            }
        }

    }
}