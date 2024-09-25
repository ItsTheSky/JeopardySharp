using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyApp.Models;

[Serializable]
public partial class Category : ObservableObject
{
    
    [ObservableProperty, JsonPropertyName("title")] private string _title;
    [ObservableProperty, JsonPropertyName("cells")] private List<Cell> _cells;
    
}