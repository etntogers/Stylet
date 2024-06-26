﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Stylet.Logging;

namespace Stylet.Xaml;

/// <summary>
/// Converter to take an Icon, and convert it to a BitmapSource
/// </summary>
public class IconToBitmapSourceConverter : IValueConverter
{
    private static readonly ILogger logger = LogManager.GetLogger(typeof(IconToBitmapSourceConverter));

    /// <summary>
    /// Singleton instance of this converter. Usage e.g. Converter="{x:Static s:IconToBitmapSourceConverter.Instance}"
    /// </summary>
    public static readonly IconToBitmapSourceConverter Instance = new();

    /// <summary>
    /// Converts a value
    /// </summary>
    /// <param name="value">value as produced by source binding</param>
    /// <param name="targetType">target type</param>
    /// <param name="parameter">converter parameter</param>
    /// <param name="culture">culture information</param>
    /// <returns>Converted value</returns>
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not Icon icon)
            return null;

        try
        {
            BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bs.Freeze();
            return bs;
        }
        catch (Exception e)
        {
            logger.Error(e, string.Format("Error trying to call CreateBitmapSourceFromHIcon: {0}", e.Message));
            return null;
        }
    }

    /// <summary>
    /// Converts a value back. Not supported.
    /// </summary>
    /// <param name="value">value, as produced by target</param>
    /// <param name="targetType">target type</param>
    /// <param name="parameter">converter parameter</param>
    /// <param name="culture">culture information</param>
    /// <returns>Converted back value</returns>
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
