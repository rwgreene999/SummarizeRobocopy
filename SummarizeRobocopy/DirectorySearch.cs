using System.IO;

namespace SummarizeRobocopy
{
    // Create a List<string> list of files based on user input 
    // valid for path, file, wildcards, qualified filename or not.
    internal class DirectorySearch
    {
        public string errMsg { get; private set; } = string.Empty;
        public List<string> filesList { get; private set; } = new List<string> { };


        /// <summary>
        /// find selected files, placed into filesList 
        /// </summary>
        /// <param name="searchString">filename, qualified filename, may contain wildcards, directory for all </param>
        /// <param name="filters">Optional, list file types ex: List<string> a = { "jpg", "png" }</param>
        /// <param name="subdirectory">Optional, if exist only file types in list returned useful for wildcards</param>
        /// <returns>true if worked, else check errMsg fr failure , check filesList.Count for return </returns>
        /// searchString examples: 
        ///         x:\junk\filename.jpg  -- Just that file returned (ignores filters)
        ///         x:\junk    -- all files in folder subject to filters 
        ///         x:\junk\*   --  all files in folder subject to filters 
        ///         x:\junk\test*.*  -- all files in folder with name starting with test subject to filters 
        public bool Search(string searchString, bool subdirectory, List<string> filters = null)
        {
            try
            {

                filesList.Clear();

                if (string.IsNullOrEmpty(searchString.Trim()))
                {
                    return false;
                }
                searchString = searchString.Trim();

                searchString = Path.GetFullPath(searchString);

                if (File.Exists(searchString))
                {
                    filesList.Add(searchString);
                    return true;
                }

                string fileName;
                string dirName;
                if (Directory.Exists(searchString))
                {
                    fileName = "*.*";
                    dirName = searchString;
                }
                else
                {
                    fileName = Path.GetFileName(searchString);
                    dirName = searchString.Replace(fileName, string.Empty);
                }

                if (Directory.Exists(dirName))
                {
                    ListFiles(fileName, dirName, filters, subdirectory);
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }

        }

        private void ListFiles(string fileName, string dirName, List<string> filters, bool recurseSubs)
        {
            var searchOption = recurseSubs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            IEnumerable<string> e = Directory.EnumerateFiles(dirName, fileName, searchOption);

            if (filters == null)
            {
                filesList = e.ToList();
            }
            else
            {
                filesList = e.Where(name => filters.Any(filter => name.EndsWith(filter))).ToList();
            }
        }

    }
}


