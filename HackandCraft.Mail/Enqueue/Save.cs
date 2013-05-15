using mandrill.net.Enqueue;
using mandrill.net.Fetching;
using mandrill.net.Model;

namespace mandrill.net
{
    public static class Mail
    {
        public static void enqueue(IMessage message)
        {
            var dbMessage = MessageBuilder.build(message);
                DbMail.save(dbMessage);

        }


    }
}
