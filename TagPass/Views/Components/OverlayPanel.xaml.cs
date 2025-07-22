using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TagPass.Views.Components
{
    /// <summary>
    /// 오버레이 패널 컴포넌트
    /// </summary>
    public partial class OverlayPanel : UserControl
    {
        public OverlayPanel()
        {
            InitializeComponent();
        }

        // 제목
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(OverlayPanel), new PropertyMetadata("제목"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // 패널 콘텐츠
        public static readonly DependencyProperty PanelContentProperty =
            DependencyProperty.Register("PanelContent", typeof(object), typeof(OverlayPanel), new PropertyMetadata(null));

        public object PanelContent
        {
            get { return GetValue(PanelContentProperty); }
            set { SetValue(PanelContentProperty, value); }
        }

        // 버튼 콘텐츠 속성
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(object), typeof(OverlayPanel), new PropertyMetadata(null));

        public object ButtonContent
        {
            get { return GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        // 버튼 표시 여부
        public static readonly DependencyProperty ShowButtonsProperty =
            DependencyProperty.Register("ShowButtons", typeof(bool), typeof(OverlayPanel), new PropertyMetadata(false));

        public bool ShowButtons
        {
            get { return (bool)GetValue(ShowButtonsProperty); }
            set { SetValue(ShowButtonsProperty, value); }
        }

        // 닫기 요청 이벤트
        public static readonly RoutedEvent CloseRequestedEvent =
            EventManager.RegisterRoutedEvent("CloseRequested", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OverlayPanel));

        public event RoutedEventHandler CloseRequested
        {
            add { AddHandler(CloseRequestedEvent, value); }
            remove { RemoveHandler(CloseRequestedEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (UseAutoSizing)
            {
                this.Loaded += (s, e) => UpdateAutoSize();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseRequestedEvent, this));
        }

        // 배경 클릭 이벤트
        private void OverlayBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 배경 클릭시 닫기
            if (e.Source == OverlayBackground)
            {
                RaiseEvent(new RoutedEventArgs(CloseRequestedEvent, this));
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAutoSize();
        }

        #region 패널 크기 관련

        // 패널 너비 속성
        public static readonly DependencyProperty PanelWidthProperty =
            DependencyProperty.Register("PanelWidth", typeof(double), typeof(OverlayPanel), new PropertyMetadata(500.0));

        // 패널 너비
        public double PanelWidth
        {
            get { return (double)GetValue(PanelWidthProperty); }
            set { SetValue(PanelWidthProperty, value); }
        }

        // 패널 높이 속성
        public static readonly DependencyProperty PanelHeightProperty =
            DependencyProperty.Register("PanelHeight", typeof(double), typeof(OverlayPanel), new PropertyMetadata(600.0));

        public double PanelHeight
        {
            get { return (double)GetValue(PanelHeightProperty); }
            set { SetValue(PanelHeightProperty, value); }
        }

        // 자동 크기 조정 사용 여부
        public static readonly DependencyProperty UseAutoSizingProperty =
            DependencyProperty.Register("UseAutoSizing", typeof(bool), typeof(OverlayPanel), new PropertyMetadata(false, OnAutoSizingChanged));

        public bool UseAutoSizing
        {
            get { return (bool)GetValue(UseAutoSizingProperty); }
            set { SetValue(UseAutoSizingProperty, value); }
        }

        // 자동 크기 조정 - 너비 비율 (0.0 ~ 1.0)
        public static readonly DependencyProperty AutoWidthRatioProperty =
            DependencyProperty.Register("AutoWidthRatio", typeof(double), typeof(OverlayPanel), new PropertyMetadata(0.8));

        public double AutoWidthRatio
        {
            get { return (double)GetValue(AutoWidthRatioProperty); }
            set { SetValue(AutoWidthRatioProperty, value); }
        }

        // 자동 크기 조정 - 높이 비율 (0.0 ~ 1.0)
        public static readonly DependencyProperty AutoHeightRatioProperty =
            DependencyProperty.Register("AutoHeightRatio", typeof(double), typeof(OverlayPanel), new PropertyMetadata(0.8));

        public double AutoHeightRatio
        {
            get { return (double)GetValue(AutoHeightRatioProperty); }
            set { SetValue(AutoHeightRatioProperty, value); }
        }

        /// <summary>
        /// 자동 크기 조정 변경 이벤트
        /// </summary>
        /// <param name="d">의존성 객체</param>
        /// <param name="e">속성 변경 이벤트 인자</param>
        private static void OnAutoSizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OverlayPanel panel && (bool)e.NewValue)
            {
                panel.UpdateAutoSize();
            }
        }

        /// <summary>
        /// 비율에 맞춰 윈도우 사이즈 자동 조절
        /// </summary>
        private void UpdateAutoSize()
        {
            if (!UseAutoSizing) return;

            // 부모 윈도우 찾기
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var dpi = VisualTreeHelper.GetDpi(window);
                double dpiScaleX = dpi.DpiScaleX;
                double dpiScaleY = dpi.DpiScaleY;

                // 논리적 윈도우 크기
                double logicalWidht = window.ActualWidth;
                double logicalHeight = window.ActualHeight;

                double targetWidth = logicalWidht * AutoWidthRatio;
                double targetHeight = logicalHeight * AutoHeightRatio;

                double physicalTargetWidth = Math.Round(targetWidth * dpiScaleX);
                double physicalTargetHeight = Math.Round(targetHeight * dpiScaleY);

                PanelWidth = Math.Round(physicalTargetWidth / dpiScaleX);
                PanelHeight = Math.Round(physicalTargetHeight / dpiScaleY);

                // 윈도우 크기 변경 이벤트 구독 (한 번만)
                window.SizeChanged -= Window_SizeChanged;
                window.SizeChanged += Window_SizeChanged;
            }
        }

        #endregion

    }
}