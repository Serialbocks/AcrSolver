using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AcrSolver
{
    public partial class uxMainWindow : Form
    {
        public uxMainWindow()
        {
            InitializeComponent();
        }

        private void uxCapture_Click(object sender, EventArgs e)
        {
            var screenshot = Screenshot.PrintWindow();
            if(screenshot != null)
            {
                screenshot.Save("test.jpg", ImageFormat.Jpeg);
            }
        }
    }
}
