// -----------------------------------------------------------------------
// Global using aliases — resolve WPF vs WinForms naming conflicts.
//
// UseWindowsForms adds System.Windows.Forms and System.Drawing to the
// implicit global namespace, which conflicts with several WPF / Win32 types.
// These aliases restore the WPF-preferred name for every project-wide conflict.
//
// Only MainWindow.xaml.cs opts in to WinForms types explicitly via:
//   using WinForms = System.Windows.Forms;
// and uses fully qualified System.Drawing.* names where needed.
// -----------------------------------------------------------------------

// WPF input / UI types take precedence over WinForms equivalents
global using Application    = System.Windows.Application;
global using KeyEventArgs   = System.Windows.Input.KeyEventArgs;
global using MessageBox     = System.Windows.MessageBox;
global using TextBox        = System.Windows.Controls.TextBox;
global using UserControl    = System.Windows.Controls.UserControl;

// WPF dialog types take precedence over WinForms equivalents
global using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
global using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

// Color is ambiguous between System.Drawing.Color and OpenXml's Color element.
// Each file resolves it with a local alias rather than a global one.
