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
        internal static string rootLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        
        public static string ReadFromFile(string filePath)
        {
            //Create file if it does not exist
            if (!File.Exists(filePath))
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                {
                    file.WriteLine("");
                }
            }

            using (StreamReader r = new StreamReader(filePath))
            {
                return r.ReadToEnd();
            }
        }
        

            
        public static void WriteStringToFile(string stringToWrite, bool overwriteExistingContent, string filePath)
        {
            try
            {
                //Overwrite existing contents if true
                if (overwriteExistingContent == true)
                {
                    File.WriteAllText(filePath, "");
                }

                //Write string
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true, Encoding.UTF8))
                {
                    file.WriteLine(stringToWrite);
                }

            }
            catch (Exception)
            {
            }

        }
        

        public static string GetFileLocation(string fileName)
        {
            string returnFileLocation = "";

            try
            {
                //Read root path file
                var fileLocations = File.ReadAllLines(rootLocation + @"\Paths.txt");

                //Check path file for specified name of txt file
                //E.G "UserCredits.txt"
                returnFileLocation = fileLocations.FirstOrDefault() + fileName;
            }
            catch (Exception)
            {
            }

            return returnFileLocation;
        }
    }
}
