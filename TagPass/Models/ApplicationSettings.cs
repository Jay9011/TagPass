using FileIOHelper;

namespace TagPass.Models
{
    /// <summary>
    /// 애플리케이션 전체 설정을 담는 통합 모델
    /// </summary>
    public class ApplicationSettings
    {
        public BrokerSettings Broker { get; set; }
        public DisplaySettings Display { get; set; }
        public GeneralSettings General { get; set; }

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
    }
}