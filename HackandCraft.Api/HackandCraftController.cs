using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using DBVC;
using HackandCraft.Config;
using NLog;
using Newtonsoft.Json;

namespace HackandCraft.Api
{
    public class HackandCraftController : Controller
    {
        public string clientId;
        public static readonly Logger log = LogManager.GetCurrentClassLogger();
        public IResult result;
        public Orm orm;
        public string jsonParam;
        public HackandCraftController()
        {
            orm = new Orm();
         }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            isHttps(filterContext);
            checkApiVersion(filterContext);
            setJson(filterContext);
            log.Info(string.Format("{0} Parameter {1}", filterContext.HttpContext.Request.RawUrl, jsonParam));
            Response.ContentType = "application/json";
        }

        protected void errorResult(Exception exp)
        {
            result.status = 1;
            result.errorMessage = exp.ToString();
            log.Error(string.Format("{0}|||{1}", exp, jsonParam));

        }

        public static string formattedResult(Result result)
        {
            return JsonConvert.SerializeObject(result, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }


        public void isHttps(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsSecureConnection)
            {
                throw new HttpException(701, "HTTPS REQUIRED");
            }
        }


        public void checkApiVersion(ActionExecutingContext filterContext)
        {
            var apiVersion = Globals.Instance.settings["CurrentVersion"];
            var urlVersion = filterContext.RouteData.Values["versionNo"].ToString();

            if (apiVersion != urlVersion)
            {
                throw new HttpException(601, "INCORRECT API VERSION");
            }
        }


        public void setJson(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            filterContext.HttpContext.Request.InputStream.Position = 0;
            using (var reader = new StreamReader(filterContext.HttpContext.Request.InputStream))
            {
                jsonParam = reader.ReadToEnd();
            }
        }


    }
}
