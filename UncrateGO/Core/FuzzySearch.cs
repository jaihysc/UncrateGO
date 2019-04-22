using System;
using System.Collections.Generic;

namespace UncrateGo.Core
{
    public static class FuzzySearch
    {
        /// <summary>
        /// Compute the distance between two strings
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <returns></returns>
        public static int Compute(string string1, string string2)
        {
            int n = string1.Length;
            int m = string2.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (string2[j - 1] == string1[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static List<string> FindSimilarItemsByWords(IEnumerable<string> itemNames, string filterString)
        {
            var userSkinEntries = new List<string>();

            bool match = false;

            string[] tokens = UnicodeManager.RemoveSpecialCharacters(filterString).ToLower().Split(' ');

            //Search through for words match specified input string separated with spaces
            foreach (var item in itemNames)
            {
                foreach (var t in tokens)
                {
                    if (!UnicodeManager.RemoveSpecialCharacters(item).ToLower().Contains(t))
                    {
                        match = false;
                        break;
                    }

                    match = true;
                }

                if (match) userSkinEntries.Add(item);
            }

            return userSkinEntries;
        }
    }
}
