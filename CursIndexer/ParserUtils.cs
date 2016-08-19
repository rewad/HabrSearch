using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursIndexer
{
    public class ParserUtils
    {
        public static string[] Tokenization(string text)
        {
            char[] separator = new char[] { '.', ',','&', '|', '(', ')', '\n', '\t', '\r', ' ', '[', ']', '{', '}', ';', ':',  '_', '!', '?',  '"', '\'', '\\', '/' };
            return Tokenization(text, separator);
        }
        public static string[] Tokenization(string text, char[] separator)
        {
            string[] tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }
    }
}
