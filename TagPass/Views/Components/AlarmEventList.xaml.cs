using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using S1SocketDataDTO.Models;

namespace TagPass.Views.Components
{
    public partial class AlarmEventList : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<AlarmEventDto> _accessLogs;
        private AlarmEventDto _selectedEmployee;

        public AlarmEventList()
        {
            InitializeComponent();
            AccessLogs = new ObservableCollection<AlarmEventDto>();
            this.DataContext = this;
        }

        /// <summary>
        /// Access Log 추가
        /// </summary>
        /// <param name="newLog">새로운 Access Event 로그</param>
        public void AddAccessLog(AlarmEventDto newLog)
        {
            AccessLogs.Insert(0, newLog); // 최신 로그를 맨 위에 추가
        }

        #region Properties

        public ObservableCollection<AlarmEventDto> AccessLogs
        {
            get => _accessLogs;
            set
            {
                _accessLogs = value;
                OnPropertyChanged();
            }
        }

        public AlarmEventDto SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                
                SelectedEmployeeChanged?.Invoke(value); // 선택된 직원 정보가 변경되었음을 부모에게 알림
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 선택된 직원이 변경되었을 때 발생하는 이벤트
        /// </summary>
        public event Action<AlarmEventDto> SelectedEmployeeChanged;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}