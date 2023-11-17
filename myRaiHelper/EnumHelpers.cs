using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace myRaiHelper
{
    /// <summary>
    /// Classe helper per gli enum
    /// </summary>
    public static class EnumHelpers
    {
        /// <summary>
        /// Reperimento della descrizione dell'enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// Reperimento dell'ambient value dell'enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetAmbientValue(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            AmbientValueAttribute[] attributes = (AmbientValueAttribute[])fi.GetCustomAttributes(typeof(AmbientValueAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value.ToString();
            else
                return value.ToString();
        }

        /// <summary>
        /// Reperimento Enum dal valore Description
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumValue<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(DescriptionAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((DescriptionAttribute)a.Att)
                                .Description == description).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }

        /// <summary>
        /// Reperimento della stringa che definisce l'icona da utilizzare per quell'elemento
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetIconDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            IconAttribute[] attributes = (IconAttribute[])fi.GetCustomAttributes(typeof(IconAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value.ToString();
            else
                return value.ToString();
        }


        public static string GetVisibilitaValue(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            VisibilitaAttribute[] attributes = (VisibilitaAttribute[])fi.GetCustomAttributes(typeof(VisibilitaAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value.ToString();
            else
                return value.ToString();
        }
    }

    /// <summary>
    /// Classe che estende gli attributi di un enum
    /// </summary>
    public class IconAttribute : Attribute
    {
        private string _value;
        public IconAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }


    public class VisibilitaAttribute : Attribute
    {
        private string _value;
        public VisibilitaAttribute(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }

}