﻿using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JeopardyApp.Models;

[Serializable]
public partial class Cell : ObservableObject
{

    [ObservableProperty, JsonPropertyName("score")] private int _score;
    
    [ObservableProperty, JsonPropertyName("question")] private DisplayData _question;
    [ObservableProperty, JsonPropertyName("answer")] private DisplayData _answer;
    
    [ObservableProperty, JsonIgnore] private bool _isAnswered;

}