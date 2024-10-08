using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JeopardyApp.Models;
using JeopardyApp.ViewModels;
using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using JeopardyApp.Views;

namespace JeopardyApp.Controls;

public partial class CardDisplayControl : UserControl
{
    public CardDisplayControl()
    {
        InitializeComponent();
        DataContext = new CardDisplayViewModel();
    }
    
    private CardDisplayViewModel ViewModel => (CardDisplayViewModel) DataContext!;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UpdateContent();
    }

    private void UpdateContent()
    {
        if (MainWindow.Instance.ViewModel.SelectedCell != null)
        {
            var displayData = MainWindow.Instance.ViewModel.IsAnswerRevealed
                ? MainWindow.Instance.ViewModel.SelectedCell.Answer
                : MainWindow.Instance.ViewModel.SelectedCell.Question;
            
            if (displayData.Type == DisplayData.DisplayDataType.Text)
            {
                ContentDisplay.Content = new TextBlock
                {
                    Text = displayData.Text,
                    FontSize = 30,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                };
            }
            else if (displayData.Type == DisplayData.DisplayDataType.Image)
            {
                ContentDisplay.Content = new Image
                {
                    Source = new Bitmap(displayData.ImagePath),
                    Margin = new Thickness(20),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.Both
                };
                RenderOptions.SetBitmapInterpolationMode((Image) ContentDisplay.Content, BitmapInterpolationMode.HighQuality);
            }
        }
    }

    public void Refresh()
    {
        ViewModel.ActionButtonCommand = new RelayCommand(() =>
        {
            if (MainWindow.Instance.ViewModel.IsAnswerRevealed)
            {
                MainWindow.Instance.ViewModel.IsAnswerRevealed = false;
                ViewModel.ActionButtonText = "Show Answer";
            }
            else
            {
                MainWindow.Instance.ViewModel.IsAnswerRevealed = true;
                MainWindow.Instance.ViewModel.SelectedCell!.IsAnswered = true;
                ViewModel.ActionButtonText = "Show Question";
            }
            UpdateContent();
        });
        ViewModel.ActionButtonText = MainWindow.Instance.ViewModel.IsAnswerRevealed ? "Show Question" : "Show Answer";
        ViewModel.CloseButtonCommand = MainWindow.Instance.ViewModel.CloseCardCommand;
        
        UpdateContent();
    }
}