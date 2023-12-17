using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LAB__2_Threading.Utilities
{
    internal class Sounds
    {
        private static WaveOutEvent waveOut = new WaveOutEvent();

        public static async Task PlaySoundAsync(string soundFileName)
        {
            await Task.Run(() =>
            {
                string soundFilePath = Path.Combine("Sounds", soundFileName);

                try
                {
                    using (var audioFile = new AudioFileReader(soundFilePath))
                    {
                        waveOut.Init(audioFile);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound: {ex.Message}");
                }
            });
        }
    }
}