using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;

namespace AudioMatcher.Media
{
    internal class AudioDecoder
    {
        public void ReadAudioFile(string file_path)
        {
            try
            {
                using var reader = new AudioFileReader(file_path);

                Console.WriteLine($"Successfully loaded: {file_path}");
                Console.WriteLine($"Sample Rate: {reader.WaveFormat.SampleRate} Hz");
                Console.WriteLine($"Channels: {reader.WaveFormat.Channels}");
                Console.WriteLine($"Duration: {reader.TotalTime.TotalSeconds:F2} seconds");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading audio file: {ex.Message}");
            }
        }
    }
}
