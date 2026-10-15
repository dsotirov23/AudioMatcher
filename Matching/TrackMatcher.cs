using System;
using System.Collections.Generic;
using System.Text;
using AudioMatcher.Database;
using AudioMatcher.Hashing;

namespace AudioMatcher.Matching
{
    internal class TrackMatcher
    {
        // Method implements algorithm that searches for matches and calculates the time offset.
        // For a match to happen we don't just need a high number of notes that are the same -
        // they need to occur at the exact same distance from one another for it to be the exact same.
        public string FindBestMatch(List<Fingerprint> query_fingerprints, FingerprintDatabase db)
        {
            // This structure represents a table that tracks the match score for the time-aligned tracks
            // It's Dictionary<song name, Dictionary<time offset, match count>>
            // It groups points by songm and then groups them by their specific time offset
            Dictionary<string, Dictionary<int, int>> scoring_table = new Dictionary<string, Dictionary<int, int>>();

            // Loop through all fingeprints from the "live" audio clip
            foreach(Fingerprint query_fp in query_fingerprints)
            {
                // See if DB has this exact hash string already
                List<SongOccurence> db_matches = db.GetOccurences(query_fp.Hash);

                if (db_matches != null)
                {
                    // DB might find this hash in different songs, so we loop through these known occurences
                    foreach (SongOccurence db_record in db_matches)
                    {
                        // Time alignment - the idea is we substract the time frame of the audio we're receiving
                        // from the one we have in the database (the initial matching one). If they actually match
                        // this offset should be the exact same over and over, which creates sth called a 'cluster'.
                        int offset = db_record.AbsoluteTimeFrame - query_fp.AnchorTimeFrame;
                        string song = db_record.SongName;

                        // If this song or specific offset is seen for the first time - initialize the nested dict.
                        if (!scoring_table.ContainsKey(song))
                        {
                            scoring_table[song] = new Dictionary<int, int>();
                        }

                        if (!scoring_table[song].ContainsKey(offset))
                        {
                            scoring_table[song][offset] = 0;
                        }

                        // Award a 'point' to the song+offset combo
                        scoring_table[song][offset]++;
                    }
                }
            }

            // Analyze the scoring table to find which combo of aligned hashes has the biggest cluster
            string best_song = "No Match Found";
            int highest_score = 0;

            foreach (var song_record in scoring_table)
            {
                string current_song = song_record.Key;

                int max_cluster_score = 0;

                // Loop through every time-offset cluster for current song to find the one with highest score
                foreach (var offsetRecord in song_record.Value)
                {
                    int clusterSize = offsetRecord.Value;

                    if (clusterSize > max_cluster_score)
                    {
                        max_cluster_score = clusterSize;
                    }
                }

                // If cluster is bigger than whatt we have, it becomes new best
                if (max_cluster_score > highest_score)
                {
                    highest_score = max_cluster_score;
                    best_song = current_song;
                }
            }

            Console.WriteLine("\n Match Results");
            Console.WriteLine($"Best Match: {best_song}");
            Console.WriteLine($"Confidence Score: {highest_score} perfectly aligned hashes");

            return best_song;
        }
    }
}
