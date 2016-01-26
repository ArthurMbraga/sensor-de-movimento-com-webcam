using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Runtime.InteropServices;
using Sensor_de_movimento.Properties;
using System.Media;

namespace Sensor_de_movimento
{

    public partial class Sensor : Form 
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

        private FilterInfoCollection webcam;
        private VideoCaptureDevice cam;
        private Bitmap padrao;
        private Bitmap imagem;
        private int a = 0;
        public Color[] paleta = new Color[2] { Color.White, Color.Black };
        int maiorerro = 0;
        int numerotestes = 0;
        int falhas = 0;
        int sequencia = 0;
        Boolean redimen = false;
        Boolean inverter2 = false;
        Boolean movimentofirst = true;
        Boolean alarme = false;
        Bitmap movimentomap;
        String folderImagens;
        Bitmap bit4;
        Size dimencao;

        int TogMove; 
        int MValX; 
        int MValY;

        SoundPlayer alarmesound;
        FormCalibracao dialogo = new FormCalibracao();

        public Sensor()        
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Sensor());
        }

        private void trackBarSensibilidade_ValueChanged(object sender, EventArgs e)
        {
            value.Text = trackBarSensibilidade.Value + "%";

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webcam = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in webcam)
            {
                comboBox1.Items.Add(VideoCaptureDevice.Name);
            }
            try {

                comboBox1.SelectedIndex = 0;
            }
            catch { }
            comboBox2.SelectedIndex = 0;
            panelacoes.Location = new Point(658, panelacoes.Location.Y);
            panelacoes.Width = 314;
            panel7.Height = 0;
            load();
    
            
        }

        private void load()
        {
            int sensibilidade = (int) Settings.Default["sensibilidade"];
            string diretorioimagens = (string)Settings.Default["diretorioimagens"];
            string diretoriosom = (string)Settings.Default["diretoriosom"];
            int quadros = (int)Settings.Default["quadros"];
            string textocalibra = (string)Settings.Default["textocalibra"];
            int index1 = (int)Settings.Default["index1"];
            Boolean check0 = (Boolean)Settings.Default["check0"];
            Boolean check1 = (Boolean)Settings.Default["check1"];
            Boolean check2 = (Boolean)Settings.Default["check2"];
            Boolean check3 = (Boolean)Settings.Default["check3"];
            Boolean check4 = (Boolean)Settings.Default["check4"];
            Boolean check5 = (Boolean)Settings.Default["check5"];
            Boolean check6 = (Boolean)Settings.Default["check6"];
            Boolean check7 = (Boolean)Settings.Default["check7"];
            Boolean check8 = (Boolean)Settings.Default["check8"];
            int index2 = (int)Settings.Default["index2"];

            trackBarSensibilidade.Value = sensibilidade;
            labelDiretorio1.Text = diretorioimagens;
            label14.Text = diretoriosom;
            trackBarClonagem.Value = quadros;
            labelStatusCalibrado.Text = textocalibra;
            try
            {
                comboBox1.SelectedIndex = index1;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                comboBox1.SelectedIndex = 0;
            }
            checkBoxMinimizarJanelas.Checked = check0;
            checkBoxSomSistema.Checked = check1;
            checkBoxFoto.Checked = check2;
            checkBoxAlarme.Checked = check3;
            checkBoxCobrirTela.Checked = check4;
            checkBoxData.Checked = check5;
            checkBoxHora.Checked = check6;
            checkBox4.Checked = check7;
            checkBoxExtra.Checked = check8;
            try
            {
                comboBox2.SelectedIndex = index2;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                comboBox2.SelectedIndex = 0;
            }
        }

        void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try {
                Bitmap bit = (Bitmap)eventArgs.Frame.Clone();
                bit4 = (Bitmap)eventArgs.Frame.Clone();
                Color c;
                redimencionar(bit.Width, bit.Height);
                Bitmap bit2 = new Bitmap(bit, bit.Size);
                bit = new Bitmap(bit, new Size(200, 200));
                for (int y = 0; y < bit.Height; y++)

                    for (int x = 0; x < bit.Width; x++)
                    {
                        c = bit.GetPixel(x, y);
                        int luma = (int)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);
                        bit.SetPixel(x, y, luma > 100 ? Color.White : Color.Black);
                    }

                Bitmap bit3 = new Bitmap(bit2, dimencao);
                pictureBox1.Image = bit3;
                pictureBox4.Image = bit;
            }
            catch { }
        }

        private void redimencionar(int width, int height)
        {
            int subtair;
            try
            {
                if (redimen == true)
                {
                    if (width > 640)
                    {
                        subtair = width - 640;
                        width -= subtair;
                        height -= subtair;
                    }
                }

                if (height > 360)
                {
                    subtair = height - 360;
                    height -= subtair;
                    width -= subtair;
                }
                
                dimencao = new Size(width, height);
                redimen = false;

            }
            catch (System.InvalidOperationException) { }

        }

        private void button1_Click(object sender, EventArgs e)
        {  
        }
       
        private void timerf_Tick(object sender, EventArgs e)
        {
            if (a == 0)
            {
                padrao = new Bitmap(pictureBox4.Image);
            }

            a = 1;

            imagem = new Bitmap(pictureBox4.Image);

            int totalSize = padrao.Height * padrao.Width;
            int compara = comparar(padrao, imagem);

            int porcentagem = compara * 100 / totalSize;

            if (porcentagem < trackBarSensibilidade.Value)
            {
                falhas = 0;
                labelEstado.Text = "Parado";
                labelEstado.BackColor = Color.FromArgb(39, 174, 96);
                movimentofirst = true;
            }
            else
            {
                falhas = falhas + 1;
                labelEstado.Text = "Movimento";
                labelEstado.BackColor = Color.FromArgb(192, 57, 43);
                movimento();
                movimentofirst = false;
            }

            if (falhas >= trackBarClonagem.Value)
            {
                padrao = new Bitmap(pictureBox4.Image);
                falhas = 0;
            }
        }

        private int comparar(Bitmap b1, Bitmap b2)
        {
            movimentomap = new Bitmap(200, 200);
            int erros = 0;
            for (int y = 0; y < b1.Height; y++)
            {
                for (int x = 0; x < b1.Width; x++)
                {
                    Color c1 = b1.GetPixel(x, y);
                    Color c2 = b2.GetPixel(x, y);

                    if (c1 != c2)
                    {   
                        movimentomap.SetPixel(x, y, Color.Red);
                        pictureBox6.Image = movimentomap;
                        erros += 1;
                    }
                }

            }

            return erros;
        }

        private void timerCalibrar2_Tick(object sender, EventArgs e)
        {
            imagem = new Bitmap(pictureBox4.Image);
            int erro = comparar(padrao, imagem);
            
            if (erro > maiorerro)
            {
                maiorerro = erro;
                dialogo.updateMaiorErro(maiorerro,imagem.Size);
            }
            if (numerotestes >= 100)
            {
                timerCalibrar.Enabled = false;
                concluirCalibrar();
            }
            numerotestes = numerotestes + 1;
            dialogo.addValue();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            padrao = new Bitmap(pictureBox4.Image);
            dialogo.ShowDialog(this);
            Boolean iniciar = dialogo.dialogo1();
            if (iniciar == true)
            {
                desligarsensor();
                trackBarSensibilidade.Value = 1;
                dialogo.dialogo2();
                timerCalibrar.Enabled = true;
                dialogo.ShowDialog(this);
                labelStatusCalibrado.Text = "Mínimo recomendado pela \n ultima calibração: " + trackBarSensibilidade.Value + "%";
            }
        }

        private void concluirCalibrar()
        {
            int totalSize = padrao.Height * padrao.Width;
            int value = maiorerro * 100 / totalSize;
            value = value + 5;
            if (value > 100)
            {
                value = 100;
            }
            movimentomap = new Bitmap(200, 200);
            pictureBox6.Image = movimentomap;
            trackBarSensibilidade.Value = value;
            numerotestes = 0;
            maiorerro = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelStatusCalibrado.Text = "Não Calibrado";
            try
            {
                cam.Stop();
            }
            catch(System.NullReferenceException) 
            {

            }

            cam = new VideoCaptureDevice(webcam[comboBox1.SelectedIndex].MonikerString);
            cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);
            redimen = true;
            cam.Start();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            TogMove = 1;
            MValX = e.X; 
            MValY = e.Y; 
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            TogMove = 0; 
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (TogMove == 1)
            {
                this.SetDesktopLocation(MousePosition.X - MValX, MousePosition.Y - MValY);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            notifyIcon1.Visible = false;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(100);
        }

        private void label3_Click(object sender, System.EventArgs e)
        {

        }

        private void pictureBox1_MouseEnter(object sender, System.EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
            label5.Visible = true;
        }

        private void pictureBox1_MouseLeave_1(object sender, System.EventArgs e)
        {
            inverter2 = true;
            timerdispositivo.Enabled = true;
            label5.Visible = false;
        }

        private void pictureBox4_MouseEnter(object sender, System.EventArgs e)
        {
            label1.Visible = true;
        }

        private void pictureBox4_MouseLeave(object sender, System.EventArgs e)
        {
            label1.Visible = false;
        }

        private void panel4_MouseEnter(object sender, System.EventArgs e)
        {
            
        }

        private void pictureBox5_MouseEnter(object sender, System.EventArgs e)
        {
         
        }

        private void pictureBox5_MouseLeave(object sender, System.EventArgs e)
        {

        }

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        private void movimento()
        {
            if (movimentofirst)
            {
                if (checkBoxMinimizarJanelas.Checked)
                {
                    Shell32.Shell objShel = new Shell32.Shell();
                    this.Hide();
                    this.Show();
                    this.Hide();
                    this.Show();
                    ((Shell32.IShellDispatch4)objShel).ToggleDesktop();
                    this.Hide();
                    notifyIcon1.Visible = true;
                }

                if (checkBoxSomSistema.Checked)
                {
                    SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                }

                if (checkBoxCobrirTela.Checked)
                {
                    Fullscreen full = new Fullscreen();
                    full.Show();
                }

                if (checkBoxAlarme.Checked && !checkBox4.Checked)
                {
                    alarmesound.Play();
                }
                else if (checkBoxAlarme.Checked && checkBox4.Checked && !alarme)
                {
                    alarme = true;
                    alarmesound.PlayLooping();
                }
            }
            if (checkBoxFoto.Checked)
            {
                sequenciaimagem();
            }
        }

        private void sequenciaimagem()
        {
            @folderImagens = @labelDiretorio1.Text;
            @folderImagens += @"\#" + sequencia;

            string data = DateTime.Now.ToString("dd/MM/yy").Replace('/','-');
            string hora = DateTime.Now.ToString("HH:mm:ss").Replace(':', '.');

            if (checkBoxData.Checked && checkBoxHora.Checked)
            {
                @folderImagens += @" D=" + @data + @" H=" + @hora;
            }else
            
            if (checkBoxData.Checked)
            {
                @folderImagens += @" D=" + @data;
            }
            else if (checkBoxHora.Checked)
            {
                @folderImagens += @" H=" + @hora;
            }
            
            @folderImagens += " .jpg";
            try
            {
                Bitmap bitc = new Bitmap(bit4.Width,bit4.Height);
                Console.WriteLine(@folderImagens);
                bit4.Save(@folderImagens);
            }
            catch
            {
                try
                {
                    timerf.Enabled = false;
                }
                catch { }

                FormErro fe = new FormErro();
                String texto = "Falha ao armazenar as imagens";
                String titulo = "Erro";
                fe.ShowDialog(titulo, texto);
            }
            sequencia++;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void timerTopBottom_Tick(object sender, EventArgs e)
        {
            if (!timerf.Enabled)
            {
                int h = panel5.Height;
                if (!inverter2)
                {
                    h += 2;
                    if (h >= 50)
                    {
                        panel5.Height = 50;
                        h = 50;
                        timerdispositivo.Enabled = false;
                    }
                    panel5.Height = h;
                }
                else
                {
                    h -= 2;
                    if (h <= 0)
                    {
                        panel5.Height = 0;
                        h = 0;
                        timerdispositivo.Enabled = false;
                    }
                    panel5.Height = h;
                }
            }
            else
            {
                timerdispositivo.Enabled = false;
                panel5.Height = 0;
            }
        }

        private void label4_MouseEnter(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            inverter2 = true;
            timerdispositivo.Enabled = true;
        }

        private void panel5_DragEnter(object sender, DragEventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void panel5_DragLeave(object sender, EventArgs e)
        {
            inverter2 = true;
            timerdispositivo.Enabled = true;
        }

        private void panel5_MouseEnter(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void panel5_MouseLeave(object sender, EventArgs e)
        {
            inverter2 = true;
            timerdispositivo.Enabled = true;
        }

        private void comboBox1_MouseHover(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void label7_Click(object sender, EventArgs e)
        {
           
            
        }

        private void labelEstado_Click(object sender, EventArgs e)
        {
          
        }

        private void buttonSensor_Click(object sender, EventArgs e)
        {
            if(checkBoxFoto.Checked && labelDiretorio1.Text == "Indefinido"){
                FormErro fe = new FormErro();
                String texto = "Por favor selecione a pasta destinada para salvar as fotos";
                String titulo = "Erro";
                fe.ShowDialog(titulo, texto);
                
            }
            else if (checkBoxAlarme.Checked && comboBox2.SelectedIndex == 4 && label14.Text == "Indefinido")
            {
                FormErro fe = new FormErro();
                fe.ShowDialog("Erro", "Erro ao ler o arquivo .wav do alarme.");
                
            }else

            if (!timerf.Enabled)
            {
                toolStripMenuItem2.Visible = true;
                toolStripMenuItem3.Visible = false;
                padrao = new Bitmap(pictureBox4.Image);
                buttonSensor.BackColor = Color.FromArgb(192, 57, 43);
                buttonSensor.Text = "Desligar Sensor";
                timerf.Enabled = true;
                panelacoes.Enabled = false;
                panel2.Enabled = false;
                panel3.Enabled = false;
                sequencia = 0;
            }
            else
            {
               desligarsensor();
            }
        }

        private void desligarsensor()
        {

            movimentomap = new Bitmap(200, 200);
            pictureBox6.Image = movimentomap;
            timerf.Enabled = false;
            panelacoes.Enabled = true;
            buttonSensor.Text = "Ligar Sensor";
            buttonSensor.BackColor = Color.FromArgb(39, 174, 96);
            labelEstado.BackColor = Color.FromArgb(149, 165, 166);
            labelEstado.Text = "Desligado";
            alarme = false;
            panel2.Enabled = true;
            panel3.Enabled = true;
            toolStripMenuItem2.Visible = false;
            toolStripMenuItem3.Visible = true;
            try
            {
                alarmesound.Stop();
            }
            catch { }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cam.IsRunning)
            {
                cam.Stop();
            }
            Settings.Default["sensibilidade"] = trackBarSensibilidade.Value;
            Settings.Default["diretorioimagens"] = labelDiretorio1.Text;
            Settings.Default["diretoriosom"] = label14.Text;
            Settings.Default["quadros"] = trackBarClonagem.Value;
            Settings.Default["textocalibra"] = labelStatusCalibrado.Text;
            Settings.Default["index1"] = comboBox1.SelectedIndex;
            Settings.Default["check0"] = checkBoxMinimizarJanelas.Checked;
            Settings.Default["check1"] = checkBoxSomSistema.Checked;
            Settings.Default["check2"] = checkBoxFoto.Checked;
            Settings.Default["check3"] = checkBoxAlarme.Checked;
            Settings.Default["check4"] = checkBoxCobrirTela.Checked;
            Settings.Default["check5"] = checkBoxData.Checked;
            Settings.Default["check6"] = checkBoxHora.Checked;
            Settings.Default["check7"] = checkBox4.Checked;
            Settings.Default["check8"] = checkBoxExtra.Checked;
            Settings.Default["index2"] = comboBox2.SelectedIndex;

            Settings.Default.Save();
        }

        private void trackBarSensibilidade_Scroll(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox6_MouseEnter(object sender, EventArgs e)
        {
            label8.Visible = true;
        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {
            label8.Visible = false;
        }

       
        private void checkBoxHora_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBoxData_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label13.Text = trackBarClonagem.Value + " Quadros";
            if (trackBarClonagem.Value == 1)
            {
                label13.Text = trackBarClonagem.Value + " Quadro";
            }
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String folderName = folderBrowserDialog1.SelectedPath;
                labelDiretorio1.Text = folderName;
            }
        }

      

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 4)
            {
                panel4.Visible = true;
            }
            else
            {
                panel4.Visible = false;
            }

            if (comboBox2.SelectedIndex == 0)
            {
                alarmesound = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Alarme1);
            }
            else if (comboBox2.SelectedIndex == 1)
            {
                alarmesound = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Alarme2);
            }
            else if (comboBox2.SelectedIndex == 2)
            {
                alarmesound = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Alarme3);
            }
            else if (comboBox2.SelectedIndex == 3)
            {
                alarmesound = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Sirene);
            }
            else if (comboBox2.SelectedIndex == 4)
            {
                alarmesound = new SoundPlayer(label14.Text);
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                SoundPlayer audio = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Alarme1); 
                audio.Play();
            }
            else if (comboBox2.SelectedIndex == 1){
                
                SoundPlayer audio = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Alarme2); 
                audio.Play();
            }
            else if (comboBox2.SelectedIndex == 2)
            {

                SoundPlayer audio = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Alarme3);
                audio.Play();
            }
            else if (comboBox2.SelectedIndex == 3)
            {

                SoundPlayer audio = new SoundPlayer(Sensor_de_movimento.Properties.Resources.Sirene); 
                audio.Play();
            }
            else if (comboBox2.SelectedIndex == 4)
            {
                try
                {
                    SoundPlayer audio = new SoundPlayer(@label14.Text);
                    audio.Play();
                }
                catch
                {
                    FormErro fe = new FormErro();
                    fe.ShowDialog("Erro", "Erro ao ler o arquivo .wav do alarme.");
                }
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void panel3_VisibleChanged(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void panel2_VisibleChanged(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int w = panelacoes.Size.Width;
            int x = panelacoes.Location.X;
            int y = panelacoes.Location.Y;
            if (panel3.Visible == false && panel2.Visible == false)
            {
                if (w <= 314)
                {
                    w = 314;
                    x = 658;
                    panelacoes.Location = new Point(x,y);
                    panelacoes.Width = w;
                    timer1.Enabled = false;
                }
                else
                {
                    w -= 5;
                    x += 5;
                    panelacoes.Width = w;
                    panelacoes.Location = new Point(x, y);
                }
            }
            else
            {
                if (w >= 548)
                {
                    w = 548;
                    x = 424;
                    panelacoes.Location = new Point(x, y);
                    panelacoes.Width = w;
                    timer1.Enabled = false;
                }
                else
                {
                    x -= 5;
                    w += 5;
                    panelacoes.Width = w;
                    panelacoes.Location = new Point(x, y);
                }
            }
        }

        private void checkBoxAlarme_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAlarme.Checked)
            {
                panel2.Visible = true;
            }
            else
            {
                panel2.Visible = false;
            }
        }

        private void checkBoxFoto_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxFoto.Checked)
            {
                panel3.Visible = true;
            }
            else
            {
                panel3.Visible = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                String folder2 = openFileDialog1.FileName;
                label14.Text = folder2;
                alarmesound = new SoundPlayer(folder2);
            }
        }

        private void comboBox1_Enter(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void comboBox1_DrawItem(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void comboBox1_MouseEnter(object sender, EventArgs e)
        {
            inverter2 = false;
            timerdispositivo.Enabled = true;
        }

        private void timerextra_Tick(object sender, EventArgs e)
        {
            
            int h = panel7.Height;
            if (checkBoxExtra.Checked)
            {
                if (h >= 72)
                {
                    h = 72;
                    panel7.Height = h;
                    timerextra.Enabled = false;
                }
                else
                {
                    panel7.Visible = true;
                    panel7.Height = h+3;
                }
            }
            else
            {
                if (h <= 0)
                {
                    h = 0;
                    panel7.Height = h;
                    panel7.Visible = false;
                    timerextra.Enabled = false;
                }
                else
                {
                    panel7.Height = h-3;
                }
            }
        }

        private void checkBoxExtra_CheckedChanged(object sender, EventArgs e)
        {
            timerextra.Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            desligarsensor();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (checkBoxFoto.Checked && labelDiretorio1.Text == "Indefinido" || checkBoxAlarme.Checked && comboBox2.SelectedIndex == 4 && label14.Text == "Indefinido")
            {
                notifyIcon1.Text = "Falha ao ligar o sensor";
                notifyIcon1.ShowBalloonTip(5,"Erro","Falha ao ligar o sensor",ToolTipIcon.Error);
            }
            else if (!timerf.Enabled)
            {
                toolStripMenuItem2.Visible = true;
                toolStripMenuItem3.Visible = false;
                padrao = new Bitmap(pictureBox4.Image);
                buttonSensor.BackColor = Color.FromArgb(192, 57, 43);
                buttonSensor.Text = "Desligar Sensor";
                timerf.Enabled = true;
                panelacoes.Enabled = false;
                panel2.Enabled = false;
                panel3.Enabled = false;
                sequencia = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Fullscreen full = new Fullscreen();
            full.Show();
        }
    }
}
