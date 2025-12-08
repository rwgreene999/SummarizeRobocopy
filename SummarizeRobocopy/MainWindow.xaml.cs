using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;
using MyExtensions;
using System.Threading.Tasks;

namespace SummarizeRobocopy
{
    public partial class MainWindow : Window
    {
        List<string> _files = new List<string>();
        private FlowFindManager _findManager;
        public MainWindow()
        {
            InitializeComponent();            
            int cntArgs = 0;
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (++cntArgs > 1)
                {
                    _files.Add(arg);
                }
            }
            this.Loaded += MainWindow_Loaded;
            _findManager = new FlowFindManager(this, contentsLoaded);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string data = String.Join(Environment.NewLine, _files);
            AppendText($"Processing files:{Environment.NewLine}{data}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
            await TheWorkAsync();
        }

        private async Task TheWorkAsync()
        {
            
            List<string> allFiles = BuildFileList();

            // Create tasks that run on the threadpool via Task.Run
            var tasklist = BuildTaskList(allFiles);

            // Await all tasks (non-blocking UI thread)
            await Task.WhenAll(tasklist);

            AddResultsTowindow(tasklist);

            FinalOutput(allFiles);
        }

        private void FinalOutput(List<string> allFiles)
        {
            string finalize = Environment.NewLine + Environment.NewLine + "Files reviewed" + Environment.NewLine + String.Join(Environment.NewLine, allFiles) + Environment.NewLine + "Finished";
            AppendText(finalize);
        }

        private void AddResultsTowindow(List<Task<List<string>>> tasklist)
        {
            foreach (Task<List<string>> t2 in tasklist)
            {
                // tasks have completed due to await Task.WhenAll above
                AppendText(String.Join(Environment.NewLine, t2.Result));
            }
        }

        // Step 2: use Task.Run so the work runs on the threadpool immediately.
        private static List<Task<List<string>>> BuildTaskList(List<string> allFiles)
        {
            var tasklist = new List<Task<List<string>>>();
            foreach (var file in allFiles)
            {
                // prefer Task.Run over new Task(...).Start()
                var task = Task.Run(() => GetRobocopySummary.GetSummary(file));
                tasklist.Add(task);
            }
            return tasklist;
        }

        private List<string> BuildFileList()
        {
            DirectorySearch ds = new DirectorySearch();
            List<string> allFiles = new List<string>();
            foreach (var file in _files)
            {
                ds.Search(file, false);
                allFiles.AddRange(ds.filesList);
            }

            return allFiles;
        }

        private void AppendText(string text)
        {
            var doc = contentsLoaded.Document ?? new FlowDocument();
            contentsLoaded.Document = doc;

            var para = new Paragraph();
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                var run = new Run(lines[i]);

                if (!string.IsNullOrEmpty(lines[i]) &&
                    lines[i].IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    SetRunToErrorColors(run);
                }
                else if ((lines[i].StartsWith("   Files : ") ||
                              lines[i].StartsWith("    Dirs : ") ||
                              lines[i].StartsWith("   Bytes :"))
                              && !lines[i].Contains("         0         0 "))
                {
                    SetRunToErrorColors(run);
                }
                else if (lines[i].StartsWith("  Started :"))
                {
                    string dtString = lines[i].Substring(12);
                    DateTime? tm = dtString.TryParseFlexible();

                    if (tm == null)
                    {
                        SetRunToWarningColors(run);
                    }
                    else
                    {
                        double hours = (DateTime.Now - tm.Value).TotalHours;
                        if (hours > 36.0)
                        {
                            SetRunToErrorColors(run);
                        }
                    }
                }
              
                para.Inlines.Add(run);

                if (i < lines.Length - 1)
                    para.Inlines.Add(new LineBreak());
            }

            doc.Blocks.Add(para);
        }

        private static void SetRunToErrorColors(Run run)
        {
            run.Background = System.Windows.Media.Brushes.Red;
            run.Foreground = System.Windows.Media.Brushes.White;
        }


        private static void SetRunToWarningColors(Run run)
        {
            run.Background = System.Windows.Media.Brushes.PaleGoldenrod;
            run.Foreground = System.Windows.Media.Brushes.White;
        }
    }
}