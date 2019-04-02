using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ddd.Taxi.Domain;

namespace Ddd.Infrastructure
{
    public class ValueType<T>
    {
        bool flag = true;

        public bool Equals(PersonName name)
        {
            if (GetType() != typeof(PersonName))
                return Equals((object)name);
            else
                return name.FirstName.Equals((this as PersonName).FirstName) &&
                        name.LastName.Equals((this as PersonName).LastName);
        }

        public override bool Equals(object type)
        {
            if (type == null) flag = false;
            else if (GetType() != type.GetType()) flag = false;
            else if (!typeof(T).IsSubclassOf(typeof(ValueType<T>))) { if (!type.Equals(this)) flag = false; }
            else
            {
                var newProps = type.GetType().GetProperties();
                var curProps = GetType().GetProperties();

                for (int i = 0; i < newProps.Length; i++)
                {
                    if ((newProps[i].GetValue(type) == null) && (curProps[i].GetValue(this) == null)) continue;
                    else if ((newProps[i].GetValue(type) == null) && (curProps[i].GetValue(this) != null)
                        || (newProps[i].GetValue(type) != null) && (curProps[i].GetValue(this) == null)) flag = false;
                    else if (newProps[i].GetValue(type).GetType().Name == "PersonName")
                        flag = newProps[i].GetValue(type).Equals(curProps[i].GetValue(this));
                    else if (!(newProps[i].GetValue(type).Equals(curProps[i].GetValue(this)))) flag = false;
                }
            }
            return flag;
        }

        public override int GetHashCode()
        {
            var properties = this.GetType().GetProperties();
            var hashCode = 974875401;

            for (int i = 0; i < properties.Length; i++)
            {
                unchecked { hashCode = hashCode * -1521134295 + properties[i].GetValue(this).GetHashCode(); }
            }
            return hashCode;
        }

        public override string ToString()
        {
            StringBuilder value = new StringBuilder();
            value.Append(GetType().Name + "(");
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
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