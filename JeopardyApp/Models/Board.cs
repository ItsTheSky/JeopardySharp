using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyApp.Models;

[Serializable]
public partial class Board : ObservableObject
{
    
    [ObservableProperty] private ObservableCollection<Category> _categories;
    [ObservableProperty] private string _title;
    
    public int GetRows() => Categories.Max(category => category.Cells.Count);
    public int GetColumns() => Categories.Count;
    
}