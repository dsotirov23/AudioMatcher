using System;
using System.Diagnostics;
using AudioMatcher.Hashing;
using AudioMatcher.Media;
using AudioMatcher.Transform;
using AudioMatcher.Database;
using AudioMatcher.Matching;

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

                // List to hold spectogram data
                List<double[]> full_spectrogram = new List<double[]>();

                // Loops through frames to apply FFT
                for (int i = 0; i < frames.Count; i++)
                {
                    double[] frame_results = analyzer.ProcessFramesFFT(frames[i]);
                    full_spectrogram.Add(frame_results);
                }

                Console.WriteLine("\n Extracting Frequency Peaks");
                List<Peak> peak_map = analyzer.GetFrequencyPeaks(full_spectrogram);
                Console.WriteLine($"Total peaks: {peak_map.Count}");

                Console.WriteLine("\nSample of first 25 peaks:");
                for (int i = 0; i < Math.Min(25, peak_map.Count); i++)
                {
                    Peak p = peak_map[i];
                    Console.WriteLine($"Frame: {p.TimeFrameIndex:D3}; Bin {p.FrequencyBin,4}; Loudness: {p.Amplitude:F2}");
                }

                Console.WriteLine("\n Generating Fingerprints");

                CombinatorialHasher hasher = new CombinatorialHasher();
                List<Fingerprint> fingerprints = hasher.GenerateFingerprints(peak_map);

                Console.WriteLine($"Total fingerprints generated: {fingerprints.Count}");

                Console.WriteLine("\nSample of first 25 fingerprints");
                for (int i = 0; i < Math.Min(25, fingerprints.Count); i++)
                {
                    Fingerprint f = fingerprints[i];
                    Console.WriteLine($"Hash String: [{f.Hash}] | Anchor Time: Frame {f.AnchorTimeFrame}");
                }

                Console.WriteLine("\n Building Temp Database");
                FingerprintDatabase db = new FingerprintDatabase();

                // Save track in memory 
                db.AddTrack("871200__cat-fox_alex__slap-house-horn-stab", fingerprints);
                Console.WriteLine($"Successfully loaded with {db.GetUniqueHashCount()} unique hashes.");

                Console.WriteLine("\n Audio Query");
                List<Peak> query_peaks = new List<Peak>();

                // Test matching without using real input yet, so simulate a snippet starting from frame 10 to 50
                int start_frame = 10;
                int end_frame = 50;

                foreach(Peak p in peak_map)
                {
                    if (p.TimeFrameIndex >= start_frame && p.TimeFrameIndex < end_frame)
                    {
                        // Since an audio recording will always start at 0, subtract the starting frame of the audio from the
                        // original time index, so that the index of both starts at 0. They should match exactly 10 frames
                        query_peaks.Add(new Peak(p.TimeFrameIndex - start_frame, p.FrequencyBin, p.Amplitude));
                    }
                }

                // Run hasher on the fake recording used for testing
                List<Fingerprint> query_fingerprints = hasher.GenerateFingerprints(query_peaks);
                Console.WriteLine($"Generated {query_fingerprints.Count} fingerprints from the 'fake' audio for testing.");

                // Pass that generated hash for the fake audio into the 'database' and use the matching algorithms
                TrackMatcher matcher = new TrackMatcher();
                matcher.FindBestMatch(query_fingerprints, db);
            }
            else
            {
                Console.WriteLine("\nError! Extraction Failed."); // Test done with file path and got correct message.
            }
        }
    }
}
