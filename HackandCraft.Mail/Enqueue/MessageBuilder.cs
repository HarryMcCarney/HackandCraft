using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using mandrill.net.Fetching;
using mandrill.net.Model;

namespace mandrill.net.Enqueue
{
    public static class MessageBuilder
    {
        public static Message build(IMessage message)
        {
            var dbmessage = new Message
                {
                    recipient = message.recipient,
                    template = message.template,
                    type = message.type,
                    id = Guid.NewGuid().ToString(),
                    Field = addFields(message)
                };
            return dbmessage;
        }

        private static List<Field> addFields(IMessage message)
        {
            var messageType = message.GetType();
            PropertyInfo[] props = messageType.GetProperties();
            var fields = props.Select(prop => new Field()
                {
                    key = prop.Name,
                    value = prop.GetValue(message, null) as string
                }).ToList();
            ;
            return fields;
        }


    }




}
