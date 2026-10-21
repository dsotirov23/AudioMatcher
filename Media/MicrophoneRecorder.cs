using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;

namespace AudioMatcher.Media
{
    internal class MicrophoneRecorder
    {
        // Captures live audio from the default microphone of device
        // Returns a float array with the raw PCM samples that are needed for the algorithms
        public float[] Record(int duration_seconds, int target_sample_rate)
        {
            List<float> recorded_samples = new List<float>();

            //WaveInEvent is the NAudio implementation for capturing audio from Windows multimedia.
            // It's in a using statement to safely use the microphone and stop after it finishes
            using (var waveIn = new WaveInEvent())
            {
                waveIn.DeviceNumber = 0; // 0 is the default input device 

                // The microphone needs to capture audio as 32-bit IEEE floats on a single channel.
                // It basically forces it to provide floats directly without need for conversion
                waveIn.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(target_sample_rate, 1);

                // This happens continously until the mic buffer is filled with data
                waveIn.DataAvailable += (sender, e) =>
                {
                    // The buffer returns raw bytes, but we need the 32-bit float audio, so 4 bytes make up
                    // a single sample (32 bytes / 8 bits = 4 bytes), so we loop is incremented by 4
                    for (int i = 0; i < e.BytesRecorded; i += 4)
                    {
                        // Convert 4-byte chunks into floats and add it to recording array
                        float sample = BitConverter.ToSingle(e.Buffer, i);
                        recorded_samples.Add(sample);
                    }
                };

                // Opens audio stream and begins the event
                Console.WriteLine($"\n[MIC] Recording started. You can now play the audio...");
                waveIn.StartRecording();

                // Recording process happens asynchronously on a background thread, we pause the main app
                // thread here to keep the microphone active for the recording time
                Thread.Sleep(duration_seconds * 1000);

                // Closes recording stream and finishes filling the buffer
                waveIn.StopRecording();
                Console.WriteLine($"[MIC] Recording stopped. Captured {recorded_samples.Count} samples.");
            }
            // Convert the list to a static array to use in the analyzer.
            return recorded_samples.ToArray();
        }
    }
}
