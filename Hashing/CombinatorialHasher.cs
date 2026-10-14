using AudioMatcher.Transform;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Hashing
{
    internal class CombinatorialHasher
    {
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
