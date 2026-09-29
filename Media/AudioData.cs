using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Media
{
    public record AudioData (float[] Samples, int SampleRate);
}
