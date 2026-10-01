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
    }
}
