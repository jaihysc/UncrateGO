using Discord.Commands;
using System;
using System.IO;
using System.Reflection;

namespace UncrateGo.Core
{
    public class SetupManager : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Checks to see if a Paths.txt is present at the execution folder, otherwise it will prompt the user to create one
        /// </summary>
        public static void CheckIfPathsFileExists()
        {
            if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Paths.txt"))
            {
                GenerateConfigFile();
            }
        }

        private static void GenerateConfigFile()
        {
            //Get user input and generate config file
            Console.WriteLine("It appears this is your first startup of UncrateGo");
            Console.WriteLine(@"Configure the bot by entering the paths file where all data will be stored, include a \ at the end of the file");
            Console.WriteLine();
            string path = Console.ReadLine();

            try
            {
                FileAccessManager.WriteStringToFile(path, true, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Paths.txt");

                Console.WriteLine();
                Console.WriteLine("Setup is complete");
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();

                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("An error occurred during setup");
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue...");
                Console.WriteLine();
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();

                Console.Clear();
            }
        }
    }
}
