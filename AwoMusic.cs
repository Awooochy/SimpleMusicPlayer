using System;
using System.Collections.Generic;
using System.IO;
using WMPLib;

namespace MusicPlayer
{
    class Program
    {
        static void Main(string[] args)
        {
            var player = new WindowsMediaPlayer();
            var musicFiles = new List<string>();
            var currentMusicIndex = 0;
            var loopEnabled = false;
            var isPlaying = false;
            var volume = 50;

            Console.WriteLine("Welcome to the music player!");

            Console.Write("Please enter the path to your music directory: ");
            var musicDirectory = Console.ReadLine();

            var extensions = new[] { ".mp3", ".wav", ".ogg" };
            var files = Directory.GetFiles(musicDirectory, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (Array.IndexOf(extensions, Path.GetExtension(file).ToLower()) != -1)
                {
                    musicFiles.Add(file);
                    Console.WriteLine($"Found music file: {file}");
                }
            }

            Console.Write("Do you want to index the music files? (y/n) ");
            var indexInput = Console.ReadLine();

            if (indexInput.ToLower() == "y")
            {
                for (var i = 0; i < musicFiles.Count; i++)
                {
                    var file = musicFiles[i];
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var fileExtension = Path.GetExtension(file);
                    var indexedFileName = $"{i + 1}_{fileName}{fileExtension}";

                    File.Move(file, Path.Combine(Path.GetDirectoryName(file), indexedFileName));
                    musicFiles[i] = Path.Combine(Path.GetDirectoryName(file), indexedFileName);
                }
            }

            Console.WriteLine($"Found {musicFiles.Count} music files.");

            while (true)
            {
                Console.Write("Enter a command: ");
                var input = Console.ReadLine();
                var parts = input.Split(' ');

                if (parts[0] == "-play")
                {
                    var index = int.Parse(parts[1]) - 1;

                    if (index >= 0 && index < musicFiles.Count)
                    {
                        currentMusicIndex = index;
                        player.URL = musicFiles[currentMusicIndex];
                        player.controls.play();
                        isPlaying = true;
                        Console.WriteLine($"Playing: {Path.GetFileName(musicFiles[currentMusicIndex])}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid song number.");
                    }
                }
                else if (parts[0] == "-next")
                {
                    if (currentMusicIndex == musicFiles.Count - 1)
                    {
                        currentMusicIndex = 0;
                    }
                    else
                    {
                        currentMusicIndex++;
                    }

                    player.URL = musicFiles[currentMusicIndex];
                    player.controls.play();
                    isPlaying = true;
                    Console.WriteLine($"Playing: {Path.GetFileName(musicFiles[currentMusicIndex])}");
                }
                else if (parts[0] == "-prev")
                {
                    if (currentMusicIndex == 0)
                    {
                        currentMusicIndex = musicFiles.Count - 1;
                    }
                    else
                    {
                        currentMusicIndex--;
                    }

                    player.URL = musicFiles[currentMusicIndex];
                    player.controls.play();
                    isPlaying = true;
                    PrintProgress(player, musicFiles[currentMusicIndex]);
                }
                else if (parts[0] == "-stop")
                {
                    player.controls.stop();
                    isPlaying = false;
                }
                else if (parts[0] == "-con")
                {
                    if (isPlaying)
                    {
                        Console.WriteLine("Music is already playing.");
                    }
                    else
                    {
                        player.controls.play();
                        isPlaying = true;
                        PrintProgress(player, musicFiles[currentMusicIndex]);
                    }
                }
                else if (parts[0] == "-vol")
                {
                    var newVolume = int.Parse(parts[1]);

                    if (newVolume < 0 || newVolume > 100)
                    {
                        Console.WriteLine("Invalid volume. Please enter a number between 0 and 100.");
                    }
                    else
                    {
                        volume = newVolume;
                        player.settings.volume = volume;
                    }
                }
                else if (parts[0] == "-loop")
                {
                    loopEnabled = true;
                    player.settings.setMode("loop", true);
                    Console.WriteLine("Looping enabled.");
                }
                else if (parts[0] == "-unloop")
                {
                    loopEnabled = false;
                    player.settings.setMode("loop", false);
                    Console.WriteLine("Looping disabled.");
                }
                else
                {
                    Console.WriteLine("Invalid command.");
                }
            }
        }

        static void PrintProgress(WindowsMediaPlayer player, string musicFile)
        {
            Console.WriteLine($"Now playing: {Path.GetFileName(musicFile)}");

            while (player.playState == WMPPlayState.wmppsPlaying)
            {
                Console.Write("|");

                var progress = player.controls.currentPosition / player.currentMedia.duration;
                var progressBarLength = Console.WindowWidth - 2;
                var progressChars = (int)(progress * progressBarLength);

                for (var i = 0; i < progressChars; i++)
                {
                    Console.Write("█");
                }

                for (var i = progressChars; i < progressBarLength; i++)
                {
                    Console.Write(" ");
                }

                Console.Write("|");
                Console.Write($" {TimeSpan.FromSeconds(player.controls.currentPosition).ToString(@"mm\:ss")} / {TimeSpan.FromSeconds(player.currentMedia.duration).ToString(@"mm\:ss")}");

                if (Console.CursorLeft < Console.WindowWidth - 1)
                {
                    Console.CursorLeft = 0;
                }
            }

            if (player.playState == WMPPlayState.wmppsStopped && !player.settings.getMode("loop") && player.controls.currentPosition == player.currentMedia.duration)
            {
                Console.WriteLine("\nEnd of song.");
            }
        }
    }
}