using System;
using System.Globalization;
using System.Windows.Data;

namespace TagPass
{
    #region 날짜 포맷 타입

    /// <summary>
    /// 날짜 포맷 타입을 정의하는 ENUM
    /// </summary>
    public enum DateFormatType
    {
        /// <summary>
        /// 스마트 포맷 (기본값)
        /// </summary>
        Smart,

        /// <summary>
        /// 상세 포맷_kor (yyyy년 MM월 dd일 HH:mm:ss)
        /// </summary>
        Detailed_kor,

        /// <summary>
        /// 상세 포맷_dash (yyyy-MM-dd HH:mm:ss)
        /// </summary>
        Detailed_dash,

        /// <summary>
        /// 상세 포맷_slash (yyyy/MM/dd HH:mm:ss)
        /// </summary>
        Detailed_slash,

        /// <summary>
        /// 간단 포맷 (MM/dd HH:mm)
        /// </summary>
        Simple,

        /// <summary>
        /// 날짜만_kor (yyyy년 MM월 dd일)
        /// </summary>
        DateOnly_kor,

        /// <summary>
        /// 날짜만_dash (yyyy-MM-dd)
        /// </summary>
        DateOnly_dash,

        /// <summary>
        /// 날짜만_slash (yyyy/MM/dd)
        /// </summary>
        DateOnly_slash,

        /// <summary>
        /// 시간만 (HH:mm:ss)
        /// </summary>
        TimeOnly,

        /// <summary>
        /// 시간만_kor (HH시 mm분 ss초)
        /// </summary>
        TimeOnly_kor,

        /// <summary>
        /// 시간만_dash (HH-MM-ss)
        /// </summary>
        TimeOnly_dash,

        /// <summary>
        /// 시간만_slash (HH/MM/ss)
        /// </summary>
        TimeOnly_slash
    }

    #endregion

}

namespace TagPass.Converters
{
    /// <summary>
    /// 문자열 형태의 날짜를 다양한 형태로 변환하는 Value Converter
    /// </summary>
    public class DateFormatConverter : IValueConverter
    {
        #region 지원 날짜 형식

        private static readonly string[] SupportedFormats = {
            "yyyyMMddHHmmssfff",
            "yyyyMMddHHmmss",
            "yyyyMMddHHmm",
            "yyyyMMdd",
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/dd",
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy",
        };

        #endregion

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string stringDatetime || string.IsNullOrEmpty(stringDatetime))
                return value;

            try
            {
                var dateTime = ParseDateTime(stringDatetime);
                var today = DateTime.Today;

                DateFormatType formatType = DateFormatType.Smart;
                if (parameter is DateFormatType enumFormat)
                {
                    formatType = enumFormat;
                }
                // 문자열로 전달된 경우 ENUM으로 파싱 시도
                else if (parameter is string stringFormat)
                {
                    if (Enum.TryParse<DateFormatType>(stringFormat, true, out var parsedFormat))
                    {
                        formatType = parsedFormat;
                    }
                }

                return FormatDateTime(dateTime, today, formatType);
            }
            catch
            {
                return stringDatetime;
            }
        }

        /// <summary>
        /// 다양한 형식의 날짜 문자열을 DateTime으로 파싱
        /// </summary>
        /// <param name="dateString">파싱할 날짜 문자열</param>
        /// <returns>파싱된 DateTime</returns>
        private static DateTime ParseDateTime(string dateString)
        {
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            // 자동 파싱이 안된 경우 미리 지정된 지원하는 형식들로 파싱
            foreach (var format in SupportedFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
            }

            throw new FormatException($"지원하지 않는 날짜 형식입니다: {dateString}");
        }

        private static string FormatDateTime(DateTime dateTime, DateTime today, DateFormatType formatType)
        {
            switch (formatType)
            {
                case DateFormatType.Detailed_kor:
                    return $"{dateTime:yyyy년 MM월 dd일 HH:mm:ss}";
                case DateFormatType.Detailed_dash:
                    return $"{dateTime:yyyy-MM-dd HH:mm:ss}";
                case DateFormatType.Detailed_slash:
                    return $"{dateTime:yyyy/MM/dd HH:mm:ss}";
                case DateFormatType.Simple:
                    return $"{dateTime:MM/dd HH:mm}";
                case DateFormatType.DateOnly_kor:
                    return $"{dateTime:yyyy년 MM월 dd일}";
                case DateFormatType.DateOnly_dash:
                    return $"{dateTime:yyyy-MM-dd}";
                case DateFormatType.DateOnly_slash:
                    return $"{dateTime:yyyy/MM/dd}";
                case DateFormatType.TimeOnly:
                    return $"{dateTime:HH:mm:ss}";
                case DateFormatType.TimeOnly_kor:
                    return $"{dateTime:HH시 mm분 ss초}";
                case DateFormatType.TimeOnly_dash:
                    return $"{dateTime:HH-MM-ss}";
                case DateFormatType.TimeOnly_slash:
                    return $"{dateTime:HH/MM/ss}";
                case DateFormatType.Smart:
                default:
                    return GetSmartFormat(dateTime, today);
            }
        }

        private static string GetSmartFormat(DateTime dateTime, DateTime today)
        {
            if (dateTime.Date == today)
            {
                return $"오늘 {dateTime:HH:mm:ss}";
            }
            else if (dateTime.Date == today.AddDays(-1))
            {
                return $"어제 {dateTime:HH:mm:ss}";
            }
            else if (dateTime.Year == today.Year)
            {
                return $"{dateTime:MM/dd HH:mm:ss}";
            }
            else
            {
                return $"{dateTime:yyyy/MM/dd HH:mm:ss}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}