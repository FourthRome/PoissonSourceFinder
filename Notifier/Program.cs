using System;

namespace Notifier
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Media.SystemSounds.Beep.Play();
            BeepPlayer.PlaySong();
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
