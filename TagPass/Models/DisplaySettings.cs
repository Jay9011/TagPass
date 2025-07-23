using FileIOHelper;
using TagPass.Common;
using System.ComponentModel;

namespace TagPass.Models
{
    /// <summary>
    /// 화면 관련 설정 모델
    /// </summary>
    public class DisplaySettings : INotifyPropertyChanged
    {
        private bool _startFullscreen = true;
        private bool _hideTitleBar = true;

        public bool StartFullscreen
        {
            get => _startFullscreen;
            set
            {
                _startFullscreen = value;
                OnPropertyChanged(nameof(StartFullscreen));
            }
        }

        /// <summary>
        /// 타이틀 바 숨김
        /// </summary>
        public bool HideTitleBar
        {
            get => _hideTitleBar;
            set
            {
                _hideTitleBar = value;
                OnPropertyChanged(nameof(HideTitleBar));
            }
        }

        public DisplaySettings()
        {
        }

        public DisplaySettings(bool startFullscreen, bool hideTitleBar)
        {
            StartFullscreen = startFullscreen;
            HideTitleBar = hideTitleBar;
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// INI 파일에서 디스플레이 설정 로드
        /// </summary>
        public static DisplaySettings LoadFromIni(IIOHelper iniHelper)
        {
            string startFullscreen = iniHelper.ReadValue(StringClass.Display, StringClass.StartFullscreen);
            string hideTitleBar = iniHelper.ReadValue(StringClass.Display, StringClass.HideTitleBar);

            bool startFullscreenValue = string.IsNullOrEmpty(startFullscreen) ? true : bool.TryParse(startFullscreen, out bool sf) && sf;
            bool hideTitleBarValue = string.IsNullOrEmpty(hideTitleBar) ? true : bool.TryParse(hideTitleBar, out bool htb) && htb;

            return new DisplaySettings(startFullscreenValue, hideTitleBarValue);
        }

        /// <summary>
        /// INI 파일에 디스플레이 설정 저장
        /// </summary>
        public void SaveToIni(IIOHelper iniHelper)
        {
            iniHelper.WriteValue(StringClass.Display, StringClass.StartFullscreen, StartFullscreen.ToString());
            iniHelper.WriteValue(StringClass.Display, StringClass.HideTitleBar, HideTitleBar.ToString());
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