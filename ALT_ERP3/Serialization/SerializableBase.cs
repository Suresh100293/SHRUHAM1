using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Web;

namespace RDL
{
    [Serializable]
    public class SerializableBase 
    {
        #region Serialization
        //turns this object into a string [xml]
        public String Serialize()//Type type)
        {
            String strOut = "";
            //the serializer will convert an object into XML
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(this.GetType());

            //we need a string writer to catch the outgoing stream
            using (System.IO.StringWriter sw = new System.IO.StringWriter())
            {
                //now we turn the object into a chunk of XML (that gets 
                //stuffed into a stream writer)
                ser.Serialize(sw, this);

                //get the text from the StreamWriter
                strOut = sw.ToString();
                sw.Close();
            }

            return strOut;
        }

        //Factory method that turns XML into an object
        public static SerializableBase Deserialize(String xml, Type type)
        {
            SerializableBase obj = new SerializableBase();//default

            //strip any of the namespaces, because they fubar the deserialization
            xml = xml.Replace(" xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2008/01/reportdefinition\"", "")
                .Replace("xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition\"", "")
                .Replace("xmlns=\"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition\"","")
                .Replace("xmlns:cl=\"http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition\"","")
                .Replace("xmlns:df=\"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition/defaultfontfamily\"","")
                .Replace("MustUnderstand=\"df\"","")
                .Replace("<cl:", "").Replace("</cl:", "")
                .Replace(" xmlns:rd=\"http://schemas.microsoft.com/SQLServer/reporting/reportdesigner\"", "")
                .Replace("<rd:", "").Replace("</rd:", "");

            //------------------
            //now parse the xml
            //------------------

            //the serializer will convert the XML into a populated instance of an object
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(type);

            //the serializer needs to read the string from a stream
            //so I create a string reader to convert the string into a stream
            using (System.IO.StringReader sr = new System.IO.StringReader(xml))
            {
                //this actually changes the xml into the object
                obj = (SerializableBase)ser.Deserialize(sr);
                sr.Close();
            }

            return obj;
        }
        #endregion
    }
}