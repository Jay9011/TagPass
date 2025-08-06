using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace TagPass.Services
{
    /// <summary>
    /// 시간 관리 서비스 구현체
    /// </summary>
    public class TimeManager : ITimeManager
    {
        private DateTime _currentTime;
        private DispatcherTimer _timer;
        private bool _isRunning;

        public TimeManager()
        {
            _currentTime = DateTime.Now;
            InitializeTimer();
        }

        #region Properties

        /// <summary>
        /// 현재 시간
        /// </summary>
        public DateTime CurrentTime
        {
            get => _currentTime;
            private set
            {
                _currentTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 시간 업데이트 시작
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _timer?.Start();
                _isRunning = true;
            }
        }

        /// <summary>
        /// 시간 업데이트 중지
        /// </summary>
        public void Stop()
        {
            if (_isRunning)
            {
                _timer?.Stop();
                _isRunning = false;
            }
        }

        /// <summary>
        /// 타이머 초기화
        /// </summary>
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// 타이머 틱 이벤트 처리
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Stop();
            _timer?.Stop();
            _timer = null;
        }

        #endregion
    }
}