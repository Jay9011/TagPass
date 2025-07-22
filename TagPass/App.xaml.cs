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

            RegistIniSettingsFile();
            RegistSettingsService();
        }

        /// <summary>
        /// iniFileHelper 생성
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
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
    }
}
