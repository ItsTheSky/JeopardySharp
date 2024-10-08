using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JeopardyApp.Models.Converters;

/// <summary>
/// Given a boolean value, returns one of the two int values provided.
/// </summary>
public class IntTernaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            Console.WriteLine(parameter?.ToString());
            var parameterSplits = parameter?.ToString()?.Split('|');
            if (parameterSplits?.Length == 2)
                return boolValue ? int.Parse(parameterSplits[0]) : int.Parse(parameterSplits[1]);
            
            throw new ArgumentException("Parameter must be in the format 'int|int'");
        }

        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}