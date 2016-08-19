using System;
using CursIndexer;
namespace CursSearchEngine
{
    class Query
    {
        public static Tuple<string, string>[] Build(string query)
        { 
            string[] tokens = ParserUtils.Tokenization(query.ToLower());
            return SpellChecker(tokens);
        }
        private static Tuple<string, string>[] SpellChecker(string[] query)
        {
            Tuple<string, string>[] res = new Tuple<string, string>[query.Length];
            for (int i = 0; i < query.Length; i++)
                res[i] = new Tuple<string, string>(query[i], query[i]);
            return res;
        }
    }
}
