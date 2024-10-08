using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using JeopardyApp.Controls;
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
                Cells = new ObservableCollection<Cell>(cells[i])
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
        
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Escape)
            {
                ViewModel.IsQuestionShowing = false;
                ViewModel.SelectedCell = null;
            }
        };
    }
    
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedCell))
        {
            if (ViewModel.SelectedCell != null)
            {
                AnimateCardAppearance(ViewModel.SelectedCell);
            }
        }
        else if (e.PropertyName == nameof(ViewModel.IsQuestionShowing) && !ViewModel.IsQuestionShowing)
        {
            AnimateCardDisappearance();
        }
    }

    private void AnimateCardAppearance(Cell selectedCell)
    {
        var cardControl = this.FindControl<CardDisplayControl>("CardDisplayControl");
        if (cardControl?.RenderTransform is not TransformGroup transformGroup)
            return;

        var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
        var translateTransform = transformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();

        if (scaleTransform == null || translateTransform == null)
            return;

        // Set initial position and scale
        scaleTransform.ScaleX = scaleTransform.ScaleY = 0.1;
        translateTransform.X = translateTransform.Y = 0;

        // Animate to final position and scale
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0.1),
                        new Setter(ScaleTransform.ScaleYProperty, 0.1)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d)
                    },
                    KeySpline = new KeySpline(0.25, 0.1, 0.25, 1)
                }
            }
        };

        animation.RunAsync(cardControl);
    }

    private void AnimateCardDisappearance()
    {
        var cardControl = this.FindControl<CardDisplayControl>("CardDisplayControl");
        if (cardControl?.RenderTransform is not TransformGroup transformGroup)
            return;

        var scaleTransform = transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();

        if (scaleTransform == null)
            return;

        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.2),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0.1),
                        new Setter(ScaleTransform.ScaleYProperty, 0.1)
                    },
                    KeySpline = new KeySpline(0.25, 0.1, 0.25, 1)
                }
            }
        };

        animation.RunAsync(cardControl);
    }
    
}