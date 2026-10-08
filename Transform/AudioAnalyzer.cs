using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Transform
{
    internal class AudioAnalyzer
    {
        // So apparently audio data needs to be split into much smaller chunks, which *should* be a
        // number, which is a power of 2. Standard chunk size in these cases is usually 4096, so I'll
        // do it like this at this point. 

        // If need to change look here(!!!):
        private const int chunk_size = 4096;

        public List<float[]> CreateFrames(float[] samples)
        {
            var frames = new List<float[]>(); // list to hold separate frames

            // Loop goes through big array, taking out chunks of 4096
            for (int i = 0; i + chunk_size < samples.Length; i += chunk_size)
            {
                float[] frame = new float[chunk_size]; // each frame is that size

                // Each iteration - copy 4096 samples from the main array are copied
                // Arguments - 1: source array, 2: index of source array on each it., 3: dest. array,
                // 4 - index to start at dest. array, 5 - size of elements going in each time
                Array.Copy(samples, i, frame, 0, chunk_size);

                // Add each frame to the big list of frames
                frames.Add(frame);
            }

            return frames;
        }

        // The core FFT algorithm, more specifically the Cooley-Tukey method, which reduces the
        // time complexity from O(N^2) to O(NlogN) by dviding the array in half continously.
        private Complex[] PerformFFT(Complex[] data)
        {
            int n = data.Length;

            // Base case: stops when array is broken into 1 single element
            if (n <= 1)
            {
                return data;
            }

            // Split array into two smaller arrays - 1 for even indexes of items and 1 for odd.
            Complex[] even = new Complex[n / 2];
            Complex[] odd = new Complex[n / 2];

            for (int i = 0; i < n / 2; i++)
            {
                even[i] = data[i * 2]; // for ex. index 0, 2, 4, 6...
                odd[i] = data[i * 2 + 1]; // for ex. index 1, 3, 5, 7...
            }

            // Call function recursively to halve array each time.
            Complex[] even_result = PerformFFT(even);
            Complex[] odd_result = PerformFFT(odd);

            // Then combine the processed halves back together 
            Complex[] result = new Complex[n];
            for (int k = 0; k < n / 2; k++)
            {
                // The 'twiddle' thing is basically the calculation of the rotation angle for a specific freq. bin
                double angle = -2.0 * Math.PI * k / n;
                Complex twiddle = new Complex(Math.Cos(angle), Math.Sin(angle));

                // The odd result is multiplied with the twiddle factor using the overloaded '*' operator.
                Complex t = twiddle * odd_result[k];

                // Both the lower and the upper half of the combined array get combined together, making it symmetrical.
                result[k] = even_result[k] + t;
                result[k + n / 2] = even_result[k] - t;
            }
            return result;
        }

        // The method that uses the FFT to process frames
        public double[] ProcessFramesFFT(float[] frame)
        {
            int total_samples = frame.Length;

            // Prepare the data for the Complex numbers, because the raw audio is just a float array.
            Complex[] complex_frame = new Complex[total_samples];
            for (int i = 0; i < total_samples; i++)
            {
                // Start with Real part, the Imaginary part starts at 0, because the sine wave is at an offset.
                complex_frame[i] = new Complex(frame[i], 0);
            }

            // Run the data through the FFT function
            Complex[] fft_result = PerformFFT(complex_frame);

            // Apply Nyquist limit rule again as like in DFT, to discard the second half because it is
            // basically a mirror of the calculated frequencies.
            int half_size = total_samples / 2;
            double[] freq_loud_score = new double[half_size];

            // Convert the Complex coordinates into the needed loudness score for each freq. bin
            for (int i = 0; i < half_size; i++)
            {
                // Use the Pythagorean theorem from the struct to find that score.
                freq_loud_score[i] = fft_result[i].Magnitude();
            }

            return freq_loud_score;
        }

        public List<Peak> GetFrequencyPeaks(List<double[]> spectrogram)
        {
            // List that will hold the target points - the loudest frequencies from each freame,
            List<Peak> frequency_peaks = new List<Peak>();

            // A noise threshold that filters out quiet/ambient sounds, can be adjusted
            double noise_threshold = 10.0;

            // Split the freq. bins into logarithmic sub-ranges to capture all necessary points in a frame
            // and not just a loud bass note or peak that overpowers the rest of the audio there.
            int[] freq_bands = { 0, 10, 20, 40, 80, 160, 512, 2048 };

            // Loop through each frame (time slice) of the audio file
            for (int i = 0; i < spectrogram.Count; i++)
            {
                // Get the array of 2048 freq. volume score for the exact (current) frame
                double[] current_frame = spectrogram[i];

                // Start looping through the defined freq. bands (0-10, 10-20, 20-40...)
                for (int j = 0; j < freq_bands.Length - 1; j++)
                {
                    // Bounds for each current band
                    int lower_bound = freq_bands[j];
                    int upper_bound = freq_bands[j + 1];

                    // Variables to hold the loudest volume and the spicific pitch (bin index) it is in
                    double max_amplitude = 0;
                    int best_bin = -1;

                    // Go through every individual freq. bin within this lower/upper bands
                    for (int bin = lower_bound; bin < upper_bound; bin++)
                    {
                        // Check if current bin is lowder than the loudest we have now
                        if (current_frame[bin] > max_amplitude)
                        {
                            // If yes, update variables
                            max_amplitude = current_frame[bin];
                            best_bin = bin;
                        }
                    }

                    // After going through the band, make a simple check ot see if loudest note is actually 
                    // louder than the threshold
                    if (max_amplitude > noise_threshold && best_bin != -1)
                    {
                        // Save the coordinates into the map for peaks.
                        frequency_peaks.Add(new Peak(i, best_bin, max_amplitude));
                    }
                }
            }

            return frequency_peaks;
        }

        public List<Fingerprint> GenerateFingerprints(List<Peak> peak_map)
        {
            // List to hold entries which will be searching for matches in db
            List<Fingerprint> fingerprints = new List<Fingerprint>();

            // The idea here is we are pairing notes, but they can't happen too far away from each other
            // so if say they are like 2 minutes apart, them matching would probably be random.
            // That's why it's capping is 50 frames to look into (the next 50).
            int target_zone_limit = 50;

            // If a given anchor has too many peaks inside the target zone, trying to pair them all, this
            // would probably cause memory to explode. So limiting it to looking at the first 4 targets
            // would probably be enough
            int max_pairs_per_anchor = 4;

            // Select an anchor (starting peak)
            for (int i = 0; i < peak_map.Count; i++)
            {
                Peak anchor = peak_map[i];
                int pairs_found = 0; // tracks number of targets found with this anchor

                // This inner loop looks ahead in the list to find targets in 'future'
                for (int j = i + 1; j < peak_map.Count; j++)
                {
                    Peak target = peak_map[j];
                    
                    // Counts the number of frames between anchor and target
                    int time_delta = target.TimeFrameIndex - anchor.TimeFrameIndex;

                    // If there are two peaks happening in the exact same time frame, we skip them
                    if (time_delta <= 0)
                    {
                        continue;
                    }
                    // If a peak is found which is more than 50 frames away, we stop searching and move
                    // to next anchor.
                    if (time_delta > target_zone_limit)
                    {
                        break;
                    }

                    // This is the pairing string we get when we combine the anchor's bin, the target 
                    // (following) bin and the time, which is between them.
                    string hash = $"{anchor.FrequencyBin}|{target.FrequencyBin}|{time_delta}";

                    // We save this as a kind of hash which'd be used to search for matches, also save the
                    // original time of the anchor to know where the match occurs
                    fingerprints.Add(new Fingerprint(hash, anchor.TimeFrameIndex));
                    pairs_found++;

                    // If there are more than 4 pairs found for the current anchor, stop the search, 
                    // because we know enough about this point in time.
                    if (pairs_found >= max_pairs_per_anchor)
                    {
                        break;
                    }
                }
            }
            return fingerprints;
        }
    }
}
