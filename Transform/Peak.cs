using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Transform
{
    // Struct to hold peaks of loudest frequencies.
    public struct Peak
    {
        public int TimeFrameIndex; // the exact frame of the peak
        public int FrequencyBin; // the pitch
        public double Amplitude; // the loudness

        public Peak(int time_frame, int freq_bin, double amplitude)
        {
            TimeFrameIndex = time_frame;
            FrequencyBin = freq_bin;
            Amplitude = amplitude;
        }
    }
}
