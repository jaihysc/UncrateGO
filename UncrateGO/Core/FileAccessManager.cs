using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UncrateGo.Core
{
    public class FileAccessManager
    {
        //This must be set prior to using the methods in this class
        private static readonly string RootLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        
        public static string ReadFromFile(string filePath)
        {
            try
            {
                //Create file if it does not exist
                if (!File.Exists(filePath))
                {
                    using (StreamWriter file = new StreamWriter(filePath, true))
                    {
                        file.WriteLine("");
                    }
                }

                using (StreamReader r = new StreamReader(filePath))
                {
                    return r.ReadToEnd();
                }
            }
            catch
            {
                EventLogger.LogMessage("Unable to read from file", ConsoleColor.Red);
                return "";
            }
        }
        

            
        public static void WriteStringToFile(string stringToWrite, bool overwriteExistingContent, string filePath)
        {
            try
            {
                //Overwrite existing contents if true
                if (overwriteExistingContent)
                {
                    File.WriteAllText(filePath, "");
                }

                //Write string
                using (StreamWriter file = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    file.WriteLine(stringToWrite);
                }

            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to write string to file", ConsoleColor.Red);
            }

        }
        

        public static string GetFileLocation(string fileName)
        {
            string returnFileLocation = "";

            try
            {
                //Read root path file
                string[] fileLocations = File.ReadAllLines(RootLocation + @"\Paths.txt");

                //Check path file for specified name of txt file
                //E.G "UserCredits.txt"
                returnFileLocation = fileLocations.FirstOrDefault() + fileName;
            }
            catch (Exception)
            {
                EventLogger.LogMessage("Unable to get file location " + fileName, ConsoleColor.Red);
            }

            return returnFileLocation;
        }
    }
}
