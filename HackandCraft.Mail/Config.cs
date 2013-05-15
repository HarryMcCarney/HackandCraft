namespace mandrill.net
{
    public class Config
    {
        private static Config instance = new Config();
        public string dbConn { get; set; }
        public string apiKey { get; set; }
        public string apiUrl { get; set; }
        public string fromEmail { get; set; }
        public string fromName { get; set; }
        public string replyTo { get; set; }


        public static Config Instance
        {
            get { return instance; }
        }

        private Config()
        {
          
        }


    }
}
