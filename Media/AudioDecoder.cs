using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;

namespace AudioMatcher.Media
{
    internal class AudioDecoder
    {

        // Function to read audio files
        public AudioData ReadAudioFile(string file_path)
        {
            try
            {
                // Read file safely
                using var reader = new AudioFileReader(file_path);
                int sample_rate = reader.WaveFormat.SampleRate;

                // The AudioFileReader converts audio to 32-bit floats (each one is 4 bytes), meaning ->
                // the total number of samples from a snippet is the file's byte lenght divided by 4.
                int total_samples = (int)(reader.Length / 4);

                // This might break for larger files, so will need to add buffering in future.
                // !!!

                // Array to hold all samples from the file
                float[] all_samples = new float[total_samples];

                // ISampleProvider turns audio files into appropriate data (into float numbers, not just raw bytes)
                // Floats range between -1.0 and 1.0
                ISampleProvider sampleProvider = reader;
                sampleProvider.Read(all_samples);

                return new AudioData(all_samples, sample_rate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading audio file: {ex.Message}");
                return null;
            }
        }
    }
}
