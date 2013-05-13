using System;
using System.Xml.Serialization;
using DBVC;

namespace HackandCraft.Payment
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
        public Payment Payment { get; set; }
    }
}
