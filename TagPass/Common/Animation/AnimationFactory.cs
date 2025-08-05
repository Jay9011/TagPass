using System.Windows;
using System.Collections.Generic;
using System;

namespace TagPass.Common.Animation
{
    /// <summary>
    /// 애니메이션 생성 및 관리를 위한 팩토리 클래스
    /// </summary>
    public static class AnimationFactory
    {
        private static readonly Dictionary<string, Func<IAnimation>> _animationFactories;
        private static readonly Dictionary<string, IAnimation> _animationInstances;
        private static DateTime _lastCleanup = DateTime.Now;
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(10);

        static AnimationFactory()
        {
            _animationFactories = new Dictionary<string, Func<IAnimation>>();
            _animationInstances = new Dictionary<string, IAnimation>();

            // 기본 애니메이션 등록
            RegisterAnimation(Animations.SlideCard, () => new SlideShowAnimation());
        }

        #region 기본 애니메이션들

        /// <summary>
        /// 기본 슬라이드쇼 애니메이션
        /// </summary>
        public static IAnimation SlideShow => GetAnimation(Animations.SlideCard)!;

        #endregion

        /// <summary>
        /// 새로운 애니메이션 객체 등록
        /// </summary>
        /// <param name="name">애니메이션 객체 이름</param>
        /// <param name="factory">애니메이션 생성 팩토리</param>
        public static void RegisterAnimation(string name, Func<IAnimation> factory)
        {
            if (string.IsNullOrWhiteSpace(name) || factory == null)
                return;

            _animationFactories[name] = factory;
        }

        /// <summary>
        /// 애니메이션 인스턴스 가져오기
        /// </summary>
        /// <param name="name">애니메이션 이름</param>
        /// <returns>애니메이션 인스턴스</returns>
        public static IAnimation? GetAnimation(string name)
        {
            TryAutoCleanup();

            if (!_animationInstances.TryGetValue(name, out var instance))
            {
                if (_animationFactories.TryGetValue(name, out var factory))
                {
                    instance = factory();
                    _animationInstances[name] = instance;
                }
            }
            return instance;
        }

        /// <summary>
        /// 주기적 자동 메모리 정리
        /// </summary>
        private static void TryAutoCleanup()
        {
            if (DateTime.Now - _lastCleanup > CleanupInterval)
            {
                PerformMaintenanceCleanup();
                _lastCleanup = DateTime.Now;
            }
        }

        /// <summary>
        /// 유지보수용 메모리 정리 (전체 캐시 초기화하지 않고 내부 정리만)
        /// </summary>
        public static void PerformMaintenanceCleanup()
        {
            try
            {
                // 각 애니메이션 인스턴스의 내부 정리만 수행
                foreach (var animation in _animationInstances.Values)
                {
                    if (animation is AnimationManager animationManager)
                    {
                        animationManager.CleanupDeadReferences();
                    }
                }

                // 강제 가비지 컬렉션
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"자동 정리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 메모리 정리 (캐싱된 애니메이션 인스턴스들 해제)
        /// </summary>
        public static void ClearCache()
        {
            try
            {
                foreach (var animation in _animationInstances.Values)
                {
                    try
                    {
                        animation?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"애니메이션 Dispose 중 오류: {ex.Message}");
                    }
                }
                _animationInstances.Clear();
                _lastCleanup = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"캐시 정리 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 메모리 사용량 정보 가져오기 (디버깅용)
        /// </summary>
        public static string GetMemoryInfo()
        {
            var totalMemory = GC.GetTotalMemory(false);
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            return $"총 메모리: {totalMemory / 1024 / 1024:F2}MB, " +
                   $"GC 수행: Gen0={gen0}, Gen1={gen1}, Gen2={gen2}, " +
                   $"캐시된 애니메이션: {_animationInstances.Count}개";
        }
    }
}