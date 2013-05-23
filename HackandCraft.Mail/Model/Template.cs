using System.Collections.Generic;
using System.Runtime.Serialization;

namespace mandrill.net
{
    [DataContract]
    public class Template
    {
        [DataMember] 
        public string key =  Config.Instance.apiKey;
        [DataMember(Name = "template_name")]
        public string name { get; set; }

        [DataMember(Name = "template_content")]
        public List<TemplateContent> TemplateContents { get; set; }

        [DataMember(Name = "message")]
        public MandrillMessage Message { get; set; }


        public Template(string _name, string email, List<Var> vars)
        {

            name = _name;
            TemplateContents = new List<TemplateContent>
                {
                    new TemplateContent() {name = "field", content = "example content"}
                };
            Message = new MandrillMessage()
                {
                    fromEmail = Config.Instance.fromEmail,
                    fromName = Config.Instance.fromName,
                    headers = new Headers(),
                    globalMergeVars = new List<Var>
                        {
                            new Var() {name = "field", content = "merge1 content"}
                        },
                    mergeVars = new List<MergeVars>
                        {
                            new MergeVars()
                                {
                                    rcpt = email,
                                    Vars = vars

                                }

                        },
                    recipients = new List<Recipient>
                        {
                            new Recipient() {name = "harry", email = email}
                        }
                };

        }

    }
}