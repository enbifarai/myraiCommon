using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.Text;

namespace myRaiHelper
{
    class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    /// <summary>
    /// Classe di utility per la serializzazione di oggetti
    /// </summary>
    public static class SerializerHelper
    {
        /// <summary>
        /// Serializzazione di un oggetto in byte array
        /// </summary>
        /// <param name="obj">
        /// Oggetto da serializzare
        /// </param>
        /// <returns></returns>
        public static byte[] Serialize(object data)
        {
            byte[] b = null;

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);

                stream.Position = 0;

                b = new byte[stream.Length];
                stream.Read(b, 0, b.Length);
            }

            return b;
        }

        /// <summary>
        /// Deserializzazione di un oggetto
        /// </summary>
        /// <param name="data">
        /// Stream dell'oggetto serializzato
        /// </param>
        /// <returns></returns>
        public static object Deserialize(byte[] data)
        {
            object obj = null;

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(data, 0, data.Length);

                stream.Position = 0;

                BinaryFormatter formatter = new BinaryFormatter();
                obj = formatter.Deserialize(stream);
            }

            return obj;
        }

        /// <summary>
        /// Serializzazione in XML di un dato
        /// </summary>
        /// <param name="data">
        /// Dati da serializzare in XML
        /// </param>
        /// <returns></returns>
        public static string SerializeXml(object data)
        {
            string serialized = null;

            XmlSerializer serializer = new XmlSerializer(data.GetType());

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (var stream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, data);
                    serialized = stream.ToString();
                }
            }

            return serialized;
        }

        /// <summary>
        /// Deserializzazione dei dati dall'XML
        /// </summary>
        /// <param name="xml"></param>
        public static Object DeserializeXml(string data, Type type)
        {
            Object instance = null;

            byte[] content = Encoding.UTF8.GetBytes(data);

            using (MemoryStream ms = new MemoryStream(content))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                instance = serializer.Deserialize(ms);
            }

            return instance;
        }

        /// <summary>
        /// Serializzazione in string di un dato generico
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oggetto"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T oggetto, bool omitXmlDeclaration=true) where T : class
        {
            string result;
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(oggetto.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = omitXmlDeclaration;
            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                using (var wri = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(wri, oggetto, emptyNamespaces);
                    result = sw.ToString();
                }
            }

            return result;
        }
    }
}