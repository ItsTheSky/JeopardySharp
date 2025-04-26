using System.Media;
using Avalonia.Media.Imaging;
using JeopardyApp.Views;
using NAudio.Wave;

namespace JeopardyApp.Models;

public class DisplayData
{
    public DisplayDataType Type { get; set; }
    
    public string? Text { get; set; }
    public string? ImagePath { get; set; }
    public string? MusicPath { get; set; }
    
    private Bitmap _image = null!;
    public Bitmap Image
    {
        set => _image = value;
        get
        {
            if (_image == null! && ImagePath != null!)
                _image = new Bitmap(ImagePath);
            
            return _image!;
        }
    }
    
    private Mp3FileReader _music = null!;
    public Mp3FileReader Music
    {
        set => _music = value;
        get
        {
            if (_music == null! && MusicPath != null!)
                _music = new Mp3FileReader(MusicPath);
            
            return _music!;
        }
    }

    public enum DisplayDataType
    {
        Text,
        Image,
        Music
    }
    
}