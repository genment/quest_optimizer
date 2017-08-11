using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace quest_optimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = $"{General.Name} (v{General.Version.ToString("0.0")})";

            if(!File.Exists("Quest.wz"))
            {
                Console.WriteLine("Quest.wz is missing.");
                Console.ReadLine();

                return;
            }

            bool silent = false;
            bool autoupdate = true;

            foreach(string arg in args)
            {
                switch(arg)
                {
                    case "-silent":
                        {
                            silent = true;
                            break;
                        }

                    case "-noupdate":
                        {
                            autoupdate = false;
                            break;
                        }

                    case "-searchmode":
                        {
                            SearchMode();
                            return;
                        }
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
                    if(exclusions.Count > 50)
                    {
                        var fifty = exclusions.ToList();
                        fifty.RemoveRange(50, fifty.Count - 50);

                        Console.WriteLine($"--- Excluded quests: {string.Join(", ", fifty)}... and {exclusions.Count - 50} more!");
                    }

                    else
                    {
                        Console.WriteLine($"--- Excluded quests: {string.Join(", ", exclusions)}.");
                    }
                }

                Console.WriteLine(Environment.NewLine + "About to iterate through Quest.wz now." + Environment.NewLine);
            }

            var delete = new List<string>
            {
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

                            if(!img.Changed)
                            {
                                img.Changed = true;
                            }
                        }
                    }

                    if(!silent && img.Changed)
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

        static void SearchMode()
        {
            Console.WriteLine("Loading Quest.wz ...");

            using(var questwz = new WzFile("Quest.wz", WzMapleVersion.BMS))
            {
                questwz.ParseWzFile();
                var all = questwz.WzDirectory.GetImageByName("QuestInfo.img").WzProperties;

                while(true)
                {
                    Console.Write(Environment.NewLine + "Enter the quest's ID or Name that you want to search for (enter 'qq' to quit): ");
                    string searchStr = Console.ReadLine().ToLower();
                    Console.WriteLine();

                    if(string.IsNullOrEmpty(searchStr) || string.IsNullOrWhiteSpace(searchStr))
                    {
                        continue;
                    }

                    if(searchStr.Length < 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"--- ERROR: Make sure your keyword is longer than 2 letters!");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    if("qq".Equals(searchStr))
                    {
                        break;
                    }

                    var results = new List<Tuple<int, string>>();

                    foreach(var item in all)
                    {
                        string id = item.Name;
                        string name = ((WzStringProperty)item["name"])?.GetString();

                        if(name == null)
                        {
                            continue;
                        }

                        if(name.ToLower().Contains(searchStr) || id.Contains(searchStr))
                        {
                            results.Add(new Tuple<int, string>(int.Parse(id), name));
                        }
                    }

                    foreach(var item in results)
                    {
                        Console.WriteLine(item.Item1 + " // " + item.Item2);
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"--- Found Quests: {results.Count}" + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.White;

                    if(results.Count > 0)
                    {
                        Console.Write("Save the results into a file?" + Environment.NewLine +
                            "('y' for simple, 'd' for detail, otherwise nothing): ");
                        string save = Console.ReadLine().ToLower();

                        if("y".Equals(save))
                        {
                            SaveSearchResult(searchStr, results, true);
                        }
                        if("d".Equals(save))
                        {
                            SaveSearchResult(searchStr, results, false);
                        }
                    }
                }
            }
        }

        static void SaveSearchResult(string name, List<Tuple<int, string>> results, bool range)
        {
            string filename = $"result-{name}.txt";

            if(File.Exists(filename))
            {
                File.Delete(filename);
            }
            if(range)
            {
                List<string> converted = MakeRange(results);
                File.WriteAllLines(filename, converted.ToArray());
            }
            else
            {
                File.WriteAllLines(filename, results.Select(item =>
                    "// " + item.Item2 + Environment.NewLine
                    + item.Item1));
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"--- Saved: {Environment.CurrentDirectory}\\{filename}" + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static List<string> MakeRange(List<Tuple<int, string>> list)
        {
            int pointer = 0;
            var rangelist = new List<int[]>();

            int[] range = new int[2] {
                list[0].Item1,
                0
            };
            rangelist.Add(range);

            for(int i = 1; i < list.Count; i++)
            {
                int id = list[i].Item1;
                if(id - rangelist[pointer][1] == 1 || id - rangelist[pointer][0] == 1) // same range
                {
                    rangelist[pointer][1] = id;
                }
                else // new range
                {
                    range = new int[2] {
                        list[i].Item1,
                        0
                    };
                    rangelist.Add(range);
                    ++pointer;
                }
            }
            var converted = new List<string>();
            string line;
            foreach(var item in rangelist)
            {
                line = item[0].ToString();
                if(item[1] != 0)
                {
                    line += $"-{item[1]}";
                }
                converted.Add(line);
            }
            return converted;
        }
    }
}
