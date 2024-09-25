using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyApp.Models;

public partial class Team : ObservableObject
{

    [ObservableProperty] private string _name;
    [ObservableProperty] private int _score;

}