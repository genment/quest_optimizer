using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapleLib.WzLib;

namespace quest_optimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = $"{General.Name} (v{General.Version.ToString("0.0")})";

            bool silent = false;
            bool autoupdate = true;

            foreach(string arg in args)
            {
                switch(arg)
                {
                    case "-silent":
                        silent = true;
                        break;
                    case "-noupdate":
                        autoupdate = false;
                        break;
                }
            }

            if(!silent)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Welcome to {Console.Title}!" + Environment.NewLine);
                Console.ForegroundColor = ConsoleColor.White;
            }

            if(autoupdate)
            {
                AutoUpdater.StartUpdate();
            }

            if(!File.Exists("Quest.wz"))
            {
                Console.WriteLine("Quest.wz is missing.");
                Console.ReadLine();

                return;
            }

            if(File.Exists("Quest.wz.patched"))
            {
                File.Delete("Quest.wz.patched");
            }

            var exclusions = ReadExclusions();

            if(!silent)
            {
                if(exclusions.Count == 0)
                {
                    Console.WriteLine("--- No quests will be excluded.");
                }

                else
                {
                    Console.WriteLine($"--- Excluded quests: {string.Join(", ", exclusions)}.");
                }

                Console.WriteLine(Environment.NewLine + "About to iterate through Quest.wz now." + Environment.NewLine);
            }

            var delete = new List<string>
            {
                // i've left the CPU demanding parts uncommented, the rest aren't very bad at all
                "Act.img",
                "Check.img",
                "Exclusive.img",
                "QuestDestination.img",
                "QuestExpByLevel.img",
                "QuestInfo.img",
                "QuestPerformByDay.img",
                "Say.img",
                "SQuest.img"
            };

            using(var questwz = new WzFile("Quest.wz", WzMapleVersion.BMS)) // for some reason GMS uses the BMS format..?
            {
                questwz.ParseWzFile();

                foreach(string image in delete)
                {
                    var img = questwz.WzDirectory.GetImageByName(image);
                    var original = img.WzProperties.ToList();

                    foreach(var prop in original)
                    {
                        if(!exclusions.Contains(int.Parse(prop.Name)))
                        {
                            img.WzProperties.Remove(prop);
                        }
                    }

                    img.Changed = true;

                    if(!silent)
                    {
                        Console.WriteLine($"--- Cleaned {image} (entries: {original.Count - img.WzProperties.Count})");
                    }
                }

                questwz.WzDirectory.ParseImages();

                if(File.Exists("Quest.wz.bak"))
                {
                    File.Delete("Quest.wz.bak");
                }

                File.Copy("Quest.wz", "Quest.wz.bak");
                questwz.SaveToDisk("Quest.wz.patched");

                if(!silent)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(Environment.NewLine + "--- Saved patched file to Quest.wz.patched.");
                }
            }

            if(!silent)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Environment.NewLine + "Finished patching!");
                Console.ReadLine();
            }
        }

        static List<int> ReadExclusions()
        {
            var temp = new List<int>();

            if(!File.Exists("exclude.txt"))
            {
                return temp;
            }

            try
            {
                foreach(string line in File.ReadLines("exclude.txt"))
                {
                    if(line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string final = line.Trim();

                    if(final.Contains("-"))
                    {
                        string[] split = final.Split('-');

                        if(split.Length > 0)
                        {
                            int start = int.Parse(split[0]);
                            int end = int.Parse(split[1]);

                            for(int i = start; i <= end; i++)
                            {
                                temp.Add(i);
                            }
                        }
                    }

                    else
                    {
                        temp.Add(int.Parse(final));
                    }
                }

                temp.Sort();
            }

            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"--- ERROR: {ex.Message}!");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return temp;
        }
    }
}
