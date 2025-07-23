using FileIOHelper;
using TagPass.Common;
using System.ComponentModel;

namespace TagPass.Models
{
    /// <summary>
    /// 일반 애플리케이션 설정 모델
    /// </summary>
    public class GeneralSettings : INotifyPropertyChanged
    {
        private bool _registerStartupProgram = false;
        private bool _autoCheckUpdates = true;
        private bool _sendUsageStatistics = false;

        public bool RegisterStartupProgram
        {
            get => _registerStartupProgram;
            set
            {
                _registerStartupProgram = value;
                OnPropertyChanged(nameof(RegisterStartupProgram));
            }
        }

        public bool AutoCheckUpdates
        {
            get => _autoCheckUpdates;
            set
            {
                _autoCheckUpdates = value;
                OnPropertyChanged(nameof(AutoCheckUpdates));
            }
        }

        public bool SendUsageStatistics
        {
            get => _sendUsageStatistics;
            set
            {
                _sendUsageStatistics = value;
                OnPropertyChanged(nameof(SendUsageStatistics));
            }
        }

        public GeneralSettings()
        {
        }

        public GeneralSettings(bool registerStartup, bool autoUpdates, bool usageStats)
        {
            RegisterStartupProgram = registerStartup;
            AutoCheckUpdates = autoUpdates;
            SendUsageStatistics = usageStats;
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;
            // 일반 설정은 현재 특별한 검증이 필요 없음
            return true;
        }

        /// <summary>
        /// INI 파일에서 일반 설정 로드
        /// </summary>
        public static GeneralSettings LoadFromIni(IIOHelper iniHelper)
        {
            string registerStartup = iniHelper.ReadValue(StringClass.General, StringClass.RegisterStartupProgram);
            string autoUpdates = iniHelper.ReadValue(StringClass.General, StringClass.AutoCheckUpdates);
            string usageStats = iniHelper.ReadValue(StringClass.General, StringClass.SendUsageStatistics);

            bool registerStartupValue = !string.IsNullOrEmpty(registerStartup) && bool.TryParse(registerStartup, out bool rs) && rs;
            bool autoUpdatesValue = string.IsNullOrEmpty(autoUpdates) ? true : bool.TryParse(autoUpdates, out bool au) && au;
            bool usageStatsValue = !string.IsNullOrEmpty(usageStats) && bool.TryParse(usageStats, out bool us) && us;

            return new GeneralSettings(registerStartupValue, autoUpdatesValue, usageStatsValue);
        }

        /// <summary>
        /// INI 파일에 일반 설정 저장
        /// </summary>
        public void SaveToIni(IIOHelper iniHelper)
        {
            iniHelper.WriteValue(StringClass.General, StringClass.RegisterStartupProgram, RegisterStartupProgram.ToString());
            iniHelper.WriteValue(StringClass.General, StringClass.AutoCheckUpdates, AutoCheckUpdates.ToString());
            iniHelper.WriteValue(StringClass.General, StringClass.SendUsageStatistics, SendUsageStatistics.ToString());
        }

        #region INotifyPropertyChanged 구현

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}