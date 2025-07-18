using System.Windows;
using System.Windows.Controls;

namespace TagPass.Views.Components
{
    public partial class PasswordBoxWithToggle : UserControl
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordBoxWithToggle), new PropertyMetadata(string.Empty, OnPasswordChanged));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordBoxWithToggle)d;
            var newPassword = e.NewValue as string ?? string.Empty;
            
            if (control.PasswordBox.Password != newPassword)
            {
                control.PasswordBox.Password = newPassword;
            }
            
            if (control.TextBox.Text != newPassword)
            {
                control.TextBox.Text = newPassword;
            }
        }

        public PasswordBoxWithToggle()
        {
            InitializeComponent();
            
            // Focus 이벤트 처리
            PasswordBox.GotFocus += OnGotFocus;
            PasswordBox.LostFocus += OnLostFocus;
            TextBox.GotFocus += OnGotFocus;
            TextBox.LostFocus += OnLostFocus;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // PasswordBox 변경 시 TextBox와 동기화
            if (PasswordBox.Password != TextBox.Text)
            {
                TextBox.Text = PasswordBox.Password;
            }

            Password = PasswordBox.Password;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TextBox 변경 시 PasswordBox와 동기화
            if (TextBox.Text != PasswordBox.Password)
            {
                PasswordBox.Password = TextBox.Text;
            }

            Password = TextBox.Text;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = ToggleButton.IsChecked == true;
            
            if (isChecked)
            {
                // 비밀번호 보이기
                PasswordBox.Visibility = Visibility.Collapsed;
                TextBox.Visibility = Visibility.Visible;
                TextBox.Text = PasswordBox.Password;
                TextBox.Focus();
            }
            else
            {
                // 비밀번호 숨기기
                TextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Password = TextBox.Text;
                PasswordBox.Focus();
            }
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            FocusBorder.Visibility = Visibility.Visible;
            MainBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("PrimaryColor");
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // 다른 내부 컨트롤이 포커스를 받은 경우가 아닐 때만 포커스 테두리 숨기기
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (!PasswordBox.IsFocused && !TextBox.IsFocused && !ToggleButton.IsFocused)
                {
                    FocusBorder.Visibility = Visibility.Collapsed;
                    MainBorder.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
                }
            }));
        }

        public new void Focus()
        {
            if (PasswordBox.Visibility == Visibility.Visible)
            {
                PasswordBox.Focus();
            }
            else
            {
                TextBox.Focus();
            }
        }
    }
}
