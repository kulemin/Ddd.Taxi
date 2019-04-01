using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ddd.Taxi.Domain;

namespace Ddd.Infrastructure
{
	/// <summary>
	/// Базовый класс для всех Value типов.
	/// </summary>
	public class ValueType<T>
    { 
        bool flag = true;
        public bool Equals(T type)
        {
            if (type == null) flag = false;

            else if (!typeof(T).IsSubclassOf(typeof(ValueType<T>)))
            {
                if (!type.Equals(this)) flag = false;
            }
            else
            {
                var a = type.GetType().GetProperties();
                var b = GetType().GetProperties();

                for (int i = 0; i < a.Length; i++)
                {
                   
                    if ((a[i].GetValue(type) == null) && (b[i].GetValue(this) == null)) continue;
                    else if ((a[i].GetValue(type) == null) && (b[i].GetValue(this) != null) ||
                        (a[i].GetValue(type) != null) && (b[i].GetValue(this) == null)) flag = false;
                    else if ((a[i].GetValue(type).GetType()).IsSubclassOf(typeof(ValueType<T>))) a[i].Equals(b[i]);
                    else if (!(a[i].GetValue(type).Equals(b[i].GetValue(this)))) flag = false;
                }
            }
            return flag;
        }
            public int GetHashCode (T type)
        {
            return type.GetType().GetProperties().ToString().GetHashCode();
        }
        public string ToString(T type)
        {
            string value = null;
            foreach (var e in type.GetType().GetProperties())
            {
                value += e.Name + ".";
            }
            return value;
        }

	}
}