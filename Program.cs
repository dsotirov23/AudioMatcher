using System;
using System.Diagnostics;
using AudioMatcher.Media;
using AudioMatcher.Transform;

namespace AudioMatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Audio Matcher");

            string test_file_path = @"C:\Users\Dimitar\Downloads\871200__cat-fox_alex__slap-house-horn-stab.wav";

            var decoder = new AudioDecoder();

            AudioData ext_data = decoder.ReadAudioFile(test_file_path);

            if (ext_data != null)
            {
                // Some properties for info
                Console.WriteLine("Data extraction phase:");
                Console.WriteLine($"Sample Rate: {ext_data.SampleRate}");
                Console.WriteLine($"Samples extracted: {ext_data.Samples.Length}");

                // check if chunking logic works:
                Console.WriteLine("\n Chunking logic:");
                var analyzer = new AudioAnalyzer();
                var frames = analyzer.CreateFrames(ext_data.Samples);

                Console.WriteLine($"Split audio into {frames.Count} individual frames.");
                // added just to check example file
                Console.WriteLine("\nFor reference number of chunks should be sample size lenght/4096 or in this case 890182/4096=" + 890182 / 4096);
                Console.WriteLine($"Each frame contains {frames[0].Length} samples.");

                Console.WriteLine("\n DFT test:");

                Stopwatch stopwatch = new Stopwatch();

                // Quickly check time bottleneck in DFT
                int[] batch_sizes = { 1, 10, 50, Math.Min(100, frames.Count) };

                foreach (int batch in batch_sizes)
                {
                    stopwatch.Restart();

                    for (int i = 0; i < batch; i++)
                    {
                        double[] results = analyzer.ProcessFramesDFT(frames[i]);
                    }

                    stopwatch.Stop();

                    double audio_durations_ms = (batch * 4096.0 / ext_data.SampleRate) * 1000;

                    Console.WriteLine($"Processed {batch} frames ({audio_durations_ms:F0} ms of audio)");
                    Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                }
            }
            else
            {
                Console.WriteLine("\nError! Extraction Failed."); // Test done with file path and got correct message.
            }
        }
    }
}
