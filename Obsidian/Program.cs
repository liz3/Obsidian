using System;
using System.IO;
using Gtk;
using Obsidian.Api;

namespace Obsidian
{
    class MainClass
    {
         static string startDir =
            new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName.Replace("\\", "/");
        public static void Main(string[] args)
        {
            Console.WriteLine(startDir);
            Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }
    }
}
