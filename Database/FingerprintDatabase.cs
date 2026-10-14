using System;
using System.Collections.Generic;
using System.Text;
using AudioMatcher.Hashing;

namespace AudioMatcher.Database
{
    internal class FingerprintDatabase
    {
        // Core memory structure
        // Use a disctionary to search it quickly (O(1)), the key is the unique fingerpring string
        // It will hold a list of every time and place where that fingerprint has occured. (list because
        // same sequence can occur at different places)
        private Dictionary<string, List<SongOccurence>> _database;

        public FingerprintDatabase()
        {
            _database = new Dictionary<string, List<SongOccurence>>();
        }

        // This method takes the generated fingerprints from an audio file and puts them into a dictionary
        public void AddTrack(string songName, List<Fingerprint> fingerprints)
        {
            foreach(Fingerprint f in fingerprints)
            {
                // Check if this key is already inside db
                if (!_database.ContainsKey(f.Hash))
                {
                    _database[f.Hash] = new List<SongOccurence>(); // If it is new, make a list to hold it
                }

                // Add the hash to the list to link it to the specific audio
                _database[f.Hash].Add(new SongOccurence(songName, f.AnchorTimeFrame));
            }
        }

        // Helper function to follow database size, returns count of unique hash keys
        public int GetUniqueHashCount()
        {
            return _database.Count;
        }
    }
}
