using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;

namespace AirWebService
{
    public class XmlHelper
    {
        /// <summary>
        /// Xml데이타를 모델로 변경
        /// </summary>
        /// <param name="XmlElem"></param>
        /// <param name="type">모델 타입</param>
        /// <returns></returns>
        public static Object Xml_ModelSerializer(string XmlElem, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(XmlElem);

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray))
            {
                Object response = serializer.Deserialize(stream);
                return response;
            }
        }

        /// <summary>
        /// 모델을 XML 형태로 변환
        /// </summary>
        /// <param name="models"></param>
        /// <param name="type">models type</param>
        /// <returns></returns>
        public static XmlElement Model_XmlSerializer(Object models, Type type)
        {
            XmlDocument XmlDoc = new XmlDocument();

            XmlSerializer xmlSerializer = new XmlSerializer(type);
            using (System.IO.StringWriter stringWriter = new System.IO.StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlSerializer.Serialize(stringWriter, models);
                    XmlDoc.LoadXml(stringWriter.ToString());
                }
            }

            return XmlDoc.DocumentElement;
        }

        /// <summary>
        /// object 형식을 Xml문자열로 변환
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertString(object obj)
        {
            string result = string.Empty;

            if (obj.GetType() == typeof(string))
            {
                result = obj.ToString();
            }
            else
            {
                var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                var serializer = new XmlSerializer(obj.GetType());
                var setting = new XmlWriterSettings();
                setting.Indent = true;
                setting.OmitXmlDeclaration = true;
                using (StringWriter stream = new StringWriter())
                using (var writer = XmlWriter.Create(stream, setting))
                {
                    serializer.Serialize(writer, obj, ns);
                    result = stream.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// Xml문자열을 해당 Type의 object로 변환
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ConvertObject(string xml, Type type)
        {
            object result = new object();

            try
            {
                var serializer = new XmlSerializer(type);
                using (TextReader reader = new StringReader(xml))
                {
                    result = serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }



    }
}