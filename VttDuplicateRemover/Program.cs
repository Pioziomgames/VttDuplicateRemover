using System.Text;

namespace VttDuplicateRemover
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VttDuplicateRemover 1.0 by Pioziomgames");
            if (args.Length == 0)
            {
                Console.WriteLine("A simple program for removing identical vtt subtitle entries");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("VttDuplicateRemover.exe input.vtt (optional)output.vtt");
                Console.WriteLine("If output is not specified the input file be overwritten");

                Exit();
            }

            try
            {
                if (!File.Exists(args[0]))
                    throw new Exception($"File: {args[0]} doesn't exist!");

                string OutputPath = args.Length == 1 ? args[0] : args[1];

                string[] lines = File.ReadAllLines(args[0]);

                if (lines.Length < 2)
                    throw new Exception("Provided file is empty");

                if (!lines[0].Contains("WEBVTT"))
                    throw new Exception("Not a proper WEBVTT file");

                //The header is treaded as subtitle line but it should be fine
                //(Actually any additional data within the subtitles is treaded as part of the last subtitle too)
                List<string> SubtitleLines = new List<string>();
                StringBuilder Current = new StringBuilder();
                int Removed = 0;
                bool Found = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    /*if (String.IsNullOrWhiteSpace(lines[i]))
                       continue;*/

                    if (lines[i].Contains("-->")) //Look for subtitle lines
                    {
                        Found = true;
                        string cur = Current.ToString();
                        if (!SubtitleLines.Contains(cur))
                            SubtitleLines.Add(cur);
                        else
                            Removed++;
                        Current.Clear();
                    }
                    //avoid using AppendLine because it uses the stupid /r/n windows seperator
                    Current.Append(lines[i]);
                    Current.Append('\n');
                }
                if (!Found)
                    throw new Exception("No WEBVTT subtitles lines found");

                string cur2 = Current.ToString(); //Don't forget about the last line!
                if (!SubtitleLines.Contains(cur2))
                    SubtitleLines.Add(cur2);
                else
                    Removed++;

                if (Removed == 0)
                {
                    if (args[0] == OutputPath)
                    {
                        Console.WriteLine($"No duplicate lines were found, nothing to save");
                        return;
                    }
                    else
                    {
                        Console.WriteLine("No duplicate lines were found, data will still be saved to the output file");
                    }
                }
                else
                    Console.WriteLine($"Removed {Removed} duplicate subtitle lines");

                string? DirectoryPath = Path.GetDirectoryName(OutputPath);
                if (!string.IsNullOrWhiteSpace(DirectoryPath) && !Directory.Exists(DirectoryPath))
                {
                    Console.WriteLine($"Creating directory: {DirectoryPath}");
                    Directory.CreateDirectory(DirectoryPath);
                }

                Console.WriteLine($"Saving file: {OutputPath}");
                using (BinaryWriter writer = new BinaryWriter(File.Create(OutputPath)))
                {
                    for (int i = 0; i < SubtitleLines.Count; i++)
                    {
                        //Write the strings as char[] because we're dealing with a text file
                        writer.Write(SubtitleLines[i].ToArray());
                        //The strings had new lines appended to them earlier
                        //so there's no longer a need to do that
                    }
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Program failed due to the following error:");
                Console.WriteLine(ex.Message);
                Exit();
            }

        }
        static void Exit()
        {
            Console.WriteLine("\n\nPress Any Button to Exit");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}