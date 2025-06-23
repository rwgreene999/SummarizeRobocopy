using System.IO;

namespace SummarizeRobocopy
{
    class GetRobocopySummary
    {
        public static List<string> GetSummary(string filename)
        {
            List<string> results = new List<string> { };
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    results.Add("Robocopy Summary for " + filename);
                    string line;
                    int cntLineBreaks = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("------------------------------------"))
                        {
                            cntLineBreaks++;
                        }
                        if (cntLineBreaks >= 1 && cntLineBreaks < 3 && line.Length > 5)
                        {
                            results.Add(line);
                        }
                        if (cntLineBreaks >= 4)
                        {
                            results.Add(line);
                        }
                    }
                }
                if (results.Count > 1)
                {
                    AddFinalizer(results);
                }

            }
            catch (Exception ex)
            {
                results.Add($"ERROR processing file {filename}");
                results.Add(ex.Message);
                AddFinalizer(results);
            }            
            return results;
        }

        private static void AddFinalizer(List<string> results)
        {
            results.Add("------------------------------------");
            results.Add("------------------------------------");
            results.Add("");
            results.Add("");
        }
    }
}
