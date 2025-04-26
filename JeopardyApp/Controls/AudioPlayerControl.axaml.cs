using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace JeopardyApp.Controls;

public partial class AudioPlayerControl : UserControl
{
    public AudioPlayerControl()
    {
        InitializeComponent();
        
        DataContext = new AudioPlayerViewModel();
        ViewModel.PositionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        ViewModel.PositionTimer.Tick += ViewModel.UpdatePosition;
        ViewModel.PositionTimer.Start();
    }
    
    public AudioPlayerViewModel ViewModel => (AudioPlayerViewModel) DataContext!;
}