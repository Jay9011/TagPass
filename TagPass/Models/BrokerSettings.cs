using System.Net;
using FileIOHelper;
using TagPass.Common;
using System.ComponentModel;

namespace TagPass.Models
{
    /// <summary>
    /// 브로커 데이터 모델
    /// </summary>
    public class BrokerSettings : INotifyPropertyChanged
    {
        private string _ip = string.Empty;
        private string _port = string.Empty;
        private string _topic = string.Empty;

        public string IP
        {
            get => _ip;
            set
            {
                _ip = value;
                OnPropertyChanged(nameof(IP));
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public string Topic
        {
            get => _topic;
            set
            {
                _topic = value;
                OnPropertyChanged(nameof(Topic));
            }
        }

        public BrokerSettings()
        {
            IP = "localhost";
            Port = "1883";
            Topic = "S1ACCESS/Normal";
        }

        public BrokerSettings(string ip, string port, string topic)
        {
            IP = ip;
            Port = port;
            Topic = topic;
        }

        /// <summary>
        /// 브로커 설정 유효성 검사
        /// </summary>
        /// <param name="errorMessage">검사 실패시 오류 문자열</param>
        /// <returns></returns>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;

            // IP 검증
            if (string.IsNullOrWhiteSpace(IP))
            {
                errorMessage = "IP 주소를 입력해주세요.";
                return false;
            }

            if (!IPAddress.TryParse(IP, out _) && IP.ToLower() != "localhost")
            {
                errorMessage = "유효하지 않은 IP 주소입니다.";
                return false;
            }

            // Port 검증
            if (string.IsNullOrWhiteSpace(Port))
            {
                errorMessage = "포트 번호를 입력해주세요.";
                return false;
            }

            if (!int.TryParse(Port, out int port) || port < 1 || port > 65535)
            {
                errorMessage = "포트 번호는 1-65535 범위의 숫자여야 합니다.";
                return false;
            }

            // Topic 검증
            if (string.IsNullOrWhiteSpace(Topic))
            {
                errorMessage = "토픽을 입력해주세요.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// INI 파일에서 브로커 설정 로드
        /// </summary>
        public static BrokerSettings LoadFromIni(IIOHelper iniHelper)
        {
            string brokerIP = iniHelper.ReadValue(StringClass.Broker, StringClass.IP);
            string brokerPort = iniHelper.ReadValue(StringClass.Broker, StringClass.Port);
            string brokerTopic = iniHelper.ReadValue(StringClass.Broker, StringClass.Topic);

            return new BrokerSettings(
                string.IsNullOrEmpty(brokerIP) ? "localhost" : brokerIP,
                string.IsNullOrEmpty(brokerPort) ? "1883" : brokerPort,
                string.IsNullOrEmpty(brokerTopic) ? "S1ACCESS/Normal" : brokerTopic
            );
        }

        /// <summary>
        /// INI 파일에 브로커 설정 저장
        /// </summary>
        public void SaveToIni(IIOHelper iniHelper)
        {
            iniHelper.WriteValue(StringClass.Broker, StringClass.IP, IP);
            iniHelper.WriteValue(StringClass.Broker, StringClass.Port, Port);
            iniHelper.WriteValue(StringClass.Broker, StringClass.Topic, Topic);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}