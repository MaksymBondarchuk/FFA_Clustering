// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestResultsWindow.xaml.cs" company="NTUU 'KPI'">
//   Created by Max Bondarchuk
// </copyright>
// <summary>
//   Findow for test results
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FFA_Clustering
{
    using System;

    /// <summary>
    /// Window for test results
    /// </summary>
    public partial class TestResultsWindow : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestResultsWindow"/> class. 
        /// Constructor
        /// </summary>
        public TestResultsWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// On window close
        /// </summary>
        /// <param name="sender">Parent</param>
        /// <param name="e">Events</param>
        private void WindowClosed(object sender, System.EventArgs e)
        {
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
