using System;
using System.ComponentModel;

namespace TagPass.Services
{
    /// <summary>
    /// 시간 관리 서비스 인터페이스
    /// </summary>
    public interface ITimeManager : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// 현재 시간
        /// </summary>
        DateTime CurrentTime { get; }

        /// <summary>
        /// 시간 업데이트 시작
        /// </summary>
        void Start();

        /// <summary>
        /// 시간 업데이트 중지
        /// </summary>
        void Stop();
    }
}