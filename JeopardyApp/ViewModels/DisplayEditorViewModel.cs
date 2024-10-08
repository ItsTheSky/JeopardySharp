using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyApp.Models;
using JeopardyApp.Views;

namespace JeopardyApp.ViewModels;

public partial class DisplayEditorViewModel : ObservableObject
{
    public DisplayEditorViewModel(DisplayData original)
    {
        SelectedType = original.Type == DisplayData.DisplayDataType.Image ? 1 : 0;
        
        Text = original.Type == DisplayData.DisplayDataType.Text ? original.Text : null;
        ImagePath = original.Type == DisplayData.DisplayDataType.Image ? original.ImagePath : null;
    }

    private int _selectedType;
    public int SelectedType
    {
        get => _selectedType;
        set
        {
            SetProperty(ref _selectedType, value);
            DisplayDataType = value == 0 ? DisplayData.DisplayDataType.Text : DisplayData.DisplayDataType.Image;
        }
    }

    [ObservableProperty] private DisplayData.DisplayDataType _displayDataType;
    
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

    [ObservableProperty] private bool _hasImageSelected;

    [ObservableProperty] private Bitmap? _bitmap;

    public AsyncRelayCommand BrowseImageCommand => new AsyncRelayCommand(async () =>
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
    
    public DisplayData ToDisplayData()
    {
        return new DisplayData
        {
            Type = DisplayDataType,
            Text = DisplayDataType == DisplayData.DisplayDataType.Text ? Text : null,
            ImagePath = DisplayDataType == DisplayData.DisplayDataType.Image ? ImagePath : null
        };
    }
}