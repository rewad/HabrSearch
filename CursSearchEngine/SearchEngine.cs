using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CursIndexer;
using Iveonik.Stemmers;
using System.Diagnostics;
namespace CursSearchEngine
{
    public class ResultSearch
    {
        public string id;
        public string snippet;
        public string title;
    }

    public class Stats
    {
        public float time;
        public int num;
        public int page;
        public List<int> pages;
        public List<ResultSearch> results;
        public string query;

    }

    public class SearchEngine
    {
        private static SearchEngine m_instance;
        private Indexer m_index;
        private Ranking m_ranking;
        private NGramm m_bigramms;
        private IStemmer m_stemmer;
        private FileSystem m_pageFileSystem;
        private FileSystem m_titleFileSystem;
        private LRUCache<string, Stats> m_cache;
        public static SearchEngine Engine
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new SearchEngine();
                    m_instance.InitEngine();
                }
                return m_instance;
            }
        }
        public void Start()
        {
            InitEngine();
        }
        private void InitEngine()
        {
            m_index = new Indexer();
            m_index.Deserialization();
            m_ranking = new Ranking();
            m_pageFileSystem = new FileSystem("pages");
            m_titleFileSystem = new FileSystem("title");
            m_pageFileSystem.ReadDict();
            m_titleFileSystem.ReadDict();
            m_bigramms = new NGramm();
            m_bigramms.Desialization();
            m_cache = new LRUCache<string, Stats>(5);
            m_stemmer = new RussianStemmer();

        }


        public Stats Search(string query, int page)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            Tuple<string, string>[] tokens = Query.Build(query);

            string[] words = new string[tokens.Length];
            IStemmer stemmer = new RussianStemmer();
            for (int i = 0; i < tokens.Length; i++)
                words[i] = stemmer.Stem(tokens[i].Item1);

            string find_query = "";

            foreach (var a in words)
            {
                find_query += a + " ";
            }
            find_query += page.ToString();

            var cache_data = m_cache.GetItem(find_query);
            if (cache_data != null)
            {
                cache_data.Data.time = 0;
                return cache_data.Data;
            }


            Ranking ranking = new Ranking();
            int num; 
            List<int> pages;
            var answer = ranking.RankingDocuments(Engine.m_index, words, page, out num, out pages);

            Stats final_result = new Stats();
            final_result.num = num;
            final_result.page = page;
            final_result.pages = pages;
            final_result.query = query;
            final_result.results = new List<ResultSearch>();
            int j = 0;
            foreach (var doc in answer)
            {
                string name = Engine.m_index.GetDoc(doc.Key);
                final_result.results.Add(new ResultSearch());
                final_result.results[j].id = name.Split('.')[0]; 
                final_result.results[j].title = CreateTitle(Encoding.Default.GetString(m_titleFileSystem.ReadFile(name)));
                j++;
            }

            timer.Stop();
            final_result.time = timer.ElapsedMilliseconds;
            m_cache.Insert(find_query, final_result);
            return final_result;

        }
        private string CreateTitle(string text)
        {
            int len_title = text.Length;
            if (len_title >= 50)
            {
                return text.Substring(0, 47) + "...";
            }
            return text;

        }
        public List<string> AutoComplete(string query)
        {
            string[] tokens = ParserUtils.Tokenization(query.ToLower());

            if (tokens.Length == 0)
                return new List<string>();

            string token = tokens.Last();
            string new_query = "";
            var candidate_words = m_bigramms.GetTake10(token);
            foreach (var t in tokens) new_query += t + " ";

            string[] result = new string[candidate_words.Count];

            for (int i = 0; i < candidate_words.Count; i++)
                result[i] = new_query + candidate_words[i];

            return result.ToList();
        }

    }
}
