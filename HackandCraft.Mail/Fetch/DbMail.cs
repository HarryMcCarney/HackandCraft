using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBVC;
using mandrill.net.Model;

namespace mandrill.net.Fetching
{
    internal static class DbMail
    {
        internal static List<Message> fetch()
        {
            var connstr = Config.Instance.dbConn;
            var orm = new Orm(new MSSQLData(connstr));
            var dbMails = orm.execObject<Result>(null, "mess.dequeue_message").Message;

            return dbMails;
        }

        internal static void setMessageStatus(Message message, MandrillResponse response)
        {
            var messageStatus = new MessageStatus() { Message = message };

            switch (response.status)
            {
                case "sent":
                    messageStatus.status = "SENT";
                    break;
                case "queued":
                    messageStatus.status = "QUEUED";
                    break;
                default:
                    messageStatus.status = "FAILED";
                    break;
            }
 
            var connstr = Config.Instance.dbConn; 
            var orm = new Orm(new MSSQLData(connstr));
            orm.execObject<Result>(messageStatus, "mess.set_message_status");
        }

        internal static void save(Message message)
        {
            
            var orm = new Orm();
            orm.execObject<Result>(message, "mess.enqueue_message");
         }


    }
}
