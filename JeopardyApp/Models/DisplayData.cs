using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;

namespace JeopardyApp.Models;

public class DisplayData
{
    public DisplayDataType Type { get; set; }
    
    public string? Text { get; set; }
    public string? ImagePath { get; set; }
    
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

    public enum DisplayDataType
    {
        Text,
        Image
    }
    
}