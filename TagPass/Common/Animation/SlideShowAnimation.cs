using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;

namespace TagPass.Common.Animation
{
    /// <summary>
    /// 슬라이드쇼 스타일의 애니메이션
    /// </summary>
    public class SlideShowAnimation : AnimationManager
    {
        private static readonly TimeSpan DefaultShowDuration = TimeSpan.FromMilliseconds(800);
        private static readonly TimeSpan DefaultHideDuration = TimeSpan.FromMilliseconds(1000);
        private static readonly TimeSpan DefaultImmediateHideDuration = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// 등장 애니메이션 생성
        /// </summary>
        protected override Storyboard? CreateShowAnimationCore(FrameworkElement target, TimeSpan? duration)
        {
            var animationDuration = duration ?? DefaultShowDuration;
            return CreateShowAnimation(target, animationDuration);
        }

        /// <summary>
        /// 사라짐 애니메이션 생성
        /// </summary>
        protected override Storyboard? CreateHideAnimationCore(FrameworkElement target, TimeSpan? duration)
        {
            var animationDuration = duration ?? DefaultHideDuration;
            return CreateHideAnimation(target, animationDuration);
        }

        /// <summary>
        /// 등장 애니메이션 생성
        /// </summary>
        private Storyboard CreateShowAnimation(FrameworkElement target, TimeSpan duration)
        {
            var storyboard = CreateStoryboard();
            EnsureScaleTransform(target);   // ScaleTransform 확인

            // 왼쪽에서 중앙으로
            var leftAnimation = CreateDoubleAnimation(
                from: -400,
                to: 0,
                duration: duration,
                easingFunction: EasingFunctions.QuadraticEaseOut
            );
            AddAnimationToStoryboard(storyboard, leftAnimation, target, Canvas.LeftProperty);

            // 페이드 인
            var opacityAnimation = CreateDoubleAnimation(
                from: 0,
                to: 1,
                duration: duration
            );
            AddAnimationToStoryboard(storyboard, opacityAnimation, target, UIElement.OpacityProperty);

            // 스케일 증가
            AddScaleAnimation(storyboard, target, 0.8, 1, duration, EasingFunctions.BackEaseOut);

            return storyboard;
        }

        /// <summary>
        /// 사라짐 애니메이션 생성
        /// </summary>
        private Storyboard CreateHideAnimation(FrameworkElement target, TimeSpan duration)
        {
            var storyboard = CreateStoryboard();
            EnsureScaleTransform(target);   // ScaleTransform 확인

            // 중앙에서 오른쪽으로
            var leftAnimation = CreateDoubleAnimation(
                from: 0,
                to: 400,
                duration: duration,
                easingFunction: EasingFunctions.QuadraticEaseIn
            );
            AddAnimationToStoryboard(storyboard, leftAnimation, target, Canvas.LeftProperty);

            // 페이드 아웃
            var opacityAnimation = CreateDoubleAnimation(
                from: 1,
                to: 0,
                duration: duration
            );
            AddAnimationToStoryboard(storyboard, opacityAnimation, target, UIElement.OpacityProperty);

            // 스케일 감소
            AddScaleAnimation(storyboard, target, 1, 0.8, duration, EasingFunctions.QuadraticEaseIn);

            return storyboard;
        }

        /// <summary>
        /// 타겟 요소에 ScaleTransform 설정 확인 및 추가
        /// </summary>
        private void EnsureScaleTransform(FrameworkElement target)
        {
            if (target.RenderTransform == null || target.RenderTransform == Transform.Identity)
            {
                target.RenderTransform = new ScaleTransform(1, 1);
                target.RenderTransformOrigin = new Point(0.5, 0.5);
            }
            else if (!(target.RenderTransform is ScaleTransform))
            {
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(target.RenderTransform);
                transformGroup.Children.Add(new ScaleTransform(1, 1));
                target.RenderTransform = transformGroup;
                target.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        /// <summary>
        /// Scale 애니메이션 추가
        /// </summary>
        private void AddScaleAnimation(Storyboard storyboard, FrameworkElement target, double from, double to, TimeSpan duration, IEasingFunction easingFunction)
        {
            string scaleXPath, scaleYPath;

            if (target.RenderTransform is TransformGroup transformGroup)
            {
                for (int i = 0; i < transformGroup.Children.Count; i++)
                {
                    if (transformGroup.Children[i] is ScaleTransform)
                    {
                        scaleXPath = $"(UIElement.RenderTransform).(TransformGroup.Children)[{i}].(ScaleTransform.ScaleX)";
                        scaleYPath = $"(UIElement.RenderTransform).(TransformGroup.Children)[{i}].(ScaleTransform.ScaleY)";

                        var scaleXAnimation = CreateDoubleAnimation(from, to, duration, easingFunction);
                        var scaleYAnimation = CreateDoubleAnimation(from, to, duration, easingFunction);

                        AddAnimationToStoryboard(storyboard, scaleXAnimation, target, new PropertyPath(scaleXPath));
                        AddAnimationToStoryboard(storyboard, scaleYAnimation, target, new PropertyPath(scaleYPath));
                        return;
                    }
                }
            }
            else if (target.RenderTransform is ScaleTransform)
            {
                scaleXPath = "(UIElement.RenderTransform).(ScaleTransform.ScaleX)";
                scaleYPath = "(UIElement.RenderTransform).(ScaleTransform.ScaleY)";

                var scaleXAnimation = CreateDoubleAnimation(from, to, duration, easingFunction);
                var scaleYAnimation = CreateDoubleAnimation(from, to, duration, easingFunction);

                AddAnimationToStoryboard(storyboard, scaleXAnimation, target, new PropertyPath(scaleXPath));
                AddAnimationToStoryboard(storyboard, scaleYAnimation, target, new PropertyPath(scaleYPath));
            }
        }
    }
}
