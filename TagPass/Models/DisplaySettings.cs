using FileIOHelper;
using TagPass.Common;

namespace TagPass.Models
{
    /// <summary>
    /// 화면 관련 설정
    /// </summary>
    public class DisplaySettings
    {
        public bool StartFullscreen { get; set; } = true;
        public bool HideTitleBar { get; set; } = true;

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
            // 화면 설정은 현재 특별한 검증이 필요 없음
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
    }
}