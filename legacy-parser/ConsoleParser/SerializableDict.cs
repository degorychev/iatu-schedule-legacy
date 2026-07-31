using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.Net;
using System.IO;

namespace ConsoleParser
{
    /// <summary>
    /// Когда то словарь десериализовывался из xml файла, в последних версиях это не так! актуально смотреть файл: Program.cs Функция:  Create_Dictionary()
    /// Есть вероятность, что в текущем состоянии словарь создан из этого унаследованного класса, по этому удалить этот файл так просто не выйдет, хотя не пробовал.
    /// </summary>
    [XmlRoot("dictionary")]
    public class  SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        static public Dictionary<string, string> deserializoving()
        {
            Dictionary<string, string> dict = new SerializableDictionary<string, string>();
            Console.WriteLine("Начата десериализация");
            if (File.Exists("slovarSopost.xml"))
            {
                Console.WriteLine("Найден файл");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                using (FileStream fs = new FileStream("slovarSopost.xml", FileMode.Open))
                {
                    dict = (SerializableDictionary<string, string>)xmlSerializer.Deserialize(fs);
                }
            }
            else
            {
                Console.WriteLine("Файл не найден, скачивание...");
                WebClient webClient = new WebClient();
                webClient.DownloadFile("http://example.local/files/Parser/Data/slovarSopost.xml", "slovarSopost.xml");
                deserializoving();
            }
            return dict;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty) return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                var key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                var value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}

