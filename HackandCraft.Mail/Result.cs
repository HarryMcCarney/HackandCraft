using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DBVC;
using Message = mandrill.net.Fetching.Message;

namespace mandrill.net
{
    [Serializable]
    public class Result : IResult
    {
        [XmlAttribute]
        public string errorMessage { get; set; }
        [XmlAttribute]
        public int status { get; set; }
        [XmlAttribute]
        public string dbMessage { get; set; }
        [XmlAttribute]
        public string procName { get; set; }
        [XmlElement]
        public List<Message> Message { get; set; }
    }
}
