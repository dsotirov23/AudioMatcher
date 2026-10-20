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

                Console.WriteLine("\n Simulating a Noisy Audio QUery");

                // Slice audio sample by 3 seconds while starting at 5 seconds into original audio
                int sample_rate = ext_data.SampleRate;
                int query_start_sample = sample_rate * 5;
                int query_length = sample_rate * 3;

                if (query_start_sample + query_length > ext_data.Samples.Length)
                {
                    query_start_sample = 0;
                    query_length = Math.Min(sample_rate * 3, ext_data.Samples.Length);
                }

                float[] gd_query = new float[query_length];
                Array.Copy(ext_data.Samples, query_start_sample, gd_query, 0, query_length);

                // Degrade the good sample by injecting it with 15% random white noise (mathematical numbers in the data)
                // This will kind of make a lot of audio peaks which are weak to disappear when doing the FFT, but the core
                // peak map of the song, which'd be needed the most for matches should work.
                float[] noisy_query = InjectNoise(gd_query, 0.15f);
                Console.WriteLine("Injected 15% static noise which is artificial into query");

                var query_frames = analyzer.CreateFrames(noisy_query);
                List<double[]> query_spectrogram = new List<double[]>();

                foreach (var frame in query_frames)
                {
                    query_spectrogram.Add(analyzer.ProcessFramesFFT(frame));
                }

                List<Peak> query_peaks = analyzer.GetFrequencyPeaks(query_spectrogram);
                List<Fingerprint> query_fingerprints = hasher.GenerateFingerprints(query_peaks);
                Console.WriteLine($"Extracted {query_fingerprints.Count} fingerprints from the degraded clip.");

                // Pass that generated hash for the fake audio into the 'database' and use the matching algorithms
                TrackMatcher matcher = new TrackMatcher();
                matcher.FindBestMatch(query_fingerprints, db);
            }
            else
            {
                Console.WriteLine("\nError! Extraction Failed."); // Test done with file path and got correct message.
            }
        }

        // Method which changes the raw PCM amplitude values of audio array
        static float[] InjectNoise(float[] original_samples, float noise_intensity)
        {
            Random rand = new Random();
            float[] noisy_samples = new float[original_samples.Length];

            for (int i = 0; i < original_samples.Length; i++)
            {
                // Generate a random amplitude shift between -1.0 and 1.0 (which can scale based on noise_intensity)
                // This simulates chaotic, multi-freq. noise waves to test wheteher algorithm works if sound has 'white noise'
                float static_noise = (float)((rand.NextDouble() * 2 - 1) * noise_intensity);

                float combined_signal = original_samples[i] + static_noise;

                // Digital audio samples are strictyl between -1.0 and 1.0 floating-points.If injected noise pushes
                // amplitude outside the bounds, we cut it back, so that it stays inside the bounds which PCM has.
                if (combined_signal > 1.0f)
                {
                    noisy_samples[i] = 1.0f;
                }
                else if (combined_signal < -1.0f)
                {
                    noisy_samples[i] = -1.0f;
                }
                else
                {
                    noisy_samples[i] = combined_signal;
                }
            }
            return noisy_samples;
        }
    }
}
