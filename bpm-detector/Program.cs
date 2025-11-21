using System;
using System.Windows.Forms;

namespace BPMDetector
{
    /// <summary>
    /// Main entry point for the BPM Detector application
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                // Run the main form
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // Handle any unhandled exceptions
                MessageBox.Show($"Application Error: {ex.Message}", "BPM Detector", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}