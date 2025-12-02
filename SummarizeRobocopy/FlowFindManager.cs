using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SummarizeRobocopy
{
    /// <summary>
    /// Manages Ctrl+F / Find behavior for a FlowDocumentReader while keeping the reader in Scroll mode.
    /// Instantiate from MainWindow after InitializeComponent: _findManager = new FindManager(this, contentsLoaded);
    /// </summary>
    public sealed class FlowFindManager
    {
        private readonly Window _owner;
        private readonly FlowDocumentReader _reader;

        // find state
        private List<TextRange> _matchRanges = new List<TextRange>();
        private int _currentMatchIndex = -1;
        private string? _lastQuery;

        private CommandBinding? _commandBinding;
        private KeyBinding? _keyBinding;

        // prevent re-entrant dialogs
        private bool _isFindDialogOpen;

        public FlowFindManager(Window owner, FlowDocumentReader reader, bool registerBindings = true)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));

            if (registerBindings)
                RegisterBindings();
        }

        public void RegisterBindings()
        {
            // avoid duplicates
            if (!_owner.CommandBindings.OfType<CommandBinding>().Any(cb => cb.Command == ApplicationCommands.Find))
            {
                _commandBinding = new CommandBinding(ApplicationCommands.Find, FindCommand_Executed, FindCommand_CanExecute);
                _owner.CommandBindings.Add(_commandBinding);
            }

            if (!_owner.InputBindings.OfType<KeyBinding>().Any(kb => kb.Command == ApplicationCommands.Find))
            {
                _keyBinding = new KeyBinding(ApplicationCommands.Find, Key.F, ModifierKeys.Control);
                _owner.InputBindings.Add(_keyBinding);
            }

            // Catch Ctrl+F even when a child control would handle the key.
            _owner.PreviewKeyDown += Owner_PreviewKeyDown;
        }

        public void UnregisterBindings()
        {
            if (_commandBinding != null && _owner.CommandBindings.Contains(_commandBinding))
                _owner.CommandBindings.Remove(_commandBinding);

            if (_keyBinding != null && _owner.InputBindings.Contains(_keyBinding))
                _owner.InputBindings.Remove(_keyBinding);

            _owner.PreviewKeyDown -= Owner_PreviewKeyDown;
        }

        private void Owner_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            // Tunneling event runs before focused child handles the key.
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // avoid re-entrancy if dialog already open
                if (_isFindDialogOpen) return;

                e.Handled = true;
                ShowFindDialog();
            }
        }

        private void FindCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _reader?.Document != null;
            e.Handled = true;
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShowFindDialog();
            e.Handled = true;
        }

        public void ShowFindDialog()
        {
            if (_isFindDialogOpen) return;
            _isFindDialogOpen = true;

            try
            {
                var dlg = new Window
                {
                    Title = "Find",
                    Owner = _owner,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize,
                    Background = Brushes.Turquoise,
                    ShowInTaskbar = false
                };

                var txt = new TextBox { Width = 320, Margin = new Thickness(6) };
                var chk = new CheckBox { Content = "Match case", Margin = new Thickness(6, 6, 6, 0) };
                var btnNext = new Button { Content = "Find Next", Margin = new Thickness(6) };
                var btnPrev = new Button { Content = "Find Previous", Margin = new Thickness(6) };
                var btnClose = new Button { Content = "Close", Margin = new Thickness(6) };

                var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                buttons.Children.Add(btnPrev);
                buttons.Children.Add(btnNext);
                buttons.Children.Add(btnClose);

                var stack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(8) };
                stack.Children.Add(new TextBlock { Text = "Find text:", Margin = new Thickness(6, 0, 6, 0) });
                stack.Children.Add(txt);
                stack.Children.Add(chk);
                stack.Children.Add(buttons);

                dlg.Content = stack;

                btnNext.Click += (_, _) => DoFindNext(txt.Text, chk.IsChecked == true);
                btnPrev.Click += (_, _) => DoFindPrevious(txt.Text, chk.IsChecked == true);
                btnClose.Click += (_, _) => dlg.Close();

                txt.KeyDown += (s, ke) =>
                {
                    if (ke.Key == Key.Enter)
                    {
                        DoFindNext(txt.Text, chk.IsChecked == true);
                        ke.Handled = true;
                    }
                    else if (ke.Key == Key.Escape)
                    {
                        dlg.Close();
                        ke.Handled = true;
                    }
                };

                txt.Text = _lastQuery ?? string.Empty;

                dlg.Loaded += (_, __) =>
                {
                    // Ensure keyboard focus and selection after the window is visible
                    txt.Focus();
                    Keyboard.Focus(txt);
                    txt.SelectAll();
                    // Also set logical focused element for keyboard navigation
                    FocusManager.SetFocusedElement(dlg, txt);
                };
                RemoveAllHighlights(); 
                dlg.ShowDialog();
            }
            finally
            {
                _isFindDialogOpen = false;
            }
        }

        // public helpers if you want programmatic control:
        public void FindNext(string query, bool matchCase = false) => DoFindNext(query, matchCase);
        public void FindPrevious(string query, bool matchCase = false) => DoFindPrevious(query, matchCase);
        public void ClearHighlights() => ClearFindHighlights();

        // internal search/navigation
        private void DoFindNext(string query, bool matchCase)
        {
            if (string.IsNullOrEmpty(query))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (!string.Equals(query, _lastQuery, StringComparison.Ordinal))
            {
                _matchRanges = FindAllMatches(query, matchCase);
                _currentMatchIndex = _matchRanges.Count > 0 ? 0 : -1;
                _lastQuery = query;
            }
            else
            {
                if (_matchRanges.Count > 0)
                {
                    _currentMatchIndex++;
                    if (_currentMatchIndex >= _matchRanges.Count) _currentMatchIndex = 0;
                }
            }

            if (_matchRanges.Count == 0)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            HighlightCurrentMatch();
        }

        private void DoFindPrevious(string query, bool matchCase)
        {
            if (string.IsNullOrEmpty(query))
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            if (!string.Equals(query, _lastQuery, StringComparison.Ordinal))
            {
                _matchRanges = FindAllMatches(query, matchCase);
                _currentMatchIndex = _matchRanges.Count - 1;
                _lastQuery = query;
            }
            else
            {
                if (_matchRanges.Count > 0)
                {
                    _currentMatchIndex--;
                    if (_currentMatchIndex < 0) _currentMatchIndex = _matchRanges.Count - 1;
                }
            }

            if (_matchRanges.Count == 0)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            HighlightCurrentMatch();
        }

        private List<TextRange> FindAllMatches(string query, bool matchCase)
        {
            var doc = _reader?.Document;
            var results = new List<TextRange>();
            if (doc == null || string.IsNullOrEmpty(query)) return results;

            var comparison = matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            for (TextPointer tp = doc.ContentStart; tp != null && tp.CompareTo(doc.ContentEnd) < 0; tp = tp.GetNextContextPosition(LogicalDirection.Forward))
            {
                if (tp.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
                    continue;

                string runText = tp.GetTextInRun(LogicalDirection.Forward);
                int searchIndex = 0;
                while (searchIndex < runText.Length)
                {
                    int idx = runText.IndexOf(query, searchIndex, comparison);
                    if (idx < 0) break;

                    TextPointer matchStart = tp.GetPositionAtOffset(idx, LogicalDirection.Forward);
                    if (matchStart == null) break;
                    TextPointer matchEnd = matchStart.GetPositionAtOffset(query.Length, LogicalDirection.Forward);
                    if (matchEnd == null) break;

                    results.Add(new TextRange(matchStart, matchEnd));
                    searchIndex = idx + query.Length;
                }
            }

            return results;
        }

        private void HighlightCurrentMatch()
        {
            ClearFindHighlights();
            if (_currentMatchIndex < 0 || _currentMatchIndex >= _matchRanges.Count) return;

            var current = _matchRanges[_currentMatchIndex];
            current.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            current.Start.Paragraph?.BringIntoView();
            _reader.Focus();
        }

        private void ClearFindHighlights()
        {
            foreach (var r in _matchRanges)
                r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightYellow);
        }
        private void RemoveAllHighlights()
        {
            foreach (var r in _matchRanges)
                r.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
        }
    }
}