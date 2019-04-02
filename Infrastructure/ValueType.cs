using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ddd.Taxi.Domain;

namespace Ddd.Infrastructure
{
    /// <summary>
    /// Базовый класс для всех Value типов.
    /// </summary>
    public class ValueType<T>
    {
        bool flag = true;

        public bool Equals(PersonName  name)
        {
            if (GetType() != typeof(PersonName)) return Equals((object)name);
            else
            return name.FirstName.Equals((this as PersonName).FirstName) &&
                name.LastName.Equals((this as PersonName).LastName);
        }

        public override bool Equals(object type)
        {
            if (type == null) flag = false;
            else if (GetType() != type.GetType()) flag = false;
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
                    //else if ((a[i].GetValue(type).GetType()).IsSubclassOf(typeof(ValueType<T>))) a[i].Equals(b[i]);
                    else if (a[i].GetValue(type).GetType().Name == "PersonName")
                    {
                        var g = a[i].GetValue(type);
                        var name = a[i].GetValue(type).GetType().GetProperties();
                        var l = b[i].GetValue(this);
                        var currentName = b[i].GetValue(this).GetType().GetProperties();
                        for (int j = 0; j < name.Length; j++)
                        {
                            if ((name[j].GetValue(g) == null) && (currentName[j].GetValue(l) == null)) continue;
                            else if ((name[j].GetValue(g) == null) && (currentName[j].GetValue(l) != null) ||
                                (name[j].GetValue(g) != null) && (currentName[j].GetValue(l) == null)) flag = false;
                            else if (!(name[j].GetValue(g).Equals(currentName[j].GetValue(l)))) flag = false;
                        }
                    }
                    else if (!(a[i].GetValue(type).Equals(b[i].GetValue(this)))) flag = false;
                }
            }

            return flag;
        }
        public override int GetHashCode()
        {
            var t = GetType().GetProperties();
            int hashCode;
            unchecked { hashCode = (int)1192343178864; }
            for (int i = 1; i < t.Length - 1; i++)
            {
                unchecked
                {
                    var w = 0;
                    if (t[i].GetValue(this) == null)
                        w = t[i].GetHashCode();
                    else w = t[i].GetValue(this).GetHashCode();
                    hashCode += (hashCode * 564366547) ^ (int)Math.Sqrt(w);
                }
            }
            return hashCode;
        }

        public override string ToString()
        {
            StringBuilder value = new StringBuilder();
            value.Append(GetType().Name + "(");
            var properties = GetType().GetProperties();
            if (GetType().Name == "Address")
                for (int i = properties.Length - 1; i >= 0; i--)
                    value.Append(properties[i].Name + ": " + properties[i].GetValue(this) + "; ");
            else
                for (int i = 0; i < properties.Length; i++)
                    value.Append(properties[i].Name + ": " + properties[i].GetValue(this) + "; ");
            value.Remove(value.Length - 2, 2);
            value.Append(")");
            return value.ToString();
        }
    }
}