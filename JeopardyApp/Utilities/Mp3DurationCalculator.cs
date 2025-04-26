using System;
using System.IO;

public class Mp3DurationCalculator
{
    private const int HEADER_SIZE = 10;
    private static readonly int[] bitRates = {
        0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0
    };
    private static readonly int[] sampleRates = {
        44100, 48000, 32000, 0
    };

    /// <summary>
    /// Gets the duration of an MP3 stream in seconds
    /// </summary>
    /// <param name="stream">The MP3 stream to analyze</param>
    /// <returns>Duration in seconds, or -1 if the duration cannot be determined</returns>
    public static double GetDuration(Stream stream)
    {
        try
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            // Read first 10 bytes to find the first frame header
            byte[] headerBuffer = new byte[HEADER_SIZE];
            stream.Read(headerBuffer, 0, HEADER_SIZE);

            // Find the sync word (11 bits set to 1)
            int offset = FindSyncWord(headerBuffer);
            if (offset < 0)
                return -1;

            // Read frame header
            int header = (headerBuffer[offset] << 24) |
                        (headerBuffer[offset + 1] << 16) |
                        (headerBuffer[offset + 2] << 8) |
                        headerBuffer[offset + 3];

            // Extract header information
            int bitRateIndex = (header >> 12) & 0x0F;
            int sampleRateIndex = (header >> 10) & 0x03;
            int paddingBit = (header >> 9) & 0x01;
            int channelMode = (header >> 6) & 0x03;

            // Get bit rate and sample rate from lookup tables
            int bitRate = bitRates[bitRateIndex] * 1000; // Convert to bits per second
            int sampleRate = sampleRates[sampleRateIndex];

            if (bitRate == 0 || sampleRate == 0)
                return -1;

            // Calculate file size and frame size
            long fileSize = stream.Length;
            int frameSize = ((144 * bitRate) / sampleRate) + paddingBit;

            // Calculate duration
            double duration = (fileSize * 8.0) / bitRate;

            // Restore original stream position
            stream.Position = originalPosition;

            return duration;
        }
        catch (Exception)
        {
            return -1;
        }
    }

    /// <summary>
    /// Finds the MP3 sync word in the header buffer
    /// </summary>
    private static int FindSyncWord(byte[] buffer)
    {
        for (int i = 0; i < buffer.Length - 1; i++)
        {
            if (buffer[i] == 0xFF && (buffer[i + 1] & 0xE0) == 0xE0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Gets the duration of an MP3 file in seconds
    /// </summary>
    /// <param name="filePath">Path to the MP3 file</param>
    /// <returns>Duration in seconds, or -1 if the duration cannot be determined</returns>
    public static double GetDuration(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            return GetDuration(fs);
        }
    }
}