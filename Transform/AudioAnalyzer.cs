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

        // A Discrete Fourier Transform implemenatation, which is esentially the algorigthm that stays 
        // below the FFT. Implementing it for the sake of understanding how it'd work, it's not optimal,
        // because its complexity is O(N^2), while an FFT implementation should be O(N log N)
        public double[] ProcessFramesDFT(float[] frame)
        {

            // So there's a rule "Nyquist limit", which essentially means that we only analyze the first
            // half because the second half of a Fourier Transform is just an image mirror of the first.
            int total_samples = frame.Length;
            int half_size = total_samples / 2;

            // In this double array, the 'loudnes' score of each individual freq. will be stored
            double[] freq_loud_score = new double[half_size];

            // So in this outer loop we'd be going through every freq. pitch we're testing
            for (int i = 0; i < half_size; i++)
            {
                // variables to hold the match scores of the wave
                double cosine_score = 0;
                double sine_score = 0;

                // In the inner loop each *4096(of size)* sample will go through 1 by 1
                for (int k = 0; k < total_samples; k++)
                {
                    // This is a perfectly clean wave for the exact frequency
                    double angle = 2 * Math.PI * i * k / total_samples;

                    // The audio is multiplied by the perfect wave and if it aligns the score gous up
                    // otherwise it goes down to 0.
                    cosine_score += frame[k] * Math.Cos(angle);
                    sine_score += frame[k] * Math.Sin(angle);
                }

                // Combine the two scores with the Pythagorean theorem to calculate the final/total volume
                // of this specific freq. in this audio chunk.
                freq_loud_score[i] = Math.Sqrt((cosine_score * cosine_score) + (sine_score * sine_score));
            }
            return freq_loud_score;
        }
    }
}
