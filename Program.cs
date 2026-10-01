using AudioMatcher.Media;
using AudioMatcher.Transform;

namespace AudioMatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Audio Matcher");

            string test_file_path = @"C:\Users\Dimitar\Downloads\871200__cat-fox_alex__slap-house-horn-stab.wav";

            var decoder = new AudioDecoder();

            AudioData ext_data = decoder.ReadAudioFile(test_file_path);

            if (ext_data != null)
            {
                // Some properties for info
                Console.WriteLine("Data extraction phase:");
                Console.WriteLine($"Sample Rate: {ext_data.SampleRate}");
                Console.WriteLine($"Samples extracted: {ext_data.Samples.Length}");

                // check if chunking logic works:
                Console.WriteLine("\n Chunking logic:");
                var analyzer = new AudioAnalyzer();
                var frames = analyzer.CreateFrames(ext_data.Samples);

                Console.WriteLine($"Split audio into {frames.Count} individual frames.");
                // added just to check example file
                Console.WriteLine("\nFor reference number of chunks should be sample size lenght/4096 or in this case 890182/4096=" + 890182 / 4096);
                Console.WriteLine($"Each frame contains {frames[0].Length} samples.");
            }
            else
            {
                Console.WriteLine("\nError! Extraction Failed."); // Test done with file path and got correct message.
            }
        }
    }
}
