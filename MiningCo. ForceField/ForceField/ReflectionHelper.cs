using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ForceField
{
    internal class ReflectionHelper
    {
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo field = type.GetField(fieldName, bindingAttr);
            return field.GetValue(instance);
        }
    }
}
