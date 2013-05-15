using System.Collections.Generic;
using System.Runtime.Serialization;

namespace mandrill.net
{
    [DataContract]
    public class MandrillMessage
    {

        [DataMember(Name = "from_email")]
        public string fromEmail = Config.Instance.fromEmail;

        [DataMember(Name = "from_name")] 
        public string fromName = Config.Instance.fromName;

        [DataMember(Name = "to")]
        public List<Recipient> recipients { get; set; }

        [DataMember(Name = "headers")]
        public Headers headers { get; set; }

        [DataMember(Name = "global_merge_vars")]
        public List<Var> globalMergeVars { get; set; }

        [DataMember(Name = "merge_vars")]
        public List<MergeVars> mergeVars { get; set; }

        
    }
}