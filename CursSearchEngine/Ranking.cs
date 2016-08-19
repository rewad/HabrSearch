using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using CursIndexer;
using System.Diagnostics;

namespace CursSearchEngine
{
    public class Ranking
    {
        private int MaxCandidate { get { return 300; } }
        private float K1 { get { return 2.0f; } }
        private float B { get { return 0.75f; } }
        private int MaxInPage { get { return 10; } }
        private SortedSet<int> UnionDocuments(TIndex index, int[] query)
        {
            SortedSet<int> res = GetDockToTerm(index, query[0]);
            for (int i = 1; i < query.Length; i++)
                res = new SortedSet<int>(GetDockToTerm(index, query[i]).Union(res));
            return res;
        }

        private Dictionary<int, float> BM25(Indexer index, int[] id_terms, int page)
        {
            var documents = UnionDocuments(index.GetIndex(), id_terms);
            var result = new Dictionary<int, float>();

            foreach (var doc in documents)
            {
                float score = 0.0f;
                foreach (var q in id_terms)
                {
                    float tf = index.GetTF(q, doc);
                    float idf = index.GetIDF(q);
                    float doc_stat = index.GetNumDocuments() / index.GetAVGDL();
                    score += idf * ((tf * (K1 + 1.0f)) / (tf + K1 * (1 - B + B * doc_stat)));
                }
                result.Add(doc, score);
            }
            return result;
        }

        public List<KeyValuePair<int, float>> RankingDocuments(Indexer index, string[] query, int page, out int num, out List<int> pages)
        {
            int[] id_q = new int[query.Length];
            for (int i = 0; i < query.Length; i++)
                id_q[i] = index.GetIdDictTerm(query[i]);

            var result_bad = BM25(index, id_q, page);
            num = result_bad.Count;
            var result_all = CosineScore(index, result_bad, id_q, page);

            float all_res = result_all.Count / (float)MaxInPage;
            int max_page = (int)Math.Ceiling(all_res);
            int current_page = Math.Min(max_page, page);
            int cmax_page = current_page * MaxInPage;
            int cmin_page = cmax_page - 10;
            cmax_page = Math.Min(result_all.Count, cmax_page) - 1;


            pages = new List<int>();
            for (int i = page; i < page + 5 && i < max_page; i++)
                pages.Add(i);

            int count = result_all.Count - cmin_page;
            count = count > 10 ? 10 : result_all.Count - cmin_page;

            if (result_all.Count == 0)
                return new List<KeyValuePair<int, float>>();
            return new List<KeyValuePair<int, float>>(result_all.GetRange(cmin_page, count));

        }



        private SortedSet<int> GetDockToTerm(TIndex documents, int q)
        {
            SortedSet<int> res = new SortedSet<int>();
            TPostings doc;
            if (documents.TryGetValue(q, out doc))
            {
                foreach (var id in doc) res.Add(id.Key);
            }
            return res;
        }
        private List<KeyValuePair<int, float>> CosineScore(Indexer index, Dictionary<int, float> res_bad, int[] query, int page)
        {
            var result = new Dictionary<int, float>();
            var tf_query = new Dictionary<int, float>();

            // TF запроса
            foreach (var i in query)
            {
                if (tf_query.ContainsKey(i))
                    tf_query[i] += 1.0f;
                else
                    tf_query.Add(i, 1.0f);

            }

            //tf-idf запроса
            var query_tfidf = new Dictionary<int, float>();
            foreach (var i in tf_query)
                query_tfidf.Add(i.Key, tf_query[i.Key] / query.Length * index.GetIDF(i.Key));
            tf_query = query_tfidf;

            // Длина запроса 
            float len_query = (float)Math.Sqrt(tf_query.Sum(x => x.Value * x.Value));

            
            //Кандидаты на отбор
            int num_candidate = Math.Min(res_bad.Count, MaxCandidate);
            List<KeyValuePair<int, float>> iddoc_rank;
            if (num_candidate < MaxCandidate)
                iddoc_rank = res_bad.ToList();
            else
                iddoc_rank = new List<KeyValuePair<int, float>>(res_bad.OrderBy(x => x.Value).Take(MaxCandidate));

            foreach (var doc in iddoc_rank)
            {
                result.Add(doc.Key, 0.0f);
                float score = 0.0f;
                foreach (var q in tf_query)
                {
                    score += (index.GetTF(q.Key, doc.Key) * index.GetIDF(q.Key) * q.Value);
                }
                score /= len_query * index.GetLenDoc(doc.Key);
                result[doc.Key] = score;
            }
            var array = result.ToList();
            return new List<KeyValuePair<int, float>>(array.OrderBy(x => x.Value).Reverse());
        }

    }
}
