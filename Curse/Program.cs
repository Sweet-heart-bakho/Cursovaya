using DataCode;	
using NAudio.Wave;

namespace LogicCode
{
	public class LogicClass
	{
		DataCreator a = new DataCreator();
		List<DataCreator> getted = new List<DataCreator> ();
		public LogicClass() { }
		public int CreateUser(string filename, string name)
		{
			//filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Аудиозаписи\Запись_МК3.m4a";
			bool trash = false;
			string buf = "";
			if (filename.Substring(filename.Length - 4, 4) == ".m4a")
            {
				trash = true;
				filename = m4aTowav(filename);
			}
			//конвертер с m4a на wav

			if (Bitrate(filename))
			{
				if (trash)
				{
					buf = filename;
					filename = FixWav(filename);
					File.Delete(buf);
					buf = filename;
				}
				else
				{
					filename = FixWav(filename);
					buf = filename;
					trash = true;
				}
			}


			getted = a.GetData();

			a.data = please(filename);
			a.UserId = rnd(getted);
			a.UserName = name;

			getted.Add(a);
			a.UpdateData(getted);

			if (trash)
				File.Delete(buf);

			return a.UserId;
		}
		public string m4aTowav(string infile)                  //конвертер из ma4 в wav файл
		{
			string outfile = infile.Substring(0, infile.Length - 4) + "_convert.wav";

			using (var reader = new MediaFoundationReader(infile))
			{
				WaveFileWriter.CreateWaveFile(outfile, reader);
			}
			return outfile;
		}
		public string FixWav(string infile)
        {
			using (var reader = new WaveFileReader(infile))        //перевод файлов в нужный нам бирейт
			{
				infile = infile.Substring(0, infile.Length - 4) + "_fixed.wav";
				var newFormat = new WaveFormat(8000, 16, 1);
				using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
				{
					WaveFileWriter.CreateWaveFile(infile, conversionStream);
				}
			}
			return infile;
        }
		public double Compare(int speaker_1, int speaker_2)       //Пред метод сравнения
		{
			double finish;
			DataCreator b = a.GetUserID(speaker_1);
			a = a.GetUserID(speaker_2);

			if (b.data.Length > a.data.Length)
				finish = Range(a.data, b.data);
			else
				finish = Range(b.data, a.data);
			return finish;
		}
		public bool DeleteUser(int speaker)         //Удаление данных о User
		{
			if (a.DeleteUser(speaker))
				return true;
			return false;
		}
		public List<string[]> ShowData()            //Вытаскивание информации из Data и удобной передачи в UI
		{
			List<string[]> data = new List<string[]>();
			getted = a.GetData();
			for (int c = 0; c < getted.Count; c++)
			{
				data.Add(new string[2]);
				data[data.Count - 1][0] = getted[c].UserId.ToString();
				data[data.Count - 1][1] = getted[c].UserName;
			}
			return data;
		}
		public List<string> ComboxChoice()				//передача данных для ComboBox
		{
			List<string> data = new List<string>();
			getted = a.GetData();
			for (int i = 0; i < getted.Count; i++)
				data.Add(getted[i].UserId.ToString() + " " + getted[i].UserName);
			return data;
		}
		public bool FileCheck (string filename)
        {
			if (filename.Length == 0)
				return false;
			char[] invalidsymbols = Path.GetInvalidFileNameChars();
			for (int i = 0; i < filename.Length; i++)
				foreach (char c in invalidsymbols)
					if (c == filename[i])
						return false;
			return true;
		}
		public int rnd(List<DataCreator> getted)				//генератор случайного int числа
		{
			int[] ids = a.GetUsersID();
			int rand;
			Random random = new Random();
			while (true)
			{
				rand = random.Next(1, 100);
				if (ids.Contains(rand))
					continue;
				return rand;
			}
		}
		static double Range(double[] p, double[] q)			//корелляция Пирсона
		{
			double d1 = 0.0;
			double d2 = 0.0;
			double d3 = 0.0;
			for (int i = 0; i < p.Length - 1; i++)
			{
				d1 += (p[i] - p[p.Length - 1]) * (q[i] - q[q.Length - 1]);
				d2 += Math.Pow(p[i] - p[p.Length - 1], 2);
				d3 += Math.Pow(q[i] - q[q.Length - 1], 2);
			}
			d2 = Math.Sqrt(d2);
			d3 = Math.Sqrt(d3);
			d1 /= d2 * d3;
			return Math.Abs(d1);
		}
		static bool Bitrate(string file)				//проверка бирейта файла
        {
			int bitrate;
			using (var f = File.OpenRead(file))
			{
				f.Seek(28, SeekOrigin.Begin);
				byte[] val = new byte[4];
				f.Read(val, 0, 4);
				bitrate = BitConverter.ToInt32(val, 0);
			}
			if (bitrate == 16000)
				return false;
			return true;
		}
		static double[] please(string infile)
		{
			int frame = 128;

			List<double> datalist = new List<double>();					//извлечение данных
			using (WaveFileReader reader = new WaveFileReader(infile))
			{
				byte[] bytesBuffer = new byte[reader.Length];
				int read = reader.Read(bytesBuffer, 0, bytesBuffer.Length);
				for (int sampleIndex = 0; sampleIndex < read / 2; sampleIndex++)
				{
					var intSampleValue = BitConverter.ToInt16(bytesBuffer, sampleIndex * 2);
					datalist.Add(intSampleValue / 32768.0);
				}
			}

			List<List<double>> slices = new List<List<double>>();			//деление массива
			for (int i = 0; i < datalist.Count - frame; i += frame / 2)
			{
				if (i + frame > datalist.Count)
				{
					slices.Add(datalist.GetRange(i, datalist.Count - i));
					break;
				}
				slices.Add(datalist.GetRange(i, frame));
			}
			datalist.Clear();

			for (int i = 0; i < slices.Count; i++)
			{
				datalist.Add(Furie(slices[i], i));
			}

			double Furie(List<double> slices, int f)                        //преобразование Фурье
			{
				double t1 = 0.0;
				double t2 = 0.0;
				for (int t = 0; t < slices.Count / 2; t++)
				{
					t1 += slices[t * 2] * (Math.Cos((2 * Math.PI * t * f) / slices.Count));
					t2 += slices[t * 2 + 1] * (Math.Sin((2 - Math.PI * t * f) / slices.Count));
				}
				t1 = (t1 * 2) / slices.Count;
				t2 = (t2 * 2) / slices.Count;
				t1 = Math.Sqrt(Math.Pow(t1, 2) + Math.Pow(t2, 2));
				return t1;
			}

			datalist = Hemming(datalist);

			List<double> Hemming(List<double> slices)                       //окно Хемминга для устранения нежелательных эффектов
			{
				for (int i = 0; i < slices.Count; i++)
					slices[i] = 0.53836 - (0.46164 * Math.Cos((Math.PI * 2 * slices[i]) / (slices.Count - 1)));
				return slices;
			}

			datalist = M_O(datalist);
			List<double> M_O(List<double> vect)
			{
				double m = 0.0;
				for (int i = 0; i < vect.Count; i++)
				{
					m += vect[i];
				}
				m /= vect.Count;
				vect.Add(m);
				return datalist;
			}

			double[] arr = datalist.ToArray();
			return arr;
		}
		static void Main() { /*LogicClass a = new LogicClass(); a.CreateUser( "", "Кирилл 3");*/ }
	}
}
