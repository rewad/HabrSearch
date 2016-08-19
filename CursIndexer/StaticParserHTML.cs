using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace CursIndexer
{
    public class StaticParserHTML
    {
        public static Tuple<string, string> ParseHabr(string html_code)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html_code);
            string text = "";
            string date="";
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'post_show')]");
            if(node==null)
            {
                node = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'company_post')]");
                if (node != null)
                {
                    text += node.SelectSingleNode("//*[contains(@class, 'content html_format')]")?.InnerText;
                    date = node.SelectSingleNode("//*[contains(@class, 'published')]")?.InnerText;
                }
            }

            if (node != null)
            {
                text = node.SelectSingleNode("//*[contains(@class, 'post_title')]/text()")?.InnerText;
                text += node.SelectSingleNode("//*[contains(@class, 'content html_format')]")?.InnerText;
                date = node.SelectSingleNode("//*[contains(@class, 'published')]")?.InnerText;

            }
            else
            {
                Console.WriteLine("Error");
            }
            return new Tuple<string,string>(text,date);
        }
        public static Tuple<string, string> ParseHabrTitle(string html_code)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html_code);
            string text = "";
            string date = "";
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'post_show')]");
            if (node == null)
            {
                node = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'company_post')]");
                if (node != null)
                { 
                    text += node.SelectSingleNode("//*[contains(@class, 'post__title')]")?.InnerText; 
                }
            }

            if (node != null)
            {
                text = node.SelectSingleNode("//*[contains(@class, 'post_title')]/text()")?.InnerText; 

            }
            else
            {
                Console.WriteLine("Error");
            }
            return new Tuple<string, string>(text, date);
        }
    }
}
