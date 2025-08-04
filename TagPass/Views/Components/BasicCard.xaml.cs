using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using S1SocketDataDTO.Models;

namespace TagPass.Views.Components
{
    /// <summary>
    /// 사원 정보 카드 컴포넌트
    /// </summary>
    public partial class BasicCard : UserControl
    {
        public BasicCard()
        {
            InitializeComponent();
            this.SizeChanged += BasicCard_SizeChanged;
        }

        private void BasicCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 높이가 넓이보다 크거나 넓이가 500보다 작으면 세로형 레이아웃
            bool isPortrait = e.NewSize.Height > e.NewSize.Width || e.NewSize.Width < 500;

            var landscapeLayout = FindName("LandscapeLayout") as Grid;
            var portraitLayout = FindName("PortraitLayout") as Grid;

            if (landscapeLayout != null && portraitLayout != null)
            {
                if (isPortrait)
                {
                    landscapeLayout.Visibility = Visibility.Collapsed;
                    portraitLayout.Visibility = Visibility.Visible;
                }
                else
                {
                    landscapeLayout.Visibility = Visibility.Visible;
                    portraitLayout.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
