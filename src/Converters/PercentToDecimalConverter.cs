// ###########################################################################
//  Project:      OmniDownloader
//  Version:      0.9.1
//  Author:       Tomer Alcavi
//  GitHub:       https://github.com/alcavix
//  Project Link: https://github.com/alcavix/OmniDownloader
//  License:      MIT
//
//  If you find this project useful, drop a star or fork!
//  Questions or ideas? Open an issue on the project’s GitHub page!
//  Please keep this little credit line. It means a lot for the open-source spirit :)
//  Grateful for the open-source community and spirit that inspires projects like this.
// ###########################################################################

using System.Globalization;
using Avalonia.Data.Converters;

namespace OmniDownloader.Converters;

public class PercentToDecimalConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            return percentage / 100.0;
        }
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double decimal_value)
        {
            return decimal_value * 100.0;
        }
        return 0.0;
    }
}
