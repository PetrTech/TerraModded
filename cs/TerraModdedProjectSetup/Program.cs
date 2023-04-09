using System.Net;
using System;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace TerraModdedProjectSetup
{
    internal class Program
    {
        static string projectName = "";
        static List<Package> publicPackages = new();
        static List<Package> experimentalPackages = new();
        static string basemanifest = "https://raw.githubusercontent.com/PetrTech/TerraModded-Packages/main/com.terramodded-base/manifest.txt";
        static string manifestDownload = "";
        static void Main(string[] args)
        {
            Console.WriteLine("Begin TerraModded project setup...");

            ChooseProjectName();
        }

        public static string RemoveSpecialCharacters(string text)
        {
            return Regex.Replace(text, "[^0-9A-Za-z _-]", "");
        }

        static void ChooseProjectName()
        {
            Console.WriteLine("Enter the project's name:");
            string projectname = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(projectname))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid project name, try again. Project names can't be empty.");
                Console.ForegroundColor = ConsoleColor.Gray;
                ChooseProjectName();
            }
            else
            {
                if(Regex.IsMatch(projectname, "[^0-9A-Za-z _-]"))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Project name contains special characters. They will be removed. Press any key to continue.");
                    Console.ReadKey();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    projectName = RemoveSpecialCharacters(projectname);
                }
            }

            projectName = projectname;
            ChooseDefaultPackages();
        }

        // going to be rewritten, please don't look at this. This sucks
        static void ParseManifest(string manifest)
        {
            publicPackages.Clear();
            experimentalPackages.Clear();
            foreach (string publicline in manifest.Split("--PUBLIC")[1].Split("--EXPERIMENTAL")[0].Split(Environment.NewLine)){
                Package pkg = new();
                pkg.name = publicline.Split(';')[0];
                pkg.name = pkg.name.Trim();
                pkg.date = publicline.Split(';')[1];
                pkg.size = publicline.Split(';')[2];
                pkg.downloadzip = publicline.Split(';')[3] + ".zip";
                pkg.version = publicline.Split(';')[4];
                publicPackages.Add(pkg);
            }

            foreach (string publicline in manifest.Split("--EXPERIMENTAL")[1].Split(Environment.NewLine))
            {
                Package pkg = new();
                pkg.name = publicline.Split(';')[0];
                pkg.name = pkg.name.Trim();
                pkg.date = publicline.Split(';')[1];
                pkg.size = publicline.Split(';')[2];
                pkg.downloadzip = publicline.Split(';')[3] + ".zip";
                pkg.version = publicline.Split(';')[4];
                experimentalPackages.Add(pkg);
            }
            manifestDownload = manifest.Split("--DATA:")[1].Split(Environment.NewLine)[0].Split(";")[1].Split("--PUBLIC")[0];
            manifestDownload = manifestDownload.Trim();
        }

        public static string GenerateManifest(List<Package> packagestowrite, List<bool> autoupdate)
        {
            string output = string.Empty;

            foreach(Package pkg in packagestowrite)
            {
                output += Environment.NewLine + manifestDownload + pkg.downloadzip + "!@";
                if (autoupdate[packagestowrite.IndexOf(pkg)] == true)
                {
                    output += "^";
                }
                else
                {
                    output += "*";
                }
                output += pkg.version;
            }

            output = output.TrimStart();
            return output;
        }

        static void ChooseDefaultPackages()
        {
            Console.Clear();
            Console.Write("Project Name");
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write(" >> ");
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write(projectName + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Please pick the base packages you want to include (separate with space)");
            Console.WriteLine("Please wait... fetching packages from com.terramodded-base");

            string s;
            using (WebClient client = new WebClient())
            {
                s = client.DownloadString(basemanifest);
            }

            ParseManifest(s);

            Console.WriteLine("Public Packages:");
            foreach(Package pkg in publicPackages)
            {
                Console.WriteLine(publicPackages.IndexOf(pkg) + ": ");
                Console.WriteLine(pkg.name + " - " + pkg.date);
                Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(pkg.size + " - " + pkg.version);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" ");
            }

            Console.WriteLine("Experimental Packages:");
            foreach (Package pkg in experimentalPackages)
            {
                Console.WriteLine(experimentalPackages.IndexOf(pkg) + publicPackages.Count + ": ");
                Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(pkg.name + " - " + pkg.date);
                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(pkg.size + " - " + pkg.version);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" ");
            }

            string selectedPackages = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(selectedPackages))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid string. Please try again. Write the numbers, not the packages names. Like this: 0 2 3 5 (...)");
                Console.ForegroundColor = ConsoleColor.Gray;
                ChooseDefaultPackages();
            }

            List<Package> chosenPackages = new List<Package>();

            Console.WriteLine("Adding these packages:");
            try
            {
                foreach (string pkg in selectedPackages.Split(" "))
                {
                    if (int.Parse(pkg) > publicPackages.Count - 1)
                    {
                        Console.WriteLine(experimentalPackages[int.Parse(pkg) - publicPackages.Count].name);
                        chosenPackages.Add(experimentalPackages[int.Parse(pkg) - publicPackages.Count]);
                    }
                    else
                    {
                        Console.WriteLine(publicPackages[int.Parse(pkg)].name);
                        chosenPackages.Add(publicPackages[int.Parse(pkg)]);
                    }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid string. Please try again. Write the numbers, not the packages names. Like this: 0 2 3 5 (...)");
                Console.ForegroundColor = ConsoleColor.Gray;
                ChooseDefaultPackages();
            }

            Console.WriteLine("Please wait. This might take a second.");
            Console.WriteLine("This process requires internet connection.");

            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraModded-dev", projectName));
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraModded-dev", projectName, ".tm"), "TERRAMODDED PROJECT");

            List<bool> autoupdate = new();

            foreach(Package package in chosenPackages)
            {
                using (var client = new WebClient())
                {
                    Console.WriteLine("Begin downloading " + package.name + " from: " + manifestDownload + package.downloadzip);
                    Console.WriteLine("Should this package automatically update? (Y/N, any other input = YES)");
                    if(Console.ReadKey().KeyChar.ToString().ToLower() == "n")
                    {
                        autoupdate.Add(false);
                    }
                    else
                    {
                        autoupdate.Add(true);
                    }
                    client.DownloadFile(manifestDownload + package.downloadzip, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraModded-dev", projectName, package.name+".pkg"));
                }
            }

            string manifestString = GenerateManifest(chosenPackages, autoupdate);
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TerraModded-dev", projectName, "manifest.tm"), manifestString);
        }
    }
}