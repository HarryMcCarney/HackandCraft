using System.Collections.Generic;
using System.Threading.Tasks;
using mandrill.net.Fetching;
using mandrill.net.Post;

namespace mandrill.net
{
    public static class Worker
    {


        private static IEnumerable<Message> dbMails()
        {

            var mails =  DbMail.fetch();
            return mails;

        }

        public static void run()
        {
            checkConfig();
            var mails = dbMails();
            if (mails != null)
            {
                foreach (var mail in mails)
                {
                    var closedMail = mail;
                    Task.Factory.StartNew(() => send(closedMail));
                   // send(closedMail);
                }
            }

        }

        private static void send(Message mail)
        {
            var template = Build.buildMandrillMessage(mail);
            var response = Send.send(template.serialise());
            DbMail.setMessageStatus(mail, response[0]);

        }

        private static void checkConfig()
        {
            if (Config.Instance.apiUrl == null)
                throw new System.InvalidOperationException("apiUrl not supplied.");
            if (Config.Instance.apiKey == null)
                throw new System.InvalidOperationException("apiKey not supplied.");
            if (Config.Instance.dbConn == null)
                throw new System.InvalidOperationException("dbConn not supplied.");
            if (Config.Instance.replyTo == null)
                throw new System.InvalidOperationException("replyTo not supplied.");
            if (Config.Instance.fromEmail == null)
                throw new System.InvalidOperationException("fromEmail not supplied.");
            if (Config.Instance.fromName == null)
                throw new System.InvalidOperationException("fromName not supplied.");

         
        }





    }
}
