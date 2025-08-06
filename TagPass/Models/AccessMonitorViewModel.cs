using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using S1SocketDataDTO.Models;

namespace TagPass.Models
{
    public class AccessMonitorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AlarmEventDto> _accessLogs;
        private AlarmEventDto _selectedEmployee;
        private string _statusFilter = "전체";
        private DateTime _currentTime;
        private DispatcherTimer _timer;

        public AccessMonitorViewModel()
        {
            AccessLogs = new ObservableCollection<AlarmEventDto>();
            InitializeTimer();
        }


        private void InitializeTimer()
        {
            _currentTime = DateTime.Now;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        /// <summary>
        /// Access Log 추가
        /// </summary>
        /// <param name="newLog">새로운 Access Event 로그</param>
        public void AddAccessLog(AlarmEventDto newLog)
        {
            AccessLogs.Insert(0, newLog); // 최신 로그를 맨 위에 추가
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now;
        }

        #region Properties

        public DateTime CurrentTime { get => _currentTime; set { _currentTime = value; OnPropertyChanged(); } }

        public ObservableCollection<AlarmEventDto> AccessLogs { get => _accessLogs; set { _accessLogs = value; OnPropertyChanged(); } }

        public AlarmEventDto SelectedEmployee { get => _selectedEmployee; set { _selectedEmployee = value; OnPropertyChanged(); } }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _timer?.Stop();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}