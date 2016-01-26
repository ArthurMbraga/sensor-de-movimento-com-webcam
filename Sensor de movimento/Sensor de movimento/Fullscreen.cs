using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sensor_de_movimento
{
    public partial class Fullscreen : Form
    {
        public Fullscreen()
        {
            InitializeComponent();
        }

        private void Fullscreen_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            
            this.Height = 50000;
            this.Width = 50000;
            
            
        }

        private void Fullscreen_DoubleClick(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void Fullscreen_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar==(char)27)
            {
                this.Hide();
            }
        }

        
      
    }
}
