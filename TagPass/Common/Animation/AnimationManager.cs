using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace TagPass.Common.Animation
{
    /// <summary>
    /// 애니메이션 관리 기본 클래스
    /// </summary>
    public abstract class AnimationManager : IAnimation
    {
        private readonly Dictionary<WeakReference, Storyboard> _runningAnimations;
        private readonly Dictionary<Storyboard, WeakReference> _storyboardToTarget;
        private bool _disposed = false;

        protected AnimationManager()
        {
            _runningAnimations = new Dictionary<WeakReference, Storyboard>();
            _storyboardToTarget = new Dictionary<Storyboard, WeakReference>();
        }

        /// <summary>
        /// 등장 애니메이션 생성
        /// </summary>
        protected abstract Storyboard? CreateShowAnimationCore(FrameworkElement target, TimeSpan? duration);

        /// <summary>
        /// 사라짐 애니메이션 생성
        /// </summary>
        protected abstract Storyboard? CreateHideAnimationCore(FrameworkElement target, TimeSpan? duration);

        /// <summary>
        /// 스토리보드 생성
        /// </summary>
        protected Storyboard CreateStoryboard()
        {
            return new Storyboard();
        }

        /// <summary>
        /// 애니메이션 리소스 가져오기
        /// </summary>
        protected virtual Storyboard? GetAnimationResource(string resourceKey)
        {
            try
            {
                // Application 리소스에서 먼저 찾기
                if (Application.Current.TryFindResource(resourceKey) is Storyboard storyboard)
                {
                    return storyboard;
                }

                // 현재 윈도우 리소스에서 찾기
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow?.TryFindResource(resourceKey) is Storyboard windowStoryboard)
                {
                    return windowStoryboard;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"애니메이션 리소스 로드 실패: {resourceKey}, {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Double 애니메이션 생성
        /// </summary>
        protected DoubleAnimation CreateDoubleAnimation(double from, double to, TimeSpan duration, IEasingFunction? easingFunction = null, TimeSpan? beginTime = null)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration
            };

            if (easingFunction != null)
                animation.EasingFunction = easingFunction;

            if (beginTime.HasValue)
                animation.BeginTime = beginTime.Value;

            return animation;
        }

        /// <summary>
        /// 애니메이션을 스토리보드에 추가하고 타겟 설정
        /// </summary>
        protected void AddAnimationToStoryboard(Storyboard storyboard, AnimationTimeline animation, FrameworkElement target, DependencyProperty targetProperty)
        {
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(targetProperty));
        }

        /// <summary>
        /// PropertyPath를 사용한 애니메이션 추가 오버로드
        /// </summary>
        protected void AddAnimationToStoryboard(Storyboard storyboard, AnimationTimeline animation, FrameworkElement target, PropertyPath propertyPath)
        {
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, propertyPath);
        }

        /// <summary>
        /// 공통 EasingFunction들
        /// </summary>
        protected static class EasingFunctions
        {
            public static readonly QuadraticEase QuadraticEaseOut = new() { EasingMode = EasingMode.EaseOut };
            public static readonly QuadraticEase QuadraticEaseIn = new() { EasingMode = EasingMode.EaseIn };
            public static readonly BackEase BackEaseOut = new() { EasingMode = EasingMode.EaseOut };
            public static readonly BackEase BackEaseIn = new() { EasingMode = EasingMode.EaseIn };
            public static readonly CubicEase CubicEaseOut = new() { EasingMode = EasingMode.EaseOut };
            public static readonly ElasticEase ElasticEaseOut = new() { EasingMode = EasingMode.EaseOut };
        }

        #region IAnimation

        /// <summary>
        /// 요소를 등장시키는 애니메이션
        /// </summary>
        public void ShowElement(FrameworkElement target, TimeSpan? duration = null, Action? completed = null)
        {
            if (target == null || _disposed) return;

            StopAnimation(target);
            CleanupDeadReferences();

            var storyboard = CreateShowAnimationCore(target, duration);
            if (storyboard != null)
            {
                ExecuteAnimation(target, storyboard, completed);
            }
        }

        /// <summary>
        /// 요소를 사라지게 하는 애니메이션
        /// </summary>
        public void HideElement(FrameworkElement target, TimeSpan? duration = null, Action? completed = null)
        {
            if (target == null || _disposed) return;

            StopAnimation(target);
            CleanupDeadReferences();

            var storyboard = CreateHideAnimationCore(target, duration);
            if (storyboard != null)
            {
                ExecuteAnimation(target, storyboard, completed);
            }
        }

        /// <summary>
        /// 현재 실행 중인 모든 애니메이션 중지
        /// </summary>
        public void StopAnimation(FrameworkElement target)
        {
            if (target == null || _disposed) return;

            var targetRef = FindWeakReference(target);
            if (targetRef != null && _runningAnimations.TryGetValue(targetRef, out var storyboard))
            {
                try
                {
                    storyboard.Stop();
                    storyboard.Remove();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"애니메이션 중지 중 오류: {ex.Message}");
                }
                finally
                {
                    CleanupStoryboard(storyboard);
                }
            }
        }

        /// <summary>
        /// 애니메이션 실행 여부 확인
        /// </summary>
        public bool IsAnimating(FrameworkElement target)
        {
            if (target == null || _disposed) return false;

            CleanupDeadReferences();
            var targetRef = FindWeakReference(target);
            return targetRef != null && _runningAnimations.ContainsKey(targetRef);
        }

        #endregion

        #region 메모리 관련 기능

        /// <summary>
        /// 애니메이션 실행 공통 로직
        /// </summary>
        private void ExecuteAnimation(FrameworkElement target, Storyboard storyboard, Action? completed)
        {
            var targetRef = new WeakReference(target);

            EventHandler completedHandler = null;
            completedHandler = (s, e) =>
            {
                try
                {
                    if (s is Storyboard sb)
                    {
                        sb.Completed -= completedHandler;
                        CleanupStoryboard(sb);
                    }
                    completed?.Invoke();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"애니메이션 완료 처리 중 오류: {ex.Message}");
                }
            };

            storyboard.Completed += completedHandler;
            _runningAnimations[targetRef] = storyboard;
            _storyboardToTarget[storyboard] = targetRef;

            try
            {
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                CleanupStoryboard(storyboard);
                System.Diagnostics.Debug.WriteLine($"애니메이션 시작 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 대상 요소의 WeakReference 찾기
        /// </summary>
        private WeakReference? FindWeakReference(FrameworkElement target)
        {
            return _runningAnimations.Keys.FirstOrDefault(wr => wr.Target == target);
        }

        /// <summary>
        /// 죽은 참조들 정리 (GC된 객체들의 WeakReference 제거)
        /// </summary>
        internal void CleanupDeadReferences()
        {
            var deadRefs = _runningAnimations.Keys.Where(wr => !wr.IsAlive).ToList();
            foreach (var deadRef in deadRefs)
            {
                if (_runningAnimations.TryGetValue(deadRef, out var storyboard))
                {
                    CleanupStoryboard(storyboard);
                }
            }
        }

        /// <summary>
        /// 스토리보드와 관련된 모든 참조 정리
        /// </summary>
        private void CleanupStoryboard(Storyboard storyboard)
        {
            try
            {
                if (_storyboardToTarget.TryGetValue(storyboard, out var targetRef))
                {
                    _runningAnimations.Remove(targetRef);
                    _storyboardToTarget.Remove(storyboard);
                }

                storyboard.Remove();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"스토리보드 정리 중 오류: {ex.Message}");
            }
        }

        #endregion

        #region IDisposable 구현

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // 모든 실행 중인 애니메이션 정리
                        foreach (var kvp in _runningAnimations.ToList())
                        {
                            try
                            {
                                kvp.Value.Stop();
                                kvp.Value.Remove();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"애니메이션 정리 중 오류: {ex.Message}");
                            }
                        }
                        _runningAnimations.Clear();
                        _storyboardToTarget.Clear();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"리소스 정리 중 전체 오류: {ex.Message}");
                    }
                }
                _disposed = true;
            }
        }

        ~AnimationManager()
        {
            Dispose(false);
        }

        #endregion
    }
}