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
			getted = a.GetData();
			filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Аудиозаписи\Запись (3).m4a";

			a.data = please(filename);
			a.UserId = rnd(getted);
			a.UserName = name;

			getted.Add(a);
			a.UpdateData(getted);

			filename = "User: " + a.UserName + " with ID: " + a.UserId + " was created.";

			return a.UserId;
		}
		public bool Compare(int speaker_1, int speaker_2)       //Пред метод сравнения
		{
			DataCreator a = new DataCreator();
			List<DataCreator> getted = a.GetData();
			double finish = 0.0;

			foreach (DataCreator c in getted)
			{
				if (c.UserId == speaker_1)
					a = c;
				if (c.UserId == speaker_2)
				{
					finish = Range(a.data, c.data);
					break;
				}
			}

			if (finish > 0.1)
				return true;
			return false;
		}
		public bool DeleteUser(int speaker)         //Удаление данных о User
		{
			DataCreator a = new DataCreator();
			List<DataCreator> getted = a.GetData();
			foreach (DataCreator c in getted)
				if (speaker == c.UserId)
				{
					a = c;
					a.DeleteUser(a.UserId, getted);
					return true;
				}
			return false;
		}
		public List<string[]> ShowData()            //Вытаскивание информации из Data и удобной передачи в UI
		{
			List<string[]> data = new List<string[]>();
			DataCreator a = new DataCreator();
			List<DataCreator> getted = a.GetData();
			for (int c = 0; c < getted.Count; c += 2)
			{
				data.Add(new string[2]);
				data[data.Count - 1][0] = getted[c].UserId.ToString();
				data[data.Count - 1][1] = getted[c].UserName;
			}
			return data;
		}
		public List<string> ComboxChoice()				//передача данных для ComboBox
		{
			DataCreator a = new DataCreator();
			List<string> data = new List<string>();
			List<DataCreator> getted = a.GetData();
			for (int i = 0; i < getted.Count; i++)
				data.Add(getted[i].UserId.ToString() + " " + getted[i].UserName);
			return data;
		}
		public bool PreCompare(string f1, string f2)
        {
			int user1 = Convert.ToInt32(f1.Substring(0, f1.IndexOf(' ')));
			int user2 = Convert.ToInt32(f2.Substring(0, f2.IndexOf(' ')));
			return Compare(user1, user2);
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
		static int rnd(List<DataCreator> getted)				//генератор случайного int числа
		{
			Random random = new Random();
			int a;
			while (true)
			{
				a = random.Next(1, 100);
				foreach (DataCreator c in getted)
					if (c.UserId == a)
						continue;
				return a;
			}
		}
		static double Range(double[] p, double[] q)			//корелляция Пирсона
		{
			double d1 = 0.0;
			double d2 = 0.0;
			double d3 = 0.0;
			for (int i = 0; i < p.Length; i++)
			{
				d1 += (p[i] - p[p.Length - 1]) * (q[i] - q[q.Length - 1]);
				d2 += Math.Pow(p[i] - p[p.Length - 1], 2);
				d3 += Math.Pow(q[i] - q[q.Length - 1], 2);
			}
			d2 = Math.Sqrt(d2);
			d3 = Math.Sqrt(d3);
			d2 *= d3;
			d1 /= d2;
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

			string outfile = ma4Tomav(infile);

			string ma4Tomav(string infile)                  //конвертер из ma4 в wav файл
			{
				string outfile = infile.Substring(0, infile.Length - 4) + "_convert.wav";

				using (var reader = new MediaFoundationReader(infile))
				{
					WaveFileWriter.CreateWaveFile(outfile, reader);
				}
				return outfile;
			}


			if (Bitrate(outfile))
			{
				infile = outfile;
				using (var reader = new WaveFileReader(outfile))        //перевод файлов в нужный нам бирейт
				{
					outfile = outfile.Substring(0, outfile.Length - 4) + "_fixed.wav";
					var newFormat = new WaveFormat(8000, 16, 1);
					using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
					{
						WaveFileWriter.CreateWaveFile(outfile, conversionStream);
					}
				}
				File.Delete(infile);
			}

			List<double> datalist = new List<double>();					//извлечение данных
			using (WaveFileReader reader = new WaveFileReader(outfile))
			{
				byte[] bytesBuffer = new byte[reader.Length];
				int read = reader.Read(bytesBuffer, 0, bytesBuffer.Length);
				for (int sampleIndex = 0; sampleIndex < read / 2; sampleIndex++)
				{
					var intSampleValue = BitConverter.ToInt16(bytesBuffer, sampleIndex * 2);
					datalist.Add(intSampleValue / 32768.0);
				}
			}

			File.Delete(outfile);

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
		static void Main() { LogicClass a = new LogicClass(); a.CreateUser("f", "Bakho"); }
	}
}