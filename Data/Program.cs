using System.Text.Json;


namespace DataCode
{
    public class DataCreator
    {
        public double[] data { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        string path = @"..\..\..\..\VoiceData.json";
        public DataCreator() { }

        public void UpdateData(List<DataCreator> put)
        {
            string jsonString = JsonSerializer.Serialize(put);
            File.WriteAllText(path, jsonString);
        }

        public List<DataCreator> GetData()
        {
            List<DataCreator> getted = new List<DataCreator>();
            if (!File.Exists(path))
            {
                var myFile = File.Create(path);
                myFile.Close();
            }
            string jsonString = File.ReadAllText(path);
            if (jsonString.Length != 0)
                getted = JsonSerializer.Deserialize<List<DataCreator>>(jsonString);
            return getted;
        }

        public int[] GetUsersID()
        {
            List<DataCreator> getted = GetData();
            int[] ids = new int[getted.Count];
            for (int i = 0; i < getted.Count; i++)
                ids[i] = getted[i].UserId;
            return ids;
        }

        public DataCreator GetUserID(int i)
        {
            List<DataCreator> getted = GetData();
            foreach (DataCreator a in getted)
                if (i == a.UserId)
                    return a;
            return null;
        }

        public bool DeleteUser(int speaker)
        {
            List<DataCreator> getted = GetData();
            for (int c = 0; c < getted.Count; c++)
                if (speaker == getted[c].UserId)
                {
                    getted.RemoveAt(c);
                    UpdateData(getted);
                    return true;
                }
            return false;
        }
    }
}
