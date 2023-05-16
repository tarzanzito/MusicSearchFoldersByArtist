using System;
using System.Windows.Forms;

namespace MusicManager
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //MessageBox.Show(args[0]);
            if (string.IsNullOrEmpty(args[0]))
            {
                return;
            }
            Application.Run(new Form1(args[0]));
        }
    }
}
