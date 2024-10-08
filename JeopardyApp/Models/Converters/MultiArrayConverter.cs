using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace JeopardyApp.Models.Converters;

public class MultiArrayConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.Count switch
        {
            0 => null,
            1 => values[0],
            _ => values.ToArray()
        };
    }
}