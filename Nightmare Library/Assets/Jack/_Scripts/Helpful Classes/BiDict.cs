using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiDict<T1, T2>
{
    private List<BiDictItem<T1, T2>> dict = new List<BiDictItem<T1, T2>>();

    public T2 this[T1 key]
    {
        get => GetT2Item(key);
    }
    public T1 this[T2 key]
    {
        get => GetT1Item(key);
    }

    public List<T1> Keys1
    {
        get => GetT1List();
    }
    public List<T2> Keys2
    {
        get => GetT2List();
    }

    private List<T1> GetT1List()
    {
        List<T1> list = new List<T1>();
        foreach(BiDictItem<T1, T2> item in dict)
        {
            list.Add(item.key1);
        }
        return list;
    }
    private List<T2> GetT2List()
    {
        List<T2> list = new List<T2>();
        foreach (BiDictItem<T1, T2> item in dict)
        {
            list.Add(item.key2);
        }
        return list;
    }

    private T1 GetT1Item(T2 key)
    {
        for (int i = 0; i < dict.Count; i++)
        {
            if (dict[i].key2.GetHashCode() == key.GetHashCode())
                return dict[i].key1;
        }
        return default(T1);
    }
    private T2 GetT2Item(T1 key)
    {
        for (int i = 0; i < dict.Count; i++)
        {
            if (dict[i].key1.GetHashCode() == key.GetHashCode())
                return dict[i].key2;
        }
        return default(T2);
    }

    public void Add(T1 key1, T2 key2)
    {
        BiDictItem<T1, T2> item = new BiDictItem<T1, T2>(key1, key2);
        if (!dict.Contains(item))
            dict.Add(item);
    }
    public void Remove(T1 key1)
    {
        for (int i = 0; i < dict.Count; i++)
        {
            if (dict[i].Equals(new BiDictItem<T1, T2>(key1)))
            {
                dict.RemoveAt(i);
                break;
            }
        }
    }
    public void Remove(T2 key2)
    {
        for (int i = 0; i < dict.Count; i++)
        {
            if (dict[i].Equals(new BiDictItem<T1, T2>(key2)))
            {
                dict.RemoveAt(i);
                break;
            }
        }
    }

    public override string ToString()
    {
        string output = String.Empty;
        foreach(BiDictItem<T1, T2> item in dict)
        {
            output += item.ToString() + "\n";
        }
        return output;
    }

    private class BiDictItem<type1, type2>
    {
        public type1 key1;
        public type2 key2;

        public BiDictItem(type1 key1, type2 key2)
        {
            this.key1 = key1;
            this.key2 = key2;
        }
        public BiDictItem(type1 key1)
        {
            this.key1 = key1;
        }
        public BiDictItem(type2 key2)
        {
            this.key2 = key2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(key1.GetHashCode(), key2.GetHashCode());
        }
        public override bool Equals(object obj)
        {
            BiDictItem<type1, type2> item = (BiDictItem<type1, type2>)obj;
            if (item != null)
                return EqualityComparer<type1>.Default.Equals(item.key1, key1) || EqualityComparer<type2>.Default.Equals(item.key2, key2);
            return false;
        }

        public override string ToString()
        {
            return key1.ToString() + " <=> " + key2.ToString();
        }
    }
}
