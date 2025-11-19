using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SummarizeRobocopy
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        List<string> _files = new List<string>();
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
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            contentsLoaded.AppendPlainText("One");
            contentsLoaded.AppendPlainText("Two");

            string data = String.Join(Environment.NewLine, _files);
            contentsLoaded.AppendPlainText( $"Processing files:" + Environment.NewLine + data + Environment.NewLine + Environment.NewLine + Environment.NewLine );
            TheWork();

        }

        private void TheWork()
        {
            List<string> allFiles = BuildFileList();

            List<Task<List<string>>> tasklist = new List<Task<List<string>>>();
            BuildTaskList(allFiles, tasklist);

            Task.WaitAll(tasklist.ToArray());

            AddResultsTowindow(tasklist);

            FinalOutput(allFiles);
        }

        private void FinalOutput(List<string> allFiles)
        {
            string finalize = Environment.NewLine + Environment.NewLine + "Files reviewed" + Environment.NewLine + String.Join(Environment.NewLine, allFiles) + Environment.NewLine + "Finished";
            contentsLoaded.AppendPlainText(finalize);
        }

        private void AddResultsTowindow(List<Task<List<string>>> tasklist)
        {
            foreach (Task<List<string>> t2 in tasklist)
            {
                var ans = t2.Result;
                contentsLoaded.AppendPlainText(String.Join(Environment.NewLine, t2.Result));
                //contentsLoaded.Text = contentsLoaded.Text + String.Join(Environment.NewLine, t2.Result);
            }
        }


        private static void BuildTaskList(List<string> allFiles, List<Task<List<string>>> tasklist)
        {
            foreach (var file in allFiles)
            {
                var task = new Task<List<string>>(() => { return GetRobocopySummary.GetSummary(file); });
                tasklist.Add(task);
                task.Start();
            }
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

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            // WIP What is this? var currentText = contentsLoaded.AppendText;
        }
    }
}