using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace JeopardyApp.Controls;

public partial class DisplayEditor : UserControl
{
    public static DisplayEditor Current { get; private set; } = null!; 
    
    public DisplayEditor()
    {
        InitializeComponent();
        Current = this;
        
    }
}