using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace CursIndexer
{
   public class SerializationIndex
    {
        public static void Serialization(Dictionary<int, string> resource, string file_name)
        {
            Console.WriteLine("Start save: " + file_name);
            using (BinaryWriter writer = new BinaryWriter(File.Open(file_name, FileMode.Create)))
            {
                SerializationINT(resource.Count, writer);
                foreach (var term in resource)
                {
                    SerializationINT(term.Key, writer);
                    writer.Write(term.Value);
                }
            }
            Console.WriteLine("End save");

        }

        public static Dictionary<int, string> DeserializationDictNS(string file_name)
        {
            Console.WriteLine("Start load: " + file_name);
            Dictionary<int, string> res = new Dictionary<int, string>();
            using (BinaryReader reader = new BinaryReader(File.Open(file_name, FileMode.Open)))
            {
                int num = reader.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    res.Add(reader.ReadInt32(), reader.ReadString());
                }
            }

            Console.WriteLine("End load: ");
            return res;
        }

        public static void Serialization(Dictionary<int, DateTime> resource, string file_name)
        {
            Console.WriteLine("Start save: " + file_name);
            using (BinaryWriter writer = new BinaryWriter(File.Open(file_name, FileMode.Create)))
            {
                SerializationINT(resource.Count, writer);
                foreach (var term in resource)
                {
                    SerializationINT(term.Key, writer);
                    writer.Write(term.Value.ToBinary());
                }
            }
            Console.WriteLine("End save");

        }

        public static Dictionary<int, DateTime> DeserializationDate(string file_name)
        {
            Console.WriteLine("Start load: " + file_name);
            Dictionary<int, DateTime> res = new Dictionary<int, DateTime>();
            using (BinaryReader reader = new BinaryReader(File.Open(file_name, FileMode.Open)))
            {
                int num = reader.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    res.Add(reader.ReadInt32(), DateTime.FromBinary( reader.ReadInt64()));
                }
            }

            Console.WriteLine("End load: ");
            return res;
        }

        public static Dictionary<string, int> DeserializationDictSN(string file_name)
        {

            Console.WriteLine("Start load: " + file_name);
            Dictionary<string, int> res = new Dictionary<string, int>();
            using (BinaryReader reader = new BinaryReader(File.Open(file_name, FileMode.Open)))
            {
                int num = reader.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    res.Add(reader.ReadString(), reader.ReadInt32());
                }
            }

            Console.WriteLine("End load: ");
            return res;
        }

        public static void Serialization(Dictionary<string, int> resource, string file_name)
        {

            Console.WriteLine("Start save: " + file_name);
            using (BinaryWriter writer = new BinaryWriter(File.Open(file_name, FileMode.Create)))
            {
                SerializationINT(resource.Count, writer);
                foreach (var term in resource)
                {

                    writer.Write(term.Key);
                    SerializationINT(term.Value, writer);
                }
            }

            Console.WriteLine("End save");
        }
        public static void SerializeIndex(TIndex index, string file_name)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(file_name, FileMode.Create)))
            {
                SerializationINT(index.Count, writer);
                foreach (var term in index)
                {
                    SerializationINT(term.Key, writer);
                    SerializationPostings(term.Value, writer);
                }
            }
        }
        public static void SerializationPostings(TPostings postings, BinaryWriter writer)
        {
            SerializationINT(postings.Count, writer);
            foreach (var posting in postings)
            {
                SerializationINT(posting.Key, writer);
                SerializationINT(posting.Value, writer);
            }
        }
        public static void SerializationPositions(TPositions positions, BinaryWriter writer)
        {
            SerializationINT(positions.Count, writer);
            foreach (var pos in positions)
            {
                SerializationUSHORT(pos, writer);
            }
        }
        public static void SerializationINT(int val, BinaryWriter writer)
        {
            writer.Write(val);
        }
        public static void SerializationUSHORT(ushort val, BinaryWriter writer)
        {
            writer.Write(val);
        }
        public static void SerializationFLOAT(float val, BinaryWriter writer)
        {
            writer.Write(val);
        }
        public static TIndex DeserializeIndex(string file_name)
        {
            TIndex index = new TIndex();

            using (BinaryReader reader = new BinaryReader(File.Open(file_name, FileMode.Open)))
            {
                int size = DeserializeINT(reader);
                for (int i = 0; i < size; i++)
                    index.Add(DeserializeINT(reader), DeserializePostings(reader));
            }
            return index;
        }
        public static TPostings DeserializePostings(BinaryReader reader)
        {
            TPostings postings = new TPostings();
            int size = DeserializeINT(reader);
            for (int i = 0; i < size; i++)
                postings.Add(DeserializeINT(reader), DeserializeINT(reader));
            return postings;
        }

        public static TPositions DeserializePositions(BinaryReader reader)
        {
            TPositions positions = new TPositions();
            int size = DeserializeINT(reader);
            for (int i = 0; i < size; i++)
                positions.Add(DeserializeUSHORT(reader));
            return positions;
        }

        public static int DeserializeINT(BinaryReader reader)
        {
            return reader.ReadInt32();
        }
        public static ushort DeserializeUSHORT(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }
        public static float DeserializeFLOAT(BinaryReader reader)
        {
            return reader.ReadSingle();
        }
        public static void SerializationCacheTF(TCacheTF cache, string file_name)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(file_name, FileMode.Create)))
            {
                SerializationINT(cache.Count, writer);
                foreach (var term in cache)
                {
                    SerializationINT(term.Key, writer);
                    SerializationDocumentTF(term.Value, writer);
                }
            }
        }
        public static void SerializationDocumentTF(TDocumentTF doc, BinaryWriter writer)
        {
            SerializationINT(doc.Count, writer);
            foreach (var tf in doc)
            {
                SerializationINT(tf.Key, writer);
                SerializationFLOAT(tf.Value, writer);
            }
        }
        public static TCacheTF DeserializationTCacheTF(string file_name)
        {
            TCacheTF cache = new TCacheTF();
            using (BinaryReader reader = new BinaryReader(File.Open(file_name, FileMode.Open)))
            {
                int size = DeserializeINT(reader);
                for (int i = 0; i < size; i++)
                    cache.Add(DeserializeINT(reader), DeserializarionTDocumentTF(reader));
            }
            return cache;
        }
        public static TDocumentTF DeserializarionTDocumentTF(BinaryReader reader)
        {
            TDocumentTF doc = new TDocumentTF();
            int size = DeserializeINT(reader);
            for (int i = 0; i < size; i++)
                doc.Add(DeserializeINT(reader), DeserializeFLOAT(reader));
            return doc;
        }
    }
}
