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
    }
}
