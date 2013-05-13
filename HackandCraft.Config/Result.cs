using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DBVC;

namespace HackandCraft.Config
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
        public Global Global { get; set; }
    }
}
