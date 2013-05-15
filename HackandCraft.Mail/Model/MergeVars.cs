using System.Collections.Generic;
using System.Runtime.Serialization;

namespace mandrill.net
{
    [DataContract]
    public class MergeVars
    {
        [DataMember(Name = "vars")]
        public List<Var> Vars { get; set; }
        [DataMember]
        public string rcpt { get; set; }


    }
}