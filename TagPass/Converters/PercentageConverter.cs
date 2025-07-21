using System;
using System.Globalization;
using System.Windows.Data;

namespace TagPass.Converters
{
    /// <summary>
    /// 값에 백분율을 적용하는 컨버터
    /// ConverterParameter: 0.0 ~ 1.0 사이의 비율 (예: 0.8 = 80%)
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualSize && parameter is string percentageString)
            {
                if (double.TryParse(percentageString, NumberStyles.Float, CultureInfo.InvariantCulture, out double percentage))
                {
                    return actualSize * percentage;
                }
            }

            // 기본값 반환
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("PercentageConverter는 단방향 변환만 지원합니다.");
        }
    }
}