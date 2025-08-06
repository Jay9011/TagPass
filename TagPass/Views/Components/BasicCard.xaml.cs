using S1SocketDataDTO.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Net.Http;
using System.IO;
using System.ComponentModel;

namespace TagPass.Views.Components
{
    /// <summary>
    /// 사원 정보 카드 컴포넌트
    /// </summary>
    public partial class BasicCard : UserControl
    {
        private DispatcherTimer _clearTimer;
        private BitmapImage _currentBitmapImage;
        private readonly Dictionary<string, WeakReference> _imageCache = new Dictionary<string, WeakReference>();

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

            ClearTimer();
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

            StartClearTimer(); // 자동 클리어 타이머 시작
        }

        /// <summary>
        /// 카드 내용 클리어
        /// </summary>
        public void ClearCard()
        {
            ClearTimer();
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

        /// <summary>
        /// 자동 클리어 타이머 시작
        /// </summary>
        private void StartClearTimer()
        {
            if (AutoClearSeconds <= 0) return;

            _clearTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(AutoClearSeconds)
            };

            _clearTimer.Tick += (s, e) => ClearCard();
            _clearTimer.Start();
        }

        /// <summary>
        /// 타이머 정리
        /// </summary>
        private void ClearTimer()
        {
            if (_clearTimer != null)
            {
                _clearTimer.Stop();
                _clearTimer = null;
            }
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
        /// 이미지 로드 및 메모리 관리
        /// </summary>
        /// <param name="imageUrl">이미지 URL</param>
        private void LoadImage(string photoPath, string photo)
        {
            try
            {
                CleanupCurrentImage();

                if (string.IsNullOrWhiteSpace(photoPath) || string.IsNullOrWhiteSpace(photo))
                {
                    return;
                }

                var imageUrl = $"{photoPath.TrimEnd('/')}/{photo}";

                if (_imageCache.TryGetValue(imageUrl, out var cachedRef) &&
                    cachedRef.Target is BitmapImage cachedImage && cachedImage.CanFreeze)
                {
                    PhotoImg = cachedImage;
                    return;
                }

                var bitmapImage = new BitmapImage();
                _currentBitmapImage = bitmapImage;

                EventHandler<DownloadProgressEventArgs> progressHandler = null;
                EventHandler downloadCompletedHandler = null;
                EventHandler<ExceptionEventArgs> downloadFailedHandler = null;

                progressHandler = (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine($"다운로드 진행률: {e.Progress}%");
                };

                downloadCompletedHandler = (s, e) =>
                {
                    try
                    {
                        var img = s as BitmapImage;
                        if (img != null && _currentBitmapImage == img)
                        {
                            if (img.PixelWidth * img.PixelHeight * 4 > 2 * 1024 * 1024) // 2MB
                            {
                                img = ResizeImage(img, 800, 600);
                            }

                            PhotoImg = img;

                            _imageCache[imageUrl] = new WeakReference(img); // 캐시 사용
                        }
                    }
                    finally
                    {
                        if (s is BitmapImage bitmap)
                        {
                            bitmap.DownloadProgress -= progressHandler;
                            bitmap.DownloadCompleted -= downloadCompletedHandler;
                            bitmap.DownloadFailed -= downloadFailedHandler;
                        }
                    }
                };

                downloadFailedHandler = (s, e) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"이미지 다운로드 실패: {e.ErrorException}");
                        PhotoImg = null;
                    }
                    finally
                    {
                        if (s is BitmapImage bitmap)
                        {
                            bitmap.DownloadProgress -= progressHandler;
                            bitmap.DownloadCompleted -= downloadCompletedHandler;
                            bitmap.DownloadFailed -= downloadFailedHandler;
                        }
                    }
                };

                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //bitmapImage.DecodePixelWidth = 800;
                //bitmapImage.DecodePixelHeight = 600;

                bitmapImage.DownloadProgress += progressHandler;
                bitmapImage.DownloadCompleted += downloadCompletedHandler;
                bitmapImage.DownloadFailed += downloadFailedHandler;

                bitmapImage.EndInit();
            }
            catch (Exception e)
            {
                CleanupCurrentImage();
                // TODO: 추후 오류 처리 필요
            }
        }

        /// <summary>
        /// 이미지 리사이즈
        /// </summary>
        /// <param name="img"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private BitmapImage ResizeImage(BitmapImage img, int maxWidth, int maxHeight)
        {
            try
            {
                var scaleX = (double)maxWidth / img.PixelWidth;
                var scaleY = (double)maxHeight / img.PixelHeight;
                var scale = Math.Min(scaleX, scaleY);

                if (scale >= 1.0) return img;

                var newWidth = (int)(img.PixelWidth * scale);
                var newHeight = (int)(img.PixelHeight * scale);

                var resized = new BitmapImage();
                resized.BeginInit();
                resized.UriSource = img.UriSource;
                resized.DecodePixelWidth = newWidth;
                resized.DecodePixelHeight = newHeight;
                resized.CacheOption = BitmapCacheOption.OnLoad;
                resized.EndInit();

                return resized;
            }
            catch (Exception)
            {
                return img;
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
                }
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

        // 컴포넌트 설정
        public static readonly DependencyProperty AutoClearSecondsProperty = CreateProperty<double>("AutoClearSeconds", 2.5);

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
        public double AutoClearSeconds { get => GetValue<double>(AutoClearSecondsProperty); set => SetValue(AutoClearSecondsProperty, value); }

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
            ClearTimer();
            CleanupCurrentImage();
            _imageCache.Clear();
        }
    }
}
