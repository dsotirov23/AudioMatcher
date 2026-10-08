using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Transform
{
    public struct Fingerprint
    {
        // A string which will hold a 'signature' for the anchor pitch, the target one and the time delay between them.
        public string Hash;

        // Where exactly the peak of the anchor occurs
        public int AnchorTimeFrame;

        public Fingerprint(string hash, int anchor_time_frame)
        {
            Hash = hash;
            AnchorTimeFrame = anchor_time_frame;
        }
    }
}
