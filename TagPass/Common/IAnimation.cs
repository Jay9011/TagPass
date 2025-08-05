using System;
using System.Windows;

namespace TagPass.Common
{
    /// <summary>
    /// 애니메이션 기본 인터페이스
    /// </summary>
    public interface IAnimation : IDisposable
    {
        /// <summary>
        /// 요소를 등장시키는 애니메이션
        /// </summary>
        /// <param name="target">애니메이션을 적용할 UI 요소</param>
        /// <param name="duration">애니메이션 지속 시간</param>
        /// <param name="completed">애니메이션 완료 시 실행할 콜백</param>
        void ShowElement(FrameworkElement target, TimeSpan? duration = null, Action? completed = null);

        /// <summary>
        /// 요소를 사라지게 하는 애니메이션
        /// </summary>
        /// <param name="target">애니메이션을 적용할 UI 요소</param>
        /// <param name="duration">애니메이션 지속 시간</param>
        /// <param name="completed">애니메이션 완료 시 실행할 콜백</param>
        void HideElement(FrameworkElement target, TimeSpan? duration = null, Action? completed = null);

        /// <summary>
        /// 현재 실행 중인 모든 애니메이션 중지
        /// </summary>
        /// <param name="target">중지할 UI 요소</param>
        void StopAnimation(FrameworkElement target);

        /// <summary>
        /// 애니메이션 실행 여부 확인
        /// </summary>
        /// <param name="target">확인할 UI 요소</param>
        /// <returns>애니메이션 실행 중이면 true</returns>
        bool IsAnimating(FrameworkElement target);
    }
}
