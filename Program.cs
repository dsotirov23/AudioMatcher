using AudioMatcher.Media;

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
                Console.WriteLine("Successfully extracted data.");
                Console.WriteLine($"Sample Rate: {ext_data.SampleRate}");
                Console.WriteLine($"Samples extracted: {ext_data.Samples.Length}");
            }
            else
            {
                Console.WriteLine("\nError! Extraction Failed."); // Test done with file path and got correct message.
            }
        }
    }
}
