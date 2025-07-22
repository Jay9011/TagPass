using FileIOHelper;
using System.IO;
using System.Net;
using TagPass.Common;
using TagPass.Models;

namespace TagPass.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IIOHelper iniFile;

        public SettingsService(IIOHelper iniHelper)
        {
            iniFile = iniHelper ?? throw new ArgumentNullException(nameof(iniHelper));
        }

        /// <summary>
        /// 정적 팩토리 메서드
        /// </summary>
        public static ISettingsService CreateInstance(IIOHelper iniHelper)
        {
            return new SettingsService(iniHelper);
        }

        /// <summary>
        /// 기본 값으로 ApplicationSettings 생성
        /// </summary>
        public ApplicationSettings CreateDefaultSettings()
        {
            return new ApplicationSettings();
        }

        /// <summary>
        /// 설정 불러오기
        /// </summary>
        public async Task<ApplicationSettings> LoadSettingsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!iniFile.IsExists())
                    {
                        return CreateDefaultSettings();
                    }

                    return ApplicationSettings.LoadFromIni(iniFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"설정 로드 중 오류가 발생했습니다: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public async Task SaveSettingsAsync(ApplicationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (!ValidateSettings(settings, out string errorMessage))
                throw new ArgumentException(errorMessage);

            await Task.Run(() =>
            {
                try
                {
                    settings.SaveToIni(iniFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"설정 저장 중 오류가 발생했습니다: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// 설정 기본 값으로 복원
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            var defaultSettings = CreateDefaultSettings();
            await SaveSettingsAsync(defaultSettings);
        }

        /// <summary>
        /// 설정 유효성 검사
        /// </summary>
        /// <param name="settings">전체 설정 통합 모델</param>
        /// <param name="errorMessage">에러 메시지</param>
        /// <returns></returns>
        public bool ValidateSettings(ApplicationSettings settings, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (settings == null)
            {
                errorMessage = "설정 정보가 null입니다.";
                return false;
            }

            return settings.IsValid(out errorMessage);
        }
    }
}