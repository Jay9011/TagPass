using System.Windows;
using System.Windows.Controls;

namespace TagPass.Common.AttachedProperty
{
    /// <summary>
    /// TextBox 자동 스크롤을 위한 Attached Property
    /// </summary>
    public static class ScrollBehavior
    {
        #region AutoScrollToEnd 속성

        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",      // 속성명
                typeof(bool),           // 속성 타입
                typeof(ScrollBehavior), // 소유자 타입
                new PropertyMetadata(false, OnAutoScrollToEndChanged));

        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToEndProperty);
        }

        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        private static void OnAutoScrollToEndChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                }
                else
                {
                    textBox.TextChanged -= TextBox_TextChanged;
                }
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && GetAutoScrollToEnd(textBox))
            {
                textBox.ScrollToEnd();
            }
        }

        #endregion

        #region ScrollToEndTrigger 속성 (바인딩용)

        public static readonly DependencyProperty ScrollToEndTriggerProperty =
            DependencyProperty.RegisterAttached(
                "ScrollToEndTrigger",
                typeof(bool),
                typeof(ScrollBehavior),
                new PropertyMetadata(false, OnScrollToEndTriggerChanged));

        public static bool GetScrollToEndTrigger(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToEndTriggerProperty);
        }

        public static void SetScrollToEndTrigger(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToEndTriggerProperty, value);
        }

        private static void OnScrollToEndTriggerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TextBox textBox && (bool)e.NewValue)
            {
                textBox.Dispatcher.BeginInvoke(() =>
                {
                    textBox.ScrollToEnd();
                    SetScrollToEndTrigger(textBox, false);  // 트리거 리셋 (다음 호출을 위해)
                });
            }
        }

        #endregion
    }
}