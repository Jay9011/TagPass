using System;
using System.Globalization;
using System.Windows.Data;

namespace TagPass.Converters
{
    /// <summary>
    /// 문자열에 특정 텍스트가 포함되어 있는지 확인하는 Converter
    /// </summary>
    public class StringContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string sourceText = value.ToString();
            string searchText = parameter.ToString();

            // 대소문자 구분 없이 검색
            return sourceText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("StringContainsConverter는 단방향 변환만 지원합니다.");
        }
    }
}