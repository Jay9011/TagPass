using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using S1SocketDataDTO.Models;

namespace TagPass.Models
{
    public class AccessMonitorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AlarmEventDto> _accessLogs;
        private AlarmEventDto _selectedEmployee;
        private string _statusFilter = "전체";

        public AccessMonitorViewModel()
        {
            AccessLogs = new ObservableCollection<AlarmEventDto>();
            InitializeSampleData();
        }

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
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                _statusFilter = value;
                OnPropertyChanged();
                FilterAccessLogs();
            }
        }

        private void InitializeSampleData()
        {
            // 샘플 데이터 생성
            var sampleData = new[]
            {
                new AlarmEventDto
                {
                    AlarmID = 1,
                    FormattedDate = DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss"),
                    Sabun = "2023001",
                    Name = "김민수",
                    OrgName = "개발팀",
                    GradeName = "팀장",
                    StateName = "출입",
                    State = "IN",
                    Photo = "/Images/sample1.jpg",
                    EqName = "정문 출입구",
                    LocationName = "본사 1층"
                },
                new AlarmEventDto
                {
                    AlarmID = 2,
                    FormattedDate = DateTime.Now.AddMinutes(-12).ToString("yyyy-MM-dd HH:mm:ss"),
                    Sabun = "2023002",
                    Name = "이영희",
                    OrgName = "인사팀",
                    GradeName = "대리",
                    StateName = "퇴실",
                    State = "OUT",
                    Photo = "/Images/sample2.jpg",
                    EqName = "후문 출입구",
                    LocationName = "본사 1층"
                },
                new AlarmEventDto
                {
                    AlarmID = 3,
                    FormattedDate = DateTime.Now.AddMinutes(-18).ToString("yyyy-MM-dd HH:mm:ss"),
                    Sabun = "2023003",
                    Name = "박철수",
                    OrgName = "영업팀",
                    GradeName = "과장",
                    StateName = "출입",
                    State = "IN",
                    Photo = "/Images/sample3.jpg",
                    EqName = "주차장 출입구",
                    LocationName = "본사 지하1층"
                },
                new AlarmEventDto
                {
                    AlarmID = 4,
                    FormattedDate = DateTime.Now.AddMinutes(-25).ToString("yyyy-MM-dd HH:mm:ss"),
                    Sabun = "2023004",
                    Name = "정수연",
                    OrgName = "기획팀",
                    GradeName = "주임",
                    StateName = "출입",
                    State = "IN",
                    Photo = "/Images/sample4.jpg",
                    EqName = "정문 출입구",
                    LocationName = "본사 1층"
                },
                new AlarmEventDto
                {
                    AlarmID = 5,
                    FormattedDate = DateTime.Now.AddMinutes(-32).ToString("yyyy-MM-dd HH:mm:ss"),
                    Sabun = "2023005",
                    Name = "최동훈",
                    OrgName = "총무팀",
                    GradeName = "사원",
                    StateName = "퇴실",
                    State = "OUT",
                    Photo = "/Images/sample5.jpg",
                    EqName = "비상구",
                    LocationName = "본사 2층"
                }
            };

            foreach (var item in sampleData)
            {
                AccessLogs.Add(item);
            }

            // 첫 번째 항목을 선택된 직원으로 설정
            SelectedEmployee = AccessLogs.FirstOrDefault();
        }

        private void FilterAccessLogs()
        {
            // 필터링 로직 (실제 구현 시 사용)
            // 현재는 샘플 데이터이므로 필터링 생략
        }

        public void AddAccessLog(AlarmEventDto newLog)
        {
            AccessLogs.Insert(0, newLog); // 최신 로그를 맨 위에 추가
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}