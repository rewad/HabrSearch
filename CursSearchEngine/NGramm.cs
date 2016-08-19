using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CursIndexer;
using System.IO;
namespace CursSearchEngine
{
    public class NGramm
    {
        private Dictionary<string, List<string>> m_BigrammIndex;
        private string m_NameDict;

        public List<string> GetTake10(string word)
        {
            if (m_BigrammIndex.ContainsKey(word))
            {
                return new List<string>(m_BigrammIndex[word].Take(Math.Min(10, m_BigrammIndex[word].Count)));
            }
            return new List<string>();
        }
        public static bool IsAllLetters(string s)
        {
            foreach (char c in s)
            {
                if (!Char.IsLetter(c))
                    return false;
            }
            return true;
        }
        public void LoadBigram(string file_name, string text)
        {
            string[] s = File.ReadAllLines(text, Encoding.Default);
            List<string> words = new List<string>();

            Dictionary<string, Dictionary<string, int>> temp_dict = new Dictionary<string, Dictionary<string, int>>();

            for (int k = 0; k < s.Length; k += 10)
            {

                if (s.Count() <= k) break;
                string[] tokens = ParserUtils.Tokenization(s[k].ToLower());
                foreach (var token in tokens)
                {
                    if (token.Length > 2 && IsAllLetters(token))
                        words.Add(token);
                }

            }


            for (int j = 0; j < words.Count - 1; j++)
            {
                string word1 = words[j];
                string word2 = words[j + 1];

                Dictionary<string, int> words_ids;
                if (temp_dict.TryGetValue(word1, out words_ids))
                {
                    if (words_ids.ContainsKey(word2)) words_ids[word2]++;
                    else words_ids.Add(word2, 1);
                }
                else
                {
                    temp_dict.Add(word1, new Dictionary<string, int>());
                    temp_dict[word1].Add(word2, 1);
                }
            }

            Serialization(temp_dict);
        }

        public void Serialization(Dictionary<string, Dictionary<string, int>> dict)
        {
            using (BinaryWriter wr = new BinaryWriter(File.Open(Indexer.GetEngineDir() + "ngram.bin", FileMode.OpenOrCreate)))
            {
                wr.Write(dict.Count);
                foreach (var f in dict)
                {
                    var a = new List<KeyValuePair<string, int>>(f.Value.OrderByDescending(x => x.Value).ToList());
                    wr.Write(f.Key);
                    wr.Write(a.Count);
                    foreach (var word in a)
                        wr.Write(word.Key);
                }
            }
        }

        public void Desialization()
        {
            m_BigrammIndex = new Dictionary<string, List<string>>();
            using (BinaryReader rd = new BinaryReader(File.Open(Indexer.GetEngineDir() + "ngram.bin", FileMode.OpenOrCreate)))
            {
                int len = rd.ReadInt32();
                for (int i = 0; i < len; i++)
                {
                    string word = rd.ReadString();
                    m_BigrammIndex.Add(word, new List<string>());
                    int len2 = rd.ReadInt32();
                    for (int j = 0; j < len2; j++)
                        m_BigrammIndex[word].Add(rd.ReadString());
                }
            }



        }
    }
}
