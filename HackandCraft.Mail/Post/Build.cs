using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mandrill.net.Fetching;

namespace mandrill.net.Post
{
    internal static class Build
    {
        internal static Template buildMandrillMessage(Message mail)
        {
            var vars = buildVars(mail);

            var temaplate = new Template(mail.template, mail.recipient, vars);

            return temaplate;

        }

        private static List<Var> buildVars(Message mail)
        {
            var vars = mail.Field.Select(field => new Var() { name = field.key, content = field.value }).ToList();

            return vars;
        }
    }
}
