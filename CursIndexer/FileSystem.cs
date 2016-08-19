using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics; 
namespace CursIndexer
{
    public class FileSystem
    {

        private string m_fileName;
        private Dictionary<string, Tuple<long, int>> m_dict = new Dictionary<string, Tuple<long, int>>();
        public FileSystem(string name)
        {
            m_fileName = name;
        }
        public int FindMax()
        {
            return m_dict.Max(x => x.Value.Item2);
        }
        public int GetNum()
        {
            return m_dict.Count;
        }
        public string MaxText()
        {
            int s = FindMax();
            return  Encoding.Default.GetString(ReadFile(m_dict.Where(x => x.Value.Item2 == s).ElementAt(0).Key));
        }
        private int end = 0;
        public void WriteDict( )
        {
            int len = m_dict.Count;

            using (BinaryWriter wr = new BinaryWriter(File.Open("C://search_engine//data//dict"+m_fileName, FileMode.OpenOrCreate)))
            {
                wr.Write(len);
                foreach (var f in m_dict)
                {
                    wr.Write(f.Key);
                    wr.Write(f.Value.Item1);
                    wr.Write(f.Value.Item2);
                }
                wr.Write(end);
            }
        }
        public byte[] ReadFile(string name)
        {
            if (!m_dict.ContainsKey(name))
                return null;
            var v = m_dict[name];

            using (BinaryReader rd = new BinaryReader(File.Open("C://search_engine//data//fs"+m_fileName, FileMode.OpenOrCreate)))
            {
                rd.BaseStream.Seek(v.Item1, SeekOrigin.Begin);
                return rd.ReadBytes(v.Item2);
            }
        }
        public void ReadDict()
        {
            m_dict = new Dictionary<string, Tuple<long, int>>();

            using (BinaryReader rd = new BinaryReader(File.Open("C://search_engine//data//dict"+m_fileName, FileMode.OpenOrCreate)))
            {
                int len = rd.ReadInt32();

                for (int i = 0; i < len; i++)
                    m_dict.Add(rd.ReadString(), new Tuple<long, int>(rd.ReadInt64(), rd.ReadInt32()));
                end = rd.ReadInt32();
            }
        }
        public void WriteFile(byte[] file, string name)
        {
            int len = file.Length;
            if (len == 0) return;
            using (BinaryWriter wr = new BinaryWriter(File.Open("C://search_engine//data//fs"+m_fileName, FileMode.Append)))
            {
                m_dict.Add(name.Split('/').Last().Split('\\').Last(), new Tuple<long, int>(end, len));
                wr.Seek(end, SeekOrigin.Begin);
                wr.Write(file);
                end += len;
            }

        }
         
    }
}