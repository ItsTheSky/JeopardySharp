using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using JeopardyApp.Models.Converters;
using NAudio.Wave;

namespace JeopardyApp.Models;

public class JeoFile
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        Converters = { new DisplayDataConverter() }
    };

    public Board Board { get; private set; }
    public Dictionary<string, Bitmap> Images { get; private set; } = [];
    public Dictionary<string, Mp3FileReader> Musics { get; private set; } = [];
    public string FilePath { get; set; } = "";

    public static async Task<JeoFile> LoadFile(string filePath = "")
    {
        var legacyFile = TryLoadLegacyFile(filePath);
        if (legacyFile != null)
            return legacyFile;
        
        using var archive = ZipFile.OpenRead(filePath);
        Board? board = null;
        var images = new Dictionary<string, Bitmap>();

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".json"))
            {
                await using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                board = JsonSerializer.Deserialize<Board>(json, JsonOptions);
            }
            else if (entry.FullName.EndsWith(".png"))
            {
                await using (var stream = entry.Open())
                {
                    using var reader = new BinaryReader(stream);
                    var bytes = reader.ReadBytes((int) entry.Length);
                    var bitmap = new Bitmap(new MemoryStream(bytes));
                    images.Add(entry.FullName, bitmap);
                }
            }
        }
        
        if (board == null)
            throw new InvalidDataException("No board data found in the file.");
        
        // Post-process images
        for (var catId = 0; catId < board.Categories.Count; catId++)
        {
            var category = board.Categories[catId];
            for (var cellId = 0; cellId < category.Cells.Count; cellId++)
            {
                var cell = category.Cells[cellId];
                DisplayData[] datas = [cell.Question, cell.Answer];
                for (var i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    if (data.Type != DisplayData.DisplayDataType.Image)
                        continue;
                    var id = $"{catId}-{cellId}-{i}.png";
                    data.Image = images[id];
                }
            }
        }

        return new JeoFile
        {
            Board = board,
            Images = images,
            FilePath = filePath
        };
    }

    private static JeoFile? TryLoadLegacyFile(string filePath)
    {
        // legacy are plain JSON files of the board
        Board? board;
        try
        {
            var json = File.ReadAllText(filePath);
            board = JsonSerializer.Deserialize<Board>(json, JsonOptions);
        }
        catch (Exception e)
        {
            return null;
        }
        return board == null ? null : new JeoFile { Board = board };
    }

    public static async Task SaveFile(string filePath, Board board)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        
        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        var boardEntry = archive.CreateEntry("board.json");
        await using (var stream = boardEntry.Open())
        {
            await JsonSerializer.SerializeAsync(stream, board, JsonOptions);
        }

        for (var catId = 0; catId < board.Categories.Count; catId++)
        {
            var category = board.Categories[catId];
            for (var cellId = 0; cellId < category.Cells.Count; cellId++)
            {
                var cell = category.Cells[cellId];
                DisplayData[] datas = [cell.Question, cell.Answer];
                for (var i = 0; i < datas.Length; i++)
                {
                    var data = datas[i];
                    if (data.Type != DisplayData.DisplayDataType.Image)
                        continue;
                    var id = $"{catId}-{cellId}-{i}.png";
                    var entry = archive.CreateEntry(id);
                    await using var stream = entry.Open();
                    data.Image.Save(stream);
                }
            }
        }
    }
}