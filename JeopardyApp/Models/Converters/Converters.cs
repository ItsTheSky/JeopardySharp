using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Common;

namespace JeopardyApp.Utilities;

public class BoolToPlayPauseSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Symbol.Pause : Symbol.Play;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToPlayPauseTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? "Pause" : "Play";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class VolumeToSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not float volume) 
            return Symbol.Speaker0;
        
        return volume > 0.5f ? Symbol.Speaker2 : volume > 0.1f ? Symbol.Speaker1 : Symbol.Speaker0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}