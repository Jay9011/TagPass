using TagPass.Models;

namespace TagPass.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// 기본 설정 생성
        /// </summary>
        ApplicationSettings CreateDefaultSettings();

        /// <summary>
        /// 설정 로드
        /// </summary>
        Task<ApplicationSettings> LoadSettingsAsync();

        /// <summary>
        /// 설정 저장
        /// </summary>
        Task SaveSettingsAsync(ApplicationSettings settings);

        /// <summary>
        /// 기본값으로 복원
        /// </summary>
        Task ResetToDefaultsAsync();

        /// <summary>
        /// 설정 유효성 검사
        /// </summary>
        bool ValidateSettings(ApplicationSettings settings, out string errorMessage);
    }
}