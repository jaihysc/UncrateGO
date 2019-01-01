using System;
using System.Collections.Generic;
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

        public static List<string> ReadFromFileToList(string filePath)
        {
            List<string> returnFileInfoList = new List<string>();

            try
            {
                //Check path file for specified name of txt file
                //E.G "UserCredits.txt"
                foreach (var item in File.ReadAllLines(filePath))
                {
                    returnFileInfoList.Add(item);
                }

            }
            catch (Exception)
            {
            }


            return returnFileInfoList;
        }

        public static string ReadFromFile(string filePath)
        {
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    return r.ReadToEnd();
                }

            }
            catch (Exception)
            {
            }

            return null;
        }


        public static void WriteListToFile(List<string> listToWrite, bool overwriteExistingContent, string filePath)
        {
            try
            {
                //Overwrite existing contents if true
                if (overwriteExistingContent == true)
                {
                    File.WriteAllText(filePath, "");
                }

                //Write items from list
                foreach (var item in listToWrite)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                    {
                        file.WriteLine(item);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
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
