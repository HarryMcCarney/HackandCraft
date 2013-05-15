using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using HackandCraft.Config;
using NLog;

namespace HackandCraft.Auth
{
    public class BasicAuthAttribute : ActionFilterAttribute
    {
        private static readonly string AuthorizationHeader = "Authorization";
        private static readonly string BasicHeader = "Basic ";
        private static readonly string Username = Globals.Instance.settings["BasicAuthUserName"];
        private static readonly string Password = Globals.Instance.settings["BasicAuthPWD"];
        private static readonly char[] Separator = ":".ToCharArray();
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                logHeaders(filterContext.HttpContext.Request);
                if (!Authenticated(filterContext.HttpContext.Request))
                    filterContext.Result = new HttpUnauthorizedResult();

                base.OnActionExecuting(filterContext);
            }
            catch
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        private bool Authenticated(HttpRequestBase httpRequestBase)
        {
            bool authenticated = false;

            if (String.IsNullOrEmpty(httpRequestBase.Headers[AuthorizationHeader]) == false &&
                httpRequestBase.Headers[AuthorizationHeader].StartsWith(BasicHeader,
                                                                        StringComparison.InvariantCultureIgnoreCase))
            {
                string[] credentials = Encoding.ASCII.GetString(Convert.FromBase64String(
                    httpRequestBase.Headers[AuthorizationHeader].Substring(BasicHeader.Length))).Split(Separator);
                log.Info(credentials[0].ToString());
                log.Info(credentials[1].ToString());
                if (credentials.Length == 2 && credentials[0] == Username && credentials[1] == Password)
                {
                    authenticated = true;
                }
            }

            return authenticated;
        }

        private void logHeaders(HttpRequestBase request)
        {
            var headers = request.Headers.ToString();

            foreach (var key in request.Headers.AllKeys)
                log.Info(string.Format("{0}:{1}", key, request.Headers[key]));




        }
    }
}
