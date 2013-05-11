using System;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text;


namespace DBVC
{
    public class Orm
    {

        private readonly IDataSource dataSource;
   
       public Orm()
        {
           dataSource = new MSSQLData();
        }

        public Orm(IDataSource _dataSource)
       {
           dataSource = _dataSource;
       }

       public T execObject<T>(Object paramObj, string procName) where T : IResult
        {
           try
            {
                var instance = Activator.CreateInstance<T>();
                var xml = (paramObj == null) ? null : SerialiseParam(paramObj, null);
                var result = dataSource.execStoredProc(procName, xml);
                return (T)deserialise(result, instance);
            }
            catch (Exception exp)
            {
                var instance = Activator.CreateInstance<T>();
                instance.errorMessage = exp.Message.ToString();
                instance.status = 1;
                return (T)instance;
            }

        }
       private T deserialise<T>(XmlDocument xml, T myResult)
       {

           var mySerializer = new XmlSerializer(myResult.GetType());
           var myStream = new MemoryStream();
           xml.Save(myStream);
           myStream.Position = 0;
           var r = mySerializer.Deserialize(myStream);
           return (T)r;

       }

        private XmlDocument SerialiseParam(Object o, Type t)
        {
            var mySerializer = t != null ? new XmlSerializer(t, new Type[] { o.GetType() }) : new XmlSerializer(o.GetType());
            var myStream = new MemoryStream();
            var xmlWriter = XmlWriter.Create(myStream, new XmlWriterSettings { OmitXmlDeclaration = true });
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            mySerializer.Serialize(xmlWriter, o, ns);
            var xml = Encoding.UTF8.GetString(myStream.GetBuffer());
            xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
            xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
            var p = new XmlDocument();
            p.LoadXml(xml);
            return p;
        }

     


    }

}