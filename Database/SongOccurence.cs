using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Database
{
    // Struct to store tags of hashes and link them to a song
    public struct SongOccurence
    {
        // The name of song where hash was found
        public string SongName;

        // The exact frame where the anchor note of this hash is found. Will be used to
        // align time of matching song to sound
        public int TimeFrame;

        public SongOccurence(string song_name, int time_frame)
        {
            SongName = song_name;
            TimeFrame = time_frame;
        }
    }
}
