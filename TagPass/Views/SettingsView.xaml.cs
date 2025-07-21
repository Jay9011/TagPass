using System.Windows;
using System.Windows.Controls;

namespace TagPass.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        // 설정 적용 이벤트
        public static readonly RoutedEvent ApplySettingsEvent =
            EventManager.RegisterRoutedEvent("ApplySettings", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsView));

        public event RoutedEventHandler ApplySettings
        {
            add { AddHandler(ApplySettingsEvent, value); }
            remove { RemoveHandler(ApplySettingsEvent, value); }
        }

        // 기본값 복원 이벤트
        public static readonly RoutedEvent ResetToDefaultsEvent =
            EventManager.RegisterRoutedEvent("ResetToDefaults", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SettingsView));

        public event RoutedEventHandler ResetToDefaults
        {
            add { AddHandler(ResetToDefaultsEvent, value); }
            remove { RemoveHandler(ResetToDefaultsEvent, value); }
        }

        public void OnApplySettings()
        {
            RaiseEvent(new RoutedEventArgs(ApplySettingsEvent, this));
        }

        public void OnResetToDefaults()
        {
            RaiseEvent(new RoutedEventArgs(ResetToDefaultsEvent, this));
        }
    }
}