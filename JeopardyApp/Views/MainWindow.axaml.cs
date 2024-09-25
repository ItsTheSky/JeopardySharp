using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using JeopardyApp.Models;
using JeopardyApp.ViewModels;

namespace JeopardyApp.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; } = null!;
    public MainWindowViewModel ViewModel => (MainWindowViewModel) DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        DataContext = new MainWindowViewModel();
        
        var cells = new List<List<Cell>>();
        for (var i = 0; i < 5; i++)
        {
            var cellList = new List<Cell>();
            for (var j = 0; j < 5; j++)
            {
                cellList.Add(new Cell
                {
                    Score = (j + 1) * 100,
                    Question = new DisplayData
                    {
                        Text = $"Question {i + 1} {j + 1}"
                    },
                    Answer = new DisplayData
                    {
                        Text = $"Answer {i + 1} {j + 1}"
                    }
                });
            }
            cells.Add(cellList);
        }
        
        var categories = new List<Category>();
        for (var i = 0; i < 5; i++)
        {
            categories.Add(new Category
            {
                Title = $"Category {i + 1}",
                Cells = cells[i]
            });
        }
        
        ViewModel.Board = new Board
        {
            Title = "Sample Board",
            Categories = new ObservableCollection<Category>(categories)
        };
        
        // Debug board
        Console.WriteLine(ViewModel.Board.Categories[0].Cells[0].Question.Text);
        
        ViewModel.AddTeam();
    }
    
}