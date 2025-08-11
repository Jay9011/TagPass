using S1SocketDataDTO.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.ComponentModel;

namespace TagPass.Views.Components
{
    /// <summary>
    /// 사원 정보 카드 컴포넌트
    /// </summary>
    public partial class BasicCard : UserControl, IDisposable
    {
        private BitmapImage _currentBitmapImage;

        public BasicCard()
        {
            InitializeComponent();
            this.DataContext = this;
            this.SizeChanged += BasicCard_SizeChanged;
        }

        /// <summary>
        /// AlarmEventDto 데이터로 카드 업데이트
        /// </summary>
        /// <param name="eventData">출입 이벤트 데이터</param>
        public void UpdateCard(AlarmEventDto eventData)
        {
            if (eventData == null)
            {
                ClearCard();
                return;
            }

            CleanupCurrentImage();

            AlarmID = eventData.AlarmID;
            FormattedDate = eventData.FormattedDate ?? string.Empty;
            EqMasterID = eventData.EqMasterID;
            EqName = eventData.EqName ?? string.Empty;
            State = eventData.State ?? string.Empty;
            StateName = eventData.StateName ?? string.Empty;
            CardNo = eventData.CardNo ?? string.Empty;
            PersonName = eventData.Name ?? string.Empty;
            Sabun = eventData.Sabun ?? string.Empty;
            Photo = eventData.Photo ?? string.Empty;
            PhotoPath = eventData.PhotoPath ?? string.Empty;
            PersonUser1 = eventData.PersonUser1 ?? string.Empty;
            PersonUser2 = eventData.PersonUser2 ?? string.Empty;
            PersonUser3 = eventData.PersonUser3 ?? string.Empty;
            PersonUser4 = eventData.PersonUser4 ?? string.Empty;
            PersonUser5 = eventData.PersonUser5 ?? string.Empty;
            OrgName = eventData.OrgName ?? string.Empty;
            OrgCode = eventData.OrgCode ?? string.Empty;
            GradeName = eventData.GradeName ?? string.Empty;
            LocationID = eventData.LocationID;
            LocationName = eventData.LocationName ?? string.Empty;
            IsAlarm = eventData.IsAlarm;

            // 이미지 처리
            LoadImage(eventData.PhotoPath, eventData.Photo);
        }

        /// <summary>
        /// 카드 내용 클리어
        /// </summary>
        public void ClearCard()
        {
            CleanupCurrentImage();

            AlarmID = 0;
            FormattedDate = string.Empty;
            EqMasterID = 0;
            EqName = string.Empty;
            State = string.Empty;
            StateName = string.Empty;
            CardNo = string.Empty;
            PersonName = string.Empty;
            Sabun = string.Empty;
            Photo = string.Empty;
            PhotoPath = string.Empty;
            PersonUser1 = string.Empty;
            PersonUser2 = string.Empty;
            PersonUser3 = string.Empty;
            PersonUser4 = string.Empty;
            PersonUser5 = string.Empty;
            OrgName = string.Empty;
            OrgCode = string.Empty;
            GradeName = string.Empty;
            LocationID = 0;
            LocationName = string.Empty;
            IsAlarm = 0;
            PhotoImg = null; // PhotoImg 프로퍼티 초기화
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

        /// <summary>
        /// 이미지 로드
        /// </summary>
        private void LoadImage(string photoPath, string photo)
        {
            CleanupCurrentImage();

            if (string.IsNullOrWhiteSpace(photoPath) || string.IsNullOrWhiteSpace(photo))
            {
                return;
            }

            try
            {
                var imageUrl = $"{photoPath.TrimEnd('/')}/{photo}";
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                PhotoImg = bitmapImage;
                _currentBitmapImage = bitmapImage;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Image loading failed: {e.Message}");
                PhotoImg = null;
            }
        }

        /// <summary>
        /// 이미지 정리
        /// </summary>
        private void CleanupCurrentImage()
        {
            if (_currentBitmapImage != null)
            {
                try
                {
                    _currentBitmapImage.UriSource = null;
                }
                catch (Exception)
                {
                    // 무시
                }
                _currentBitmapImage = null;
            }

            PhotoImg = null;
        }

        #region DependencyProperties

        // 기본 정보
        public static readonly DependencyProperty IsAlarmProperty = CreateProperty<int>("IsAlarm");
        public static readonly DependencyProperty AlarmIDProperty = CreateProperty<int>("AlarmID");
        public static readonly DependencyProperty FormattedDateProperty = CreateProperty<string>("FormattedDate");
        public static readonly DependencyProperty EqMasterIDProperty = CreateProperty<int>("EqMasterID");
        public static readonly DependencyProperty EqNameProperty = CreateProperty<string>("EqName");
        public static readonly DependencyProperty StateProperty = CreateProperty<string>("State");
        public static readonly DependencyProperty StateNameProperty = CreateProperty<string>("StateName");

        // 카드 및 사원 정보
        public static readonly DependencyProperty PersonNameProperty = CreateProperty<string>("PersonName");
        public static readonly DependencyProperty SabunProperty = CreateProperty<string>("Sabun");
        public static readonly DependencyProperty CardNoProperty = CreateProperty<string>("CardNo");

        // 조직 정보
        public static readonly DependencyProperty OrgCodeProperty = CreateProperty<string>("OrgCode");
        public static readonly DependencyProperty OrgNameProperty = CreateProperty<string>("OrgName");
        public static readonly DependencyProperty GradeNameProperty = CreateProperty<string>("GradeName");

        // 사용자 정의 필드
        public static readonly DependencyProperty PersonUser1Property = CreateProperty<string>("PersonUser1");
        public static readonly DependencyProperty PersonUser2Property = CreateProperty<string>("PersonUser2");
        public static readonly DependencyProperty PersonUser3Property = CreateProperty<string>("PersonUser3");
        public static readonly DependencyProperty PersonUser4Property = CreateProperty<string>("PersonUser4");
        public static readonly DependencyProperty PersonUser5Property = CreateProperty<string>("PersonUser5");

        // 위치 정보
        public static readonly DependencyProperty LocationIDProperty = CreateProperty<int>("LocationID");
        public static readonly DependencyProperty LocationNameProperty = CreateProperty<string>("LocationName");

        // 이미지 프로퍼티
        public static readonly DependencyProperty PhotoProperty = CreateProperty<string>("Photo");
        public static readonly DependencyProperty PhotoPathProperty = CreateProperty<string>("PhotoPath");
        public static readonly DependencyProperty PhotoImgProperty = CreateProperty<BitmapImage>("PhotoImg");

        #endregion

        #region Properties

        public int IsAlarm { get => GetValue<int>(IsAlarmProperty); set => SetValue(IsAlarmProperty, value); }
        public int AlarmID { get => GetValue<int>(AlarmIDProperty); set => SetValue(AlarmIDProperty, value); }
        public string FormattedDate { get => GetValue<string>(FormattedDateProperty); set => SetValue(FormattedDateProperty, value); }
        public int EqMasterID { get => GetValue<int>(EqMasterIDProperty); set => SetValue(EqMasterIDProperty, value); }
        public string EqName { get => GetValue<string>(EqNameProperty); set => SetValue(EqNameProperty, value); }
        public string State { get => GetValue<string>(StateProperty); set => SetValue(StateProperty, value); }
        public string StateName { get => GetValue<string>(StateNameProperty); set => SetValue(StateNameProperty, value); }
        public string PersonName { get => GetValue<string>(PersonNameProperty); set => SetValue(PersonNameProperty, value); }
        public string Sabun { get => GetValue<string>(SabunProperty); set => SetValue(SabunProperty, value); }
        public string CardNo { get => GetValue<string>(CardNoProperty); set => SetValue(CardNoProperty, value); }
        public string OrgCode { get => GetValue<string>(OrgCodeProperty); set => SetValue(OrgCodeProperty, value); }
        public string OrgName { get => GetValue<string>(OrgNameProperty); set => SetValue(OrgNameProperty, value); }
        public string GradeName { get => GetValue<string>(GradeNameProperty); set => SetValue(GradeNameProperty, value); }
        public string PersonUser1 { get => GetValue<string>(PersonUser1Property); set => SetValue(PersonUser1Property, value); }
        public string PersonUser2 { get => GetValue<string>(PersonUser2Property); set => SetValue(PersonUser2Property, value); }
        public string PersonUser3 { get => GetValue<string>(PersonUser3Property); set => SetValue(PersonUser3Property, value); }
        public string PersonUser4 { get => GetValue<string>(PersonUser4Property); set => SetValue(PersonUser4Property, value); }
        public string PersonUser5 { get => GetValue<string>(PersonUser5Property); set => SetValue(PersonUser5Property, value); }
        public int LocationID { get => GetValue<int>(LocationIDProperty); set => SetValue(LocationIDProperty, value); }
        public string LocationName { get => GetValue<string>(LocationNameProperty); set => SetValue(LocationNameProperty, value); }
        public string Photo { get => GetValue<string>(PhotoProperty); set => SetValue(PhotoProperty, value); }
        public string PhotoPath { get => GetValue<string>(PhotoPathProperty); set => SetValue(PhotoPathProperty, value); }
        public BitmapImage PhotoImg { get => GetValue<BitmapImage>(PhotoImgProperty); set => SetValue(PhotoImgProperty, value); }

        #endregion

        /// <summary>
        /// DependencyProperty 생성 헬퍼 메서드
        /// </summary>
        private static DependencyProperty CreateProperty<T>(string name, T defaultValue = default(T))
        {
            return DependencyProperty.Register(name, typeof(T), typeof(BasicCard), new PropertyMetadata(defaultValue));
        }

        /// <summary>
        /// DependencyProperty 값 가져오기 헬퍼 메서드
        /// </summary>
        private T GetValue<T>(DependencyProperty property)
        {
            return (T)GetValue(property);
        }

        /// <summary>
        /// 리소스 정리
        /// </summary>
        public void Dispose()
        {
            CleanupCurrentImage();
        }
    }
}
