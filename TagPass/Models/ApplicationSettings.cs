using FileIOHelper;
using System.ComponentModel;

namespace TagPass.Models
{
    /// <summary>
    /// 애플리케이션 전체 설정을 담는 통합 모델
    /// </summary>
    public class ApplicationSettings : INotifyPropertyChanged
    {
        private BrokerSettings _broker;
        private DisplaySettings _display;
        private GeneralSettings _general;

        public BrokerSettings Broker
        {
            get => _broker;
            set
            {
                if (_broker != null)
                    _broker.PropertyChanged -= OnSubObjectPropertyChanged;

                _broker = value;

                if (_broker != null)
                    _broker.PropertyChanged += OnSubObjectPropertyChanged;

                OnPropertyChanged(nameof(Broker));
            }
        }

        public DisplaySettings Display
        {
            get => _display;
            set
            {
                if (_display != null)
                    _display.PropertyChanged -= OnSubObjectPropertyChanged;

                _display = value;

                if (_display != null)
                    _display.PropertyChanged += OnSubObjectPropertyChanged;

                OnPropertyChanged(nameof(Display));
            }
        }

        public GeneralSettings General
        {
            get => _general;
            set
            {
                if (_general != null)
                    _general.PropertyChanged -= OnSubObjectPropertyChanged;

                _general = value;

                if (_general != null)
                    _general.PropertyChanged += OnSubObjectPropertyChanged;

                OnPropertyChanged(nameof(General));
            }
        }

        public ApplicationSettings()
        {
            Broker = new BrokerSettings();
            Display = new DisplaySettings();
            General = new GeneralSettings();
        }

        public ApplicationSettings(BrokerSettings broker, DisplaySettings display, GeneralSettings general)
        {
            Broker = broker ?? new BrokerSettings();
            Display = display ?? new DisplaySettings();
            General = general ?? new GeneralSettings();
        }

        /// <summary>
        /// 서브 객체의 PropertyChanged 이벤트 핸들러
        /// </summary>
        private void OnSubObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 서브 객체의 속성 변경을 상위로 전파
            OnPropertyChanged($"{sender.GetType().Name}.{e.PropertyName}");
        }

        /// <summary>
        /// 모든 설정 항목 유효성 검사
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (!Broker.IsValid(out errorMessage))
                return false;

            if (!Display.IsValid(out errorMessage))
                return false;

            if (!General.IsValid(out errorMessage))
                return false;

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// 기본값으로 초기화
        /// </summary>
        public void ResetToDefaults()
        {
            Broker = new BrokerSettings();
            Display = new DisplaySettings();
            General = new GeneralSettings();
        }

        /// <summary>
        /// INI 파일에서 모든 설정 로드
        /// </summary>
        public static ApplicationSettings LoadFromIni(IIOHelper iniHelper)
        {
            var brokerSettings = BrokerSettings.LoadFromIni(iniHelper);
            var displaySettings = DisplaySettings.LoadFromIni(iniHelper);
            var generalSettings = GeneralSettings.LoadFromIni(iniHelper);

            return new ApplicationSettings(brokerSettings, displaySettings, generalSettings);
        }

        /// <summary>
        /// INI 파일에 모든 설정 저장
        /// </summary>
        public void SaveToIni(IIOHelper iniHelper)
        {
            Broker.SaveToIni(iniHelper);
            Display.SaveToIni(iniHelper);
            General.SaveToIni(iniHelper);
        }

        #region INotifyPropertyChanged 구현

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}