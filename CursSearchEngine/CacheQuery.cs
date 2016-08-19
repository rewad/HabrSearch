using System;
using System.Collections.Generic;
using System.Linq;

namespace CursSearchEngine
{

    public class Node<D, K>
    {
        public D Data { get; private set; }
        public K Key { get; private set; }
        public Node<D, K> Previous { get; set; }
        public Node<D, K> Next { get; set; }

        public Node(D data, K key)
        {
            Data = data;
            Key = key;
        }
    }
    public class LRUCache<K, V>
    {
        private int m_maxCapacity = 0;
        private Dictionary<K, Node<V, K>> m_LRUCache;
        private Node<V, K> m_head = null;
        private Node<V, K> m_tail = null;

        public LRUCache(int argMaxCapacity)
        {
            m_maxCapacity = argMaxCapacity;
            m_LRUCache = new Dictionary<K, Node<V, K>>();
        }

        public void Insert(K key, V value)
        {
            if (m_LRUCache.ContainsKey(key))
            {
                MakeMostRecentlyUsed(m_LRUCache[key]);
            }

            if (m_LRUCache.Count >= m_maxCapacity) RemoveLeastRecentlyUsed();

            Node<V, K> insertedNode = new Node<V, K>(value, key);

            if (m_head == null)
            {
                m_head = insertedNode;
                m_tail = m_head;
            }
            else MakeMostRecentlyUsed(insertedNode);

            if (!m_LRUCache.ContainsKey(key))
                m_LRUCache.Add(key, insertedNode);
        }

        public Node<V, K> GetItem(K key)
        {
            if (!m_LRUCache.ContainsKey(key)) return null;

            MakeMostRecentlyUsed(m_LRUCache[key]);

            return m_LRUCache[key];
        }

        public int Size()
        {
            return m_LRUCache.Count();
        }

        public string CacheFeed()
        {
            var headReference = m_head;

            List<string> items = new List<string>();

            while (headReference != null)
            {
                items.Add(String.Format("[V: {0}]", headReference.Data));
                headReference = headReference.Next;
            }

            return String.Join(",", items);
        }

        private void RemoveLeastRecentlyUsed()
        {
            m_LRUCache.Remove(m_tail.Key);
            m_tail.Previous.Next = null;
            m_tail = m_tail.Previous;
        }

        private void MakeMostRecentlyUsed(Node<V, K> foundItem)
        { 
            if (foundItem.Next == null && foundItem.Previous == null)
            {
                foundItem.Next = m_head;
                m_head.Previous = foundItem;
                if (m_head.Next == null) m_tail = m_head;
                m_head = foundItem;
            } 
            else if (foundItem.Next == null && foundItem.Previous != null)
            {
                foundItem.Previous.Next = null;
                m_tail = foundItem.Previous;
                foundItem.Next = m_head;
                m_head.Previous = foundItem;
                m_head = foundItem;
            } 
            else if (foundItem.Next != null && foundItem.Previous != null)
            {
                foundItem.Previous.Next = foundItem.Next;
                foundItem.Next.Previous = foundItem.Previous;
                foundItem.Next = m_head;
                m_head.Previous = foundItem;
                m_head = foundItem;
            } 
        }
    }
}