using LogicCode;
using NAudio.Wave;

namespace UI
{
    public partial class Form1 : Form
    {
        LogicClass a = new LogicClass();
        WaveIn waveIn;
        WaveFileWriter writer;
        string outputFilename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Аудиозаписи\Запись (4).wav";
        string user1;
        string user2;
        string dictor;
        bool turn = true;
        public Form1()
        {
            InitializeComponent();
            LoadData();
            List<string> data = a.ComboxChoice();
            for (int i = 0; i < data.Count; i++)
            {
                comboBox1.Items.Add(data[i]);
                comboBox2.Items.Add(data[i]);
                comboBox3.Items.Add(data[i]);
            }
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                comboBox4.Items.Add(deviceInfo.ProductName);
            }
        }
        public void Refresh(string user)
        {
            comboBox1.Items.Add(user);
            comboBox2.Items.Add(user);
            comboBox3.Items.Add(user);
        }
        private void LoadData()
        {
            List<string[]> data = a.ShowData();
            foreach (string[] item in data)
                dataGridView1.Rows.Add(item);
        }
        //Получение данных из входного буфера 
        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_DataAvailable), sender, e);
            }
            else
            {
                //Записываем данные из буфера в файл
                writer.WriteData(e.Buffer, 0, e.BytesRecorded);
            }
        }
        //Завершаем запись
        void StopRecording()
        {
            MessageBox.Show("Запись было успешно завершена");
            waveIn.StopRecording();
        }
        //Окончание записи
        private void waveIn_RecordingStopped(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<WaveInEventArgs>(waveIn_RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }
        //Начинаем запись - обработчик нажатия кнопки
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == -1)
            {
                MessageBox.Show("Please choise input device");
                return;
            }
            if (!a.FileCheck(textBox2.Text) && textBox1.Text.Length == 0)
            {
                MessageBox.Show("Wrong file name");
                return;
            }
            dictor = textBox1.Text;
            outputFilename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Аудиозаписи\" + textBox2.Text + ".wav";
            if (turn)
            {
                turn = false;
                try
                {
                    label1.Text = "Запись идет";
                    waveIn = new WaveIn();
                    //Дефолтное устройство для записи (если оно имеется)
                    //встроенный микрофон ноутбука имеет номер 0
                    waveIn.DeviceNumber = comboBox4.SelectedIndex;
                    //Прикрепляем к событию DataAvailable обработчик, возникающий при наличии записываемых данных
                    waveIn.DataAvailable += waveIn_DataAvailable;
                    //Прикрепляем обработчик завершения записи
                    waveIn.RecordingStopped += waveIn_RecordingStopped;
                    //Формат wav-файла - принимает параметры - частоту дискретизации и количество каналов(здесь mono)
                    waveIn.WaveFormat = new WaveFormat(8000, 1);
                    //Инициализируем объект WaveFileWriter
                    writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
                    //Начало записи
                    waveIn.StartRecording();
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }
            }
            //Прерываем запись - обработчик нажатия второй кнопки
            else
            {
                turn = true;
                if (waveIn != null)
                {
                    StopRecording();
                    int id = a.CreateUser(outputFilename, dictor);
                    label1.Text = "Имя диктора и навазние файла";
                    comboBox1.Items.Add(id + " " + dictor);
                    comboBox2.Items.Add(id + " " + dictor);
                    comboBox3.Items.Add(id + " " + dictor);
                    string[] newone = { id.ToString(), dictor };
                    dataGridView1.Rows.Add(newone);
                }

            }
            Refresh();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text.Length != 0 && comboBox2.Text.Length != 0)
            {
                user1 = comboBox1.Text;
                user2 = comboBox2.Text;
                if (a.PreCompare(user1, user2))
                    MessageBox.Show("Same users");
                else
                    MessageBox.Show("Not same");
            }
            else
                MessageBox.Show("Please choose users");
            Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            a.DeleteUser(Convert.ToInt32(comboBox3.Text.Substring(0, comboBox3.Text.IndexOf(' '))));
            dataGridView1.Rows.RemoveAt(comboBox3.SelectedIndex);
            comboBox1.Items.RemoveAt(comboBox3.SelectedIndex);
            comboBox2.Items.RemoveAt(comboBox3.SelectedIndex);
            comboBox3.Items.RemoveAt(comboBox3.SelectedIndex);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}