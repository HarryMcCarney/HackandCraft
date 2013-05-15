Mandrill.net
============
An implementation of complete mailing system in c#. Includes creating, queuing and sending through the mandrill api.
Requires a mandrill account, api key and template setup.

Depends upon json,net, dbvc and the dbvc db pattern.

The db objects are setup with the sql script BuildDbObjects.sql

First define the config setting
The dbConn will be the location of the message queue where message should be inserted and fetched.
    
            
            Config.Instance.apiUrl = @"https://mandrillapp.com/api/1.0/messages/send-template.json";
            Config.Instance.apiKey = @"[Mandrill api key]";
            Config.Instance.dbConn = @"data source=192.176.0.1\MSSQLinstance;Initial Catalog=[MyDb];User Id=[User];Password=[pwd];";
            Config.Instance.replyTo = @"replyto@yourdomain.com";
            Config.Instance.fromEmail = @"from@yourdomain.com";
            Config.Instance.fromName = @"some mail sender";

The we can run the queue with 

    Worker.run();

You can add mails to the queue by first defining a template class. This assumes there is a template defined at mandrill 'test'


    public class Test : IMessage
    {
        public string id { get; set; }
        public string type { get; set; }
        public string template { get; set; }
        public string recipient { get; set; }
        public string age { get; set; }
        public string city { get; set; }
        public string name { get; set; }

        public Test()
        {
            id = Guid.NewGuid().ToString();
            type = "EMAIL";
            name = "Ludwig";
            age = "34";
            city = "Vienna";
            recipient = "ludwig@domain.com";
            template = "test";//mandrill template name
		}
  	 }

Every template must implement IMessage

You can then add the mail to the queue with

    var test = new Test();
    Mail.enqueue(test);



