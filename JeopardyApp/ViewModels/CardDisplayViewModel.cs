using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeopardyApp.Views;

namespace JeopardyApp.ViewModels;

public partial class CardDisplayViewModel : ObservableObject
{
    
    [ObservableProperty] private ICommand _actionButtonCommand;
    [ObservableProperty] private string _actionButtonText;
    [ObservableProperty] private ICommand _closeButtonCommand;

}