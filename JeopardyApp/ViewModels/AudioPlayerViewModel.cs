using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Wave;

public partial class AudioPlayerViewModel : ObservableObject
{
    public DispatcherTimer PositionTimer;
    private IWavePlayer? _wavePlayer;
    private AudioFileReader? _audioFileReader;
    
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private double _position;
    [ObservableProperty] private double _duration;
    [ObservableProperty] private bool _canSeek;
    [ObservableProperty] private float _volume = 1.0f;
    
    [ObservableProperty] private string _currentTime;
    [ObservableProperty] private string _totalTime;

    public void Initialize(string audioFilePath)
    {
        try
        {
            DisposeAudio();
            
            _audioFileReader = new AudioFileReader(audioFilePath);
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(_audioFileReader);
            
            Duration = _audioFileReader.TotalTime.TotalSeconds;
            Position = 0;
            Volume = 1.0f;
            CanSeek = true;
            
            _audioFileReader.Volume = Volume;
        }
        catch (Exception ex)
        {
            // Handle error
            DisposeAudio();
        }
    }

    public void UpdatePosition(object? sender, EventArgs e)
    {
        if (_audioFileReader != null && _wavePlayer != null)
        {
            Position = _audioFileReader.CurrentTime.TotalSeconds;
            CurrentTime = _audioFileReader.CurrentTime.ToString(@"mm\:ss");
            TotalTime = _audioFileReader.TotalTime.ToString(@"mm\:ss");
            
            if (_wavePlayer.PlaybackState == PlaybackState.Stopped)
            {
                IsPlaying = false;
                Position = 0;
            }
        }
    }

    partial void OnVolumeChanged(float value)
    {
        if (_audioFileReader != null)
        {
            _audioFileReader.Volume = value;
        }
    }

    partial void OnPositionChanged(double value)
    {
        if (_audioFileReader != null && Math.Abs(value - _audioFileReader.CurrentTime.TotalSeconds) > 0.1)
        {
            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
        }
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (_wavePlayer == null) return;

        if (_wavePlayer.PlaybackState == PlaybackState.Playing)
        {
            _wavePlayer.Pause();
            IsPlaying = false;
        }
        else
        {
            _wavePlayer.Play();
            IsPlaying = true;
        }
    }

    [RelayCommand]
    private void Stop()
    {
        if (_wavePlayer == null || _audioFileReader == null) return;

        _wavePlayer.Stop();
        _audioFileReader.Position = 0;
        IsPlaying = false;
        Position = 0;
    }

    private void DisposeAudio()
    {
        if (_wavePlayer != null)
        {
            if (_wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                _wavePlayer.Stop();
            }
            _wavePlayer.Dispose();
            _wavePlayer = null;
        }

        if (_audioFileReader != null)
        {
            _audioFileReader.Dispose();
            _audioFileReader = null;
        }
        
        IsPlaying = false;
        Position = 0;
        Duration = 0;
        CanSeek = false;
    }

}