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
    public partial class FormCalibracao : Form
    {
        private const int CS_DROPSHADOW = 0x20000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        int value = 0;
        Boolean iniciado = false;
        Boolean dialog;
        public FormCalibracao()
        {
            InitializeComponent();
        }

        public void addValue(){
            value += 1;
            if (value <= 100)
            {
                progressBar1.Value = value;
                label2.Text = value + "%";
            }
            if (value == 100)
            {
                button1.Enabled = true;
                button1.UseWaitCursor = false;
            }
        }

        public void updateMaiorErro(int erro,Size size)
        {
            int w = size.Width;
            int h = size.Height;
            int total = w * h;
            int porcentagem = erro * 100 / total;
            label3.Text = erro + "/" + total + " (" + porcentagem + "%)";
        }

        public  Boolean dialogo1()
        {
            button1.Click += (sender, e) => { dialog = true; };
            button2.Click += (sender, e)  => { dialog = false; };
            return dialog;
        }

        public void dialogo2()
        {
            button1.UseWaitCursor = true;
            button1.Text = "Concluir";
            button1.Enabled =false;
            progressBar1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            button2.Visible = false;
            iniciado = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dialog = true;
            if (iniciado == false)
            {
                this.Close();
            }
            else
            {   
                iniciado = false;
                value = 0;
                progressBar1.Value = 0;
                button1.Text = "Iniciar";
                button1.UseWaitCursor = true;
                label2.Visible = false;
                label3.Visible = false;
                button2.Visible = true;
                dialog = false;
                label2.Text = "0%";
                label3.Text = "0/0 (0%)";
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dialog = false;
            this.Close();
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
           
        }

        private void labeltitulo_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void FormCalibracao_Enter(object sender, EventArgs e)
        {
        
        }

        private void FormCalibracao_Load(object sender, EventArgs e)
        {

        }
    }
}
