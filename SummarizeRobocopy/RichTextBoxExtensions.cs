using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SummarizeRobocopy
{
    public static class RichTextBoxExtensions
    {
        public static void AppendPlainText(this RichTextBox rtb, string text)
        {
            // Ensure there is at least one Paragraph
            if (rtb.Document.Blocks.Count == 0)
            {
                rtb.Document.Blocks.Add(new Paragraph());
            }

            Paragraph p = rtb.Document.Blocks.FirstBlock as Paragraph;

            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.Contains(" ERROR "))
                {
                    p.Inlines.Add(new Run(line) { Background = Brushes.Yellow, FontFamily = new FontFamily("Consolas") });
                    p.Inlines.Add(new Run("\r\n")); 
                } else if ( ( line.StartsWith("   Files : ") ||
                              line.StartsWith("    Dirs : ") ||
                              line.StartsWith("   Bytes :") ) &&
                              !line.Contains("         0         0 " ) )
                {
                    p.Inlines.Add(new Run(line) { Background = Brushes.Yellow, FontFamily = new FontFamily("Consolas") }); 
                    p.Inlines.Add(new Run("\r\n"));
                } else
                {
                    p.Inlines.Add(new Run(line) { FontFamily = new FontFamily("Consolas") });
                    p.Inlines.Add(new Run("\r\n"));
                }
            }

        }

        public static string GetPlainText(this RichTextBox rtb)
        {
            TextRange range = new TextRange(
                rtb.Document.ContentStart,
                rtb.Document.ContentEnd);
            return range.Text;
        }
    }
}
