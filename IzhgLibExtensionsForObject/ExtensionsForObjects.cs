using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProxyLibs.Extensions
{

    public static class ExtensionsForDateTime
    {
        public static string ToStringAsFilename(in this DateTime dateTime)
        {
            return dateTime.ToString("yyyy MM dd HH mm ss fffffff");
        }
    }

    public static class ExtensionsForObjects
    {
        public static T DeepClone<T>(this T s) where T : ICloneable
        {
            return (T)s.Clone();
        }
        //public static T DeepCopy<T>(this T s) where T : class
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        var formatter = new BinaryFormatter();
        //        formatter.Serialize(ms, s);
        //        ms.Position = 0;
        //        return (T)formatter.Deserialize(ms);
        //    }
        //}

        public static T DeepCopyReflection<T>(this T input) where T : class
        {
            var type = input.GetType();
            var properties = type.GetProperties();

            T clonedObj = (T)Activator.CreateInstance(type);

            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    object value = property.GetValue(input);
                    if (value != null && value.GetType().IsClass && !value.GetType().FullName.StartsWith("System."))
                    {
                        property.SetValue(clonedObj, DeepCopyReflection(value));
                    }
                    else
                    {
                        property.SetValue(clonedObj, value);
                    }
                }
            }

            return clonedObj;
        }
    }
}