﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Hardcodet.Wpf.TaskbarNotification;
using SellSn.Client.Windows.Common;
using SellSn.Client.Windows.Configuration.Interfaces;
using SellSn.Client.Windows.Configuration.Models;
using SellSn.Client.Windows.ViewModels;
using SellSn.Client.Windows.Views;

namespace SellSn.Client.Windows;

/// <inheritdoc cref="System.Windows.Window" />
internal sealed partial class MainWindow : Window, IDisposable
{
    // Keep a memory-cached version of MainView so we don't have to reinitialise it all the time

    private readonly MainView _mainView;

    private readonly TaskbarIcon _taskbarIcon;

    private readonly BeginStoryboard _viewLoaded;

    private readonly BeginStoryboard _viewUnloaded;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel(this);
        Application.Current.MainWindow = this;

        /* if (Globals.IsBeta)
            MessageBox.Show(
                "This is a pre-release (beta) build of the LightVPN client, compiled on the 22nd of June 2021 (3:14 BST). This build is intended for use with intention of feedback on any issues or suggestions. Please make sure you keep up-to-date with the release cycles. This build should not be used to replace your stable build of LightVPN, and should be kept in a separate folder.",
                "LightVPN beta notice", MessageBoxButton.OK, MessageBoxImage.Exclamation); */

        var settings = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Read();

        if (settings is null)
        {
            settings = new AppConfiguration();
            Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>().Write(settings);
        }

        if ((settings.SizeSaving?.Width != null && settings.SizeSaving?.Width >= MinWidth) ||
            settings.SizeSaving?.Height >= MinHeight)
        {
            Width = settings.SizeSaving.Width;
            Height = settings.SizeSaving.Height;
        }

        // Locates resources in the XAML and stores them for use in code-behind
        _viewLoaded = FindResource("LoadView") as BeginStoryboard;
        _viewUnloaded = FindResource("UnloadView") as BeginStoryboard;
        _taskbarIcon = FindResource("TaskbarIcon") as TaskbarIcon;

        _mainView = new MainView();

        if (settings.IsFirstRun)
        {
            LoadView(new FirstRunView());
            return;
        }

        LoadView(_mainView);

        SizeChanged += (_, args) =>
        {
            var manager = Globals.Container.GetInstance<IConfigurationManager<AppConfiguration>>();
            var config = manager.Read();

            config.SizeSaving = new AppSizeSaving
            {
                Width = (uint)args.NewSize.Width,
                Height = (uint)args.NewSize.Height
            };

            manager.Write(config);
        };

        if (!Globals.IsStartingMinimised) return;

        Globals.IsInTray = true;
        Application.Current.MainWindow?.Hide();
    }

    public void Dispose()
    {
        _taskbarIcon.Dispose();
    }

    private void UnloadCompleted(object sender, EventArgs e)
    {
        _viewLoaded.Storyboard.Begin();
    }

    public async void LoadView(Page page)
    {
        if (page is MainView) page = _mainView;


        // Play animation
        _viewUnloaded.Storyboard.Begin();
        await Task.Delay(400);

        MainFrame.Navigate(page);
    }
}