using TagPass.Models;

namespace TagPass.Services
{
    public interface IMqttService
    {
        /// <summary>
        /// MQTT 연결 상태
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 현재 브로커 설정
        /// </summary>
        BrokerSettings? CurrentSettings { get; }

        /// <summary>
        /// MQTT 브로커에 연결
        /// </summary>
        Task<bool> ConnectAsync(BrokerSettings settings);

        /// <summary>
        /// MQTT 브로커 연결 해제
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 메시지 발행
        /// </summary>
        Task PublishAsync(string topic, string payload, bool retain = false);

        /// <summary>
        /// 토픽 구독
        /// </summary>
        Task SubscribeAsync(string topic);

        /// <summary>
        /// 토픽 구독 해제
        /// </summary>
        Task UnsubscribeAsync(string topic);
        
        #region 이벤트

        /// <summary>
        /// 연결 상태 변경 이벤트
        /// </summary>
        event EventHandler<bool> ConnectionStatusChanged;

        /// <summary>
        /// 메시지 수신 이벤트
        /// </summary>
        event EventHandler<MqttMessageEventArgs> MessageReceived; 
        
        #endregion
    }

    /// <summary>
    /// MQTT 메시지 이벤트 인자
    /// </summary>
    public class MqttMessageEventArgs : EventArgs
    {
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool Retain { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}