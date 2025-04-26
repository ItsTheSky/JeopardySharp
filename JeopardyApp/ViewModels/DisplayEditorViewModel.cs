using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyApp.Controls;
using JeopardyApp.Models;
using JeopardyApp.Views;
using NAudio.Wave;

namespace JeopardyApp.ViewModels;

public partial class DisplayEditorViewModel : ObservableObject, IDisposable
{
    public DisplayEditorViewModel(DisplayData original)
    {
        SelectedType = original.Type switch
        {
            DisplayData.DisplayDataType.Text => 0,
            DisplayData.DisplayDataType.Image => 1,
            DisplayData.DisplayDataType.Music => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        Text = original.Type == DisplayData.DisplayDataType.Text ? original.Text : null;
        ImagePath = original.Type == DisplayData.DisplayDataType.Image ? original.ImagePath : null;
        MusicPath = original.Type == DisplayData.DisplayDataType.Music ? original.MusicPath : null;
    }

    private int _selectedType;
    public int SelectedType
    {
        get => _selectedType;
        set
        {
            SetProperty(ref _selectedType, value);
            DisplayDataType = value switch
            {
                0 => DisplayData.DisplayDataType.Text,
                1 => DisplayData.DisplayDataType.Image,
                2 => DisplayData.DisplayDataType.Music,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private DisplayData.DisplayDataType _displayDataType;
    public DisplayData.DisplayDataType DisplayDataType
    {
        get => _displayDataType;
        set
        {
            SetProperty(ref _displayDataType, value);
            
            IsTextSelected = value == DisplayData.DisplayDataType.Text;
            IsImageSelected = value == DisplayData.DisplayDataType.Image;
            IsMusicSelected = value == DisplayData.DisplayDataType.Music;
        }
    }

    [ObservableProperty] private string? _text;
    private string? _imagePath;
    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            SetProperty(ref _imagePath, value);
            if (value == null)
            {
                HasImageSelected = false;
                return;
            }
            try
            {
                Bitmap = new(value);
                HasImageSelected = true;
            }
            catch (Exception e)
            {
                Dispatcher.UIThread.InvokeAsync(async () => 
                    await MainWindow.Instance.ViewModel.ShowInformationMessage("Invalid Image", e.Message));
            }
        }
    }
    
    private string? _musicPath;
    public string? MusicPath
    {
        get => _musicPath;
        set
        {
            SetProperty(ref _musicPath, value);
            if (value == null) 
            {
                HasMusic = false;
                return;
            }
            
            try
            {
                var audioFileReader = new AudioFileReader(value);
                // Create the duration string
                var duration = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds);
                MusicInfo = new MusicFileInfo(
                    System.IO.Path.GetFileNameWithoutExtension(value), 
                    duration.ToString("mm\\:ss")
                );
                
                HasMusic = true;
                
                DisplayEditor.Current.AudioPlayer.ViewModel.Initialize(value);
            }
            catch (Exception e)
            {
                Dispatcher.UIThread.InvokeAsync(async () => 
                    await MainWindow.Instance.ViewModel.ShowInformationMessage("Invalid Music File", e.Message));
                Console.WriteLine(e);
            }
        }
    }

    [ObservableProperty] private bool _isImageSelected;
    [ObservableProperty] private bool _isTextSelected;
    [ObservableProperty] private bool _isMusicSelected;
    
    [ObservableProperty] private bool _hasImageSelected;
    [ObservableProperty] private bool _hasMusic;
    [ObservableProperty] private bool _isPlaying;

    [ObservableProperty] private Bitmap? _bitmap;
    [ObservableProperty] private MusicFileInfo? _musicInfo;
    
    public record MusicFileInfo(string Title, string Duration);

    public AsyncRelayCommand BrowseImageCommand => new(async () =>
    {
        var files = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                FilePickerFileTypes.ImageAll
            ],
            SuggestedStartLocation = await MainWindow.Instance.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
        });
        if (files.Count == 0) 
            return;

        ImagePath = Uri.UnescapeDataString(files[0].Path.AbsolutePath);
    });
    
    public AsyncRelayCommand BrowseMusicCommand => new(async () =>
    {
        var files = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("MP3 Files")
                {
                    Patterns = ["*.mp3"]
                }
            ],
            SuggestedStartLocation = await MainWindow.Instance.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Music)
        });
        if (files.Count == 0) 
            return;

        MusicPath = Uri.UnescapeDataString(files[0].Path.AbsolutePath);
    });

    public DisplayData ToDisplayData()
    {
        return new DisplayData
        {
            Type = DisplayDataType,
            Text = DisplayDataType == DisplayData.DisplayDataType.Text ? Text : null,
            ImagePath = DisplayDataType == DisplayData.DisplayDataType.Image ? ImagePath : null,
            MusicPath = DisplayDataType == DisplayData.DisplayDataType.Music ? MusicPath : null,
        };
    }

    public void Dispose()
    {
        
    }
}