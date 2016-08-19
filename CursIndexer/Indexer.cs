using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using Iveonik.Stemmers;
namespace CursIndexer
{
    public class Indexer
    {
        private static string m_workDir = "work//";
        private static string m_engineDir = "C://search_engine//data//";
        private static string m_debugDir = "";

        private IStemmer m_stemmer;

        #region words - id
        private Dictionary<int, string> m_dictIdWordTerms;
        private Dictionary<string, int> m_dictWordIdTerms;
        private int lastIdWord;
        #endregion

        #region dock - id
        private Dictionary<int, string> m_dictIdDock;
        private Dictionary<string, int> m_dictDockId;
        private int m_lastIdDock;
        #endregion


        #region inverted index
        private TIndex m_invertIndex;
        private Dictionary<int, DateTime> m_indexDate;
        #endregion

        #region
        private TCacheTF cacheTF;
        private Dictionary<int, int> m_lengthDocument;
        private Dictionary<int, float> m_cacheIDF;
        private Dictionary<int, float> m_normLenDoc;
        private float avgdl;
        #endregion

        Dictionary<string, string> m_mount = new Dictionary<string, string>();
        public Indexer()
        {
            InitDir();
            m_dictIdWordTerms = new Dictionary<int, string>();
            m_dictWordIdTerms = new Dictionary<string, int>();

            m_dictDockId = new Dictionary<string, int>();
            m_dictIdDock = new Dictionary<int, string>();

            m_invertIndex = new TIndex();
            cacheTF = new TCacheTF();
            m_lengthDocument = new Dictionary<int, int>();
            m_cacheIDF = new Dictionary<int, float>();
            m_normLenDoc = new Dictionary<int, float>();
            m_indexDate = new Dictionary<int, DateTime>();

            m_mount.Add("января", "01");
            m_mount.Add("февраля", "02");
            m_mount.Add("марта", "03");
            m_mount.Add("апреля", "04");
            m_mount.Add("мая", "05");
            m_mount.Add("июня", "06");
            m_mount.Add("июля", "07");
            m_mount.Add("августа", "08");
            m_mount.Add("сентября", "09");
            m_mount.Add("октября", "10");
            m_mount.Add("ноября", "11");
            m_mount.Add("декабря", "12");
            m_stemmer = new RussianStemmer();
        }
        public TIndex GetIndex() { return m_invertIndex; }
        public int GetNumDocuments() { return m_dictDockId.Count; }
        public void InitDir()
        { 
            if (!Directory.Exists(m_debugDir + m_engineDir))
                Directory.CreateDirectory(m_debugDir + m_engineDir); 
        }

        public string[] GetNewDocuments()
        {
            return Directory.GetFiles(m_workDir);
        }

        public string[] Tokenization(string text)
        {
            return ParserUtils.Tokenization(text);
        }

        public static string ReadText(string file, out string date, Func<string, Tuple<string, string>> extractor)
        {
            string file_doc = m_workDir + "html//" + file;
            using (StreamReader reader = new StreamReader(new FileStream(file, FileMode.Open), Encoding.UTF8))
            {
                string read = reader.ReadToEnd();
                read = read.Replace("&nbsp;", " ").Replace("&nbsp", " "); 
                var text = extractor(read);
                date = text.Item2;
                return text.Item1.ToLower();
            }
        }
        public float GetIDF(string term)
        {
            int id;
            if (m_dictWordIdTerms.TryGetValue(term, out id))
            {
                return m_cacheIDF[id];
            }
            return 0.0f;
        }
        public float GetIDF(int term)
        {
            if (m_cacheIDF.ContainsKey(term))
                return m_cacheIDF[term];
            return 0.0f;
        }
        public float GetTF(string term, int doc)
        {
            int id;
            if (m_dictWordIdTerms.TryGetValue(term, out id))
            {

                Dictionary<int, float> cache = cacheTF[id];
                float tf = 0.0f;
                if (cache.TryGetValue(doc, out tf))
                    return tf;
            }
            return 0.0f;
        }
        public float GetTF(int term, int doc)
        {
            if (cacheTF.ContainsKey(term))
            {
                Dictionary<int, float> cache = cacheTF[term];
                float tf = 0.0f;
                if (cache.TryGetValue(doc, out tf))
                    return tf;
            }
            return 0.0f;
        }
        public float GetLenDoc(int doc)
        {
            return m_normLenDoc[doc];
        }
        public int GetIdDictTerm(string term)
        {
            int id_term = 0;
            if (m_dictWordIdTerms.TryGetValue(term, out id_term))
            {
                return id_term;
            }
            id_term = -1;
            return id_term;
        }
        public void Calcavgdl()
        {
            avgdl = 0.0f;
            foreach (var l in m_lengthDocument) avgdl += (float)l.Value;
            avgdl /= m_lengthDocument.Count;

        }
        public float GetAVGDL()
        {
            return avgdl;
        }
        public int GetIDTerm(string term)
        {
            int id_term = 0;
            if (m_dictWordIdTerms.TryGetValue(term, out id_term))
            {
                return id_term;
            }
            id_term = lastIdWord;
            m_dictWordIdTerms.Add(term, id_term);
            m_dictIdWordTerms.Add(id_term, term);
            lastIdWord++;
            return id_term;
        }

        public int GetIDDock(string dock)
        {
            int id_dock = 0;
            if (m_dictDockId.TryGetValue(dock, out id_dock))
            {
                return id_dock;
            }
            id_dock = m_lastIdDock;
            m_dictDockId.Add(dock, id_dock);
            m_dictIdDock.Add(id_dock, dock);
            m_lastIdDock++;
            return id_dock;
        }

        public static string GetEngineDir()
        {
            return m_debugDir + m_engineDir;
        }
        public string GetWorkDir()
        {
            return m_debugDir + m_workDir;
        }
        public string Terminization(string token)
        {
            return m_stemmer.Stem(token);
        }

        public string GetWord(int id)
        {
            string word;
            if (m_dictIdWordTerms.TryGetValue(id, out word))
                return word;
            return "";
        }
        private DateTime ParseDate(string s)
        {
            DateTime date;
            try
            {
                string[] tokens = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (Convert.ToInt32(tokens[0]) < 10) tokens[0] = "0" + tokens[0];
                if (tokens.Length < 5) return DateTime.MinValue;
                string new_date = tokens[0] + m_mount[tokens[1]] + tokens[2] + " " + tokens[4];
                date = DateTime.ParseExact(new_date, "ddMMyyyy HH:mm", null);
            }
            catch
            {
                return DateTime.MinValue;
            }
            return date;

        }
        public void AddDocument(string file_name)
        {
            string date_string = "";
            var text = ReadText(file_name, out date_string, StaticParserHTML.ParseHabr);
            string[] tokens = Tokenization(text);

            ushort positional_index = 0;
            int len = 0;
            int doc_id = GetIDDock(file_name.Split('/').Last());
            DateTime date = ParseDate(date_string);
            m_indexDate.Add(doc_id, date);
            foreach (var token in tokens)
            { 
                len++;
                var term = Terminization(token);
                int term_id = GetIDTerm(term);
                TPostings documents;

                if (m_invertIndex.TryGetValue(term_id, out documents))
                { 
                    if (documents.ContainsKey(doc_id))
                        documents[doc_id]++;               
                    else     
                        documents.Add(doc_id, 1); 
                }
                else
                {
                    var document = new TPostings(); 
                    document.Add(doc_id, 1);
                    m_invertIndex.Add(term_id, document);
                }
                positional_index++;
            }

            m_lengthDocument.Add(doc_id, len);

        }
        public void ComputeTF()
        {
            foreach (var term in m_invertIndex)
            {
                foreach (var doc in term.Value)
                {
                    int id_doc = doc.Key;
                    int count_terms = doc.Value;
                    int len_doc = m_lengthDocument[id_doc];
                    float tf = count_terms / (float)len_doc;
                    TDocumentTF docs_tfs;
                    if (cacheTF.TryGetValue(term.Key, out docs_tfs))
                    {
                        docs_tfs.Add(id_doc, tf);
                    }
                    else
                    {
                        docs_tfs = new TDocumentTF();
                        docs_tfs.Add(id_doc, tf);
                        cacheTF.Add(term.Key, docs_tfs);
                    }
                }
            }

        }
        public void ComputeIDF()
        {
            foreach (var term in m_invertIndex)
            {
                float idf = (float)Math.Log10(m_lengthDocument.Count / (double)term.Value.Count);
                m_cacheIDF.Add(term.Key, idf);
            }
            int num = m_lengthDocument.Sum(x => x.Value);
            avgdl = num / (float)m_lengthDocument.Count;
        }

        public void ComputeLenDocs()
        {
            foreach (var doc in m_dictIdDock) m_normLenDoc.Add(doc.Key, 0);

            foreach (var term in m_invertIndex)
            {
                float idf = m_cacheIDF[term.Key];
                foreach (var docs in term.Value)
                {
                    float tf = cacheTF[term.Key][docs.Key];
                    float w = tf * idf;
                    m_normLenDoc[docs.Key] += w * w;
                }
            }
            Dictionary<int, float> temp = new Dictionary<int, float>();
            foreach (var len in m_normLenDoc) temp.Add(len.Key, (float)Math.Sqrt(m_normLenDoc[len.Key]));
            m_normLenDoc = temp;
        }
        public string GetDoc(int id)
        {
            return m_dictIdDock[id];
        }
        public void Serialization()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            SerializationIndex.Serialization(m_dictIdWordTerms, m_debugDir + m_engineDir + "\\n2w.bin");
            SerializationIndex.Serialization(m_dictWordIdTerms, m_debugDir + m_engineDir + "\\w2n.bin");
            SerializationIndex.Serialization(m_dictIdDock, m_debugDir + m_engineDir + "\\n2d.bin");
            SerializationIndex.Serialization(m_dictDockId, m_debugDir + m_engineDir + "\\d2n.bin"); 
            SerializationIndex.SerializeIndex(m_invertIndex, m_debugDir + m_engineDir + "\\index.bin");
            SerializationIndex.SerializationCacheTF(cacheTF, m_debugDir + m_engineDir + "\\cachetf.bin");
            using (Stream writer = File.Open(m_debugDir + m_engineDir + "\\cachelen.bin", FileMode.Create))
            {
                BinaryFormatter form = new BinaryFormatter();
                form.Serialize(writer, m_lengthDocument);
            }
            using (Stream writer = File.Open(m_debugDir + m_engineDir + "\\cacheidf.bin", FileMode.Create))
            {
                BinaryFormatter form = new BinaryFormatter();
                form.Serialize(writer, m_cacheIDF);
            }
            watch.Stop();
            Console.WriteLine("WriteIndex: " + watch.ElapsedMilliseconds.ToString());
            Calcavgdl();
            ComputeLenDocs();
        }
        
        public void Deserialization()
        {
            m_dictWordIdTerms = SerializationIndex.DeserializationDictSN(m_debugDir + m_engineDir + "\\w2n.bin");
            m_dictIdWordTerms = SerializationIndex.DeserializationDictNS(m_debugDir + m_engineDir + "\\n2w.bin");
            m_dictDockId = SerializationIndex.DeserializationDictSN(m_debugDir + m_engineDir + "\\d2n.bin");
            m_dictIdDock = SerializationIndex.DeserializationDictNS(m_debugDir + m_engineDir + "\\n2d.bin");  
            m_invertIndex = SerializationIndex.DeserializeIndex(m_debugDir + m_engineDir + "\\index.bin"); 
            if (File.Exists(m_debugDir + m_engineDir + "\\cachetf.bin"))
            {
                cacheTF = SerializationIndex.DeserializationTCacheTF(m_debugDir + m_engineDir + "\\cachetf.bin");
            }
            if (File.Exists(m_debugDir + m_engineDir + "\\cachelen.bin"))
            {
                using (Stream reader = File.Open(m_debugDir + m_engineDir + "\\cachelen.bin", FileMode.Open))
                {
                    BinaryFormatter form = new BinaryFormatter();
                    m_lengthDocument = (Dictionary<int, int>)form.Deserialize(reader);
                }
            }
            if (File.Exists(m_debugDir + m_engineDir + "\\cacheidf.bin"))  
            {
                using (Stream reader = File.Open(m_debugDir + m_engineDir + "\\cacheidf.bin", FileMode.Open))
                {
                    BinaryFormatter form = new BinaryFormatter();
                    m_cacheIDF = (Dictionary<int, float>)form.Deserialize(reader);
                }
            }
            Calcavgdl();
            ComputeLenDocs();
        }
    }
}
