using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JeopardyApp.Models.Converters;

public class DisplayDataConverter : JsonConverter<DisplayData>
{
    public override DisplayData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        DisplayData displayData = new DisplayData();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return displayData;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "type":
                    displayData.Type = reader.GetString() switch
                    {
                        "text" => DisplayData.DisplayDataType.Text,
                        "image" => DisplayData.DisplayDataType.Image,
                        _ => throw new JsonException()
                    };
                    break;
                case "text":
                    displayData.Text = reader.GetString();
                    break;
                case "imagePath":
                    displayData.ImagePath = reader.GetString();
                    break;
                default:
                    throw new JsonException();
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, DisplayData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", value.Type switch
        {
            DisplayData.DisplayDataType.Text => "text",
            DisplayData.DisplayDataType.Image => "image",
            _ => throw new JsonException()
        });

        if (value.Type == DisplayData.DisplayDataType.Text)
            writer.WriteString("text", value.Text);
        else if (value.Type == DisplayData.DisplayDataType.Image)
            writer.WriteString("imagePath", value.ImagePath);

        writer.WriteEndObject();
    }
}