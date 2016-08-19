using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 
using System.Diagnostics;
namespace CursIndexer
{
    class Program
    {
        static void Main(string[] args)
        {
            Indexer index = new Indexer();
            Indexing(index); 
        }

        private static void Indexing(Indexer index)
        {
            string[] files = Directory.GetFiles(index.GetWorkDir());
            Console.WriteLine(files.Length);
            Stopwatch timer = new Stopwatch();
            timer.Start();

            int j = 0;
            foreach (var file in files)
            {
                if (j % 50 == 0) Console.WriteLine(timer.ElapsedMilliseconds);
                index.AddDocument(file);
                j++;
            }
            timer.Stop();
            index.ComputeTF();
            index.ComputeIDF();
            index.Serialization();

            

        }
    }
}
