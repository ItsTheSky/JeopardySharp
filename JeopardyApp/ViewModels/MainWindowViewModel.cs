using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using JeopardyApp.Controls;
using JeopardyApp.Models;
using JeopardyApp.Models.Converters;
using JeopardyApp.Views;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.SymbolIcon;

namespace JeopardyApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private Dictionary<Cell, int> _scorePerRow = new();

    [ObservableProperty] private JeoFile? _openedFile;
    [ObservableProperty] private bool _editMode;
    
    [ObservableProperty] private Board _board;
    [ObservableProperty] private ObservableCollection<Team> _teams = new();
    
    public async Task OpenFile()
    {
        Console.WriteLine("Opening file");
        var files = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("JeoSharp Files")
                {
                    Patterns = ["*.jeosharp"]
                }
            ],
            SuggestedStartLocation = await MainWindow.Instance.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
        });
        if (files.Count == 0) 
            return;
        
        var file = files[0];
        var path = Uri.UnescapeDataString(file.Path.AbsolutePath);
        try
        {
            OpenedFile = await JeoFile.LoadFile(path);
            Board = OpenedFile.Board;
        }
        catch (Exception e)
        {
            await ShowInformationMessage("Error Opening File", e.Message);
            Console.WriteLine(e);
            return;
        }
        
        ResetGame();
        await ShowInformationMessage("File Opened", "The file has been opened successfully.");
    }

    private string? _lastSaveFolder;
    public async Task SaveFile(bool showSuccess = true)
    {
        string filePath;
        if (OpenedFile == null || string.IsNullOrEmpty(OpenedFile.FilePath))
        {
            var startFolder = _lastSaveFolder == null 
                ? await MainWindow.Instance.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                : await MainWindow.Instance.StorageProvider.TryGetFolderFromPathAsync(_lastSaveFolder);
            var result = await MainWindow.Instance.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                DefaultExtension = "jeosharp",
                FileTypeChoices = [
                    new FilePickerFileType("JeoSharp Files")
                    {
                        Patterns = ["*.jeosharp"]
                    }
                ],
                SuggestedFileName = "JeopardyBoard",
                SuggestedStartLocation = startFolder
            });
            if (result == null) 
                return;
            
            filePath = result.Path.AbsolutePath;
        }
        else filePath = OpenedFile.FilePath;
        
        _lastSaveFolder = Path.GetDirectoryName(filePath);
        await JeoFile.SaveFile(filePath, Board);
        if (showSuccess) 
            await ShowInformationMessage("File Saved", "The file has been saved successfully.");
    }

    public void DeleteRow(Cell cell)
    {
        var row = Board.Categories[0].Cells.ToList().IndexOf(cell);
        foreach (var category in Board.Categories)
            category.Cells.RemoveAt(row);
    }

    public void ApplyRowScore(object param)
    {
        var values = (object[]) param;
        var cell = (Cell) values[0];
        var score = (int) (decimal) values[1];
        
        var row = Board.Categories[0].Cells.ToList().IndexOf(cell);
        foreach (var category in Board.Categories)
            category.Cells[row].Score = score;
    }
    
    [ObservableProperty] private SymbolIcon _saveStatusIcon = new SymbolIcon { Symbol = Symbol.SaveSync, FontSize = 24 };

    [RelayCommand]
    public async Task QuickSave()
    {
        if (OpenedFile == null || string.IsNullOrEmpty(OpenedFile.FilePath))
        {
            await ShowInformationMessage("Error Saving", "You must open a file before you can save.");
            return;
        }
        
        await SaveFile(false);
        SaveStatusIcon = new SymbolIcon { Symbol = Symbol.Checkmark, FontSize = 24 };
        await Task.Delay(2000);
        SaveStatusIcon = new SymbolIcon { Symbol = Symbol.SaveSync, FontSize = 24 };
    }

    #region Methods

    public void ResetGame()
    {
        foreach (var cell in Board.Categories.SelectMany(category => category.Cells))
            cell.IsAnswered = false;
    }
    
    [RelayCommand]
    public void AddRow()
    {
        foreach (var category in Board.Categories)
        {
            category.Cells.Add(new Cell
            {
                Score = 0,
                Question = new DisplayData(),
                Answer = new DisplayData()
            });
        }
    }
    
    public void AddTeam()
    {
        Teams.Add(new Team()
        {
            Name = "Team " + (Teams.Count + 1),
            Score = 0
        });
    }
    
    public void RemoveTeam(Team team)
    {
        Teams.Remove(team);
    }
    
    [RelayCommand]
    public void AddCategory()
    {
        var cells = new List<Cell>();
        for (var i = 0; i < Board.GetRows(); i++)
        {
            cells.Add(new Cell
            {
                Score = 0,
                Question = new DisplayData(),
                Answer = new DisplayData()
            });
        }
        
        Board.Categories.Add(new Category
        {
            Title = "Category " + (Board.Categories.Count + 1),
            Cells = new ObservableCollection<Cell>(cells)
        });
    }
    
    public void RemoveCategory(Category category)
    {
        Board.Categories.Remove(category);
    }
    
    public async Task EditCategory(Category category)
    {
        var newName = await AskForString("Enter new category name");
        if (newName != null)
            category.Title = newName;
    }
    
    public void DecreaseScore(Team team)
    {
        if (SelectedCell == null) 
            team.Score -= 1;
        else 
            team.Score -= SelectedCell.Score;
    }
    
    public void IncreaseScore(Team team)
    {
        if (SelectedCell == null) 
            team.Score += 1;
        else 
            team.Score += SelectedCell.Score;
    }
    
    public async Task EditTeam(Team team)
    {
        var newName = await AskForString("Enter new team name");
        if (newName != null)
            team.Name = newName;
    }

    public async Task EditCell(Cell cell, bool question)
    {
        var context = new DisplayEditorViewModel(question ? cell.Question : cell.Answer);
        var dialog = new ContentDialog
        {
            Content = new DisplayEditor
            {
                DataContext = context
            },
            Title = "Edit Question",
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel"
        };
        
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (question) 
                cell.Question = context.ToDisplayData();
            else 
                cell.Answer = context.ToDisplayData();
        }
    }

    public async Task EditQuestion(Cell cell)
    {
        await EditCell(cell, true);
    }
    
    public async Task EditAnswer(Cell cell)
    {
        await EditCell(cell, false);
    }

    #endregion

    #region Commands
    
    public RelayCommand<Team> DecreaseScoreCommand => new(DecreaseScore!);
    public RelayCommand<Team> IncreaseScoreCommand => new(IncreaseScore!);
    public RelayCommand<Team> RemoveTeamCommand => new(RemoveTeam!);
    public AsyncRelayCommand<Team> EditTeamCommand => new(EditTeam!);
    public RelayCommand AddTeamCommand => new(AddTeam!);
    
    public RelayCommand<Category> RemoveCategoryCommand => new(RemoveCategory!);
    public AsyncRelayCommand<Category> EditCategoryCommand => new(EditCategory!);
    
    public AsyncRelayCommand OpenFileCommand => new(OpenFile!);
    public AsyncRelayCommand SaveFileCommand => new(() => SaveFile());
    public RelayCommand ResetGameStateCommand => new(ResetGame!);
    
    public RelayCommand<Cell> DeleteRowCommand => new(DeleteRow!);
    public RelayCommand<object> ApplyRowScoreCommand => new(ApplyRowScore!);
    
    public AsyncRelayCommand<Cell> EditCellQuestionCommand => new(EditQuestion!);
    public AsyncRelayCommand<Cell> EditCellAnswerCommand => new(EditAnswer!);
    
    #endregion

    #region Utilities

    public async Task<string?> AskForString(string title)
    {
        var textbox = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        var dialog = new ContentDialog()
        {
            Title = title,
            Content = textbox,
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel"
        };
        
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? textbox.Text : null;
    }
    
    public async Task ShowInformationMessage(string title, string message)
    {
        var dialog = new ContentDialog()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK"
        };
        
        await dialog.ShowAsync();
    }
    
    #endregion

    #region Card Reveal

    [ObservableProperty] private Cell? _selectedCell;
    [ObservableProperty] private bool _isQuestionShowing;
    [ObservableProperty] private bool _isAnswerRevealed;
    
    public RelayCommand<Cell> SelectCellCommand => new(SelectCell!);
    public RelayCommand RevealAnswerCommand => new(RevealAnswer!);
    public RelayCommand CloseCardCommand => new(CloseCard!);

    private void SelectCell(Cell cell)
    {
        if (!EditMode && !cell.IsAnswered)
        {
            SelectedCell = cell;
            IsAnswerRevealed = false;
            MainWindow.Instance.CardDisplayControl.Refresh();

            IsQuestionShowing = true;
        }
    }

    private void RevealAnswer()
    {
        IsAnswerRevealed = true;
    }

    private void CloseCard()
    {
        if (SelectedCell != null)
        {
            SelectedCell.IsAnswered = true;
        }
        SelectedCell = null;
        IsQuestionShowing = false;
        IsAnswerRevealed = false;
    }

    #endregion
    
}