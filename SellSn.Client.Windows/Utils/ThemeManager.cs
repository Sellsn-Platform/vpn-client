﻿using System;
using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using SellSn.Client.Debug;
using SellSn.Client.Windows.Configuration.Models;
using SellSn.Client.Windows.Models;

namespace SellSn.Client.Windows.Utils;

/// <summary>
///     Responsible for managing the colors, themes, etc. (dark or light mode, UI colors)
/// </summary>
internal static class ThemeManager
{
    /// <summary>
    ///     Switches the theme by hot-swapping the resource dictionaries
    /// </summary>
    /// <param name="primaryColor">Primary color to set</param>
    /// <param name="backgroundMode">Background color to set (dark or light)</param>
    /// <param name="customColorBrush">Custom color brush to be set for the UI colors</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="backgroundMode" /> is out-of-range</exception>
    internal static void SwitchTheme(ThemeColor primaryColor, BackgroundMode backgroundMode,
        Color customColorBrush = default)
    {
        try
        {
            var baseTheme = backgroundMode switch
            {
                BackgroundMode.Dark => Theme.Dark,
                BackgroundMode.Light => Theme.Light,
                _ => throw new ArgumentOutOfRangeException(nameof(backgroundMode), backgroundMode, null)
            };

            ITheme theme = primaryColor switch
            {
                ThemeColor.Default => Theme.Create(baseTheme,
                    Color.FromRgb(71, 96, 255),
                    Color.FromRgb(13, 204, 255)),
                ThemeColor.Accent => Theme.Create(baseTheme,
                    SystemParameters.WindowGlassColor,
                    SystemParameters.WindowGlassColor),
                ThemeColor.Custom => Theme.Create(baseTheme,
                    customColorBrush,
                    customColorBrush),
                _ => Theme.Create(baseTheme,
                    (Color)ColorConverter.ConvertFromString(primaryColor.ToString()),
                    (Color)ColorConverter.ConvertFromString(primaryColor.ToString()))
            };

            Application.Current.Resources.SetTheme(theme);
        }
        catch (Exception e)
        {
            DebugLogger.Write("svpn-client-win-thememan", $"wtf?!?! {e}");
            MessageBox.Show(
                "Something went wrong when processing theme settings, please report this exception, and the log file to SellSN-VPN support.",
                "SellSN-VPN", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}