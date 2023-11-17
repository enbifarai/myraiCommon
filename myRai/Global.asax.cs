using myRai.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using myRai.Business;
using myRai.Data;
using System.Diagnostics;
using myRaiData;
using myRai.DataAccess;
using myRaiHelper;

namespace myRai
{
    // Nota: per istruzioni su come abilitare la modalità classica di IIS6 o IIS7, 
    // visitare il sito Web all'indirizzo http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
     
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        //try
        //{
        //    string sh = CommonHelper.GetParametro<string>(EnumParametriSistema.ShareAllegati);
        //    string share = String.IsNullOrWhiteSpace(sh) ? "" : sh.ToLower();

        //    string matr = CommonHelper.GetCurrentUserMatricola();
        //    if (HttpContext.Current != null &&
        //        HttpContext.Current.Request != null &&
        //        HttpContext.Current.Request.RawUrl != null &&
        //        HttpContext.Current.Request.RawUrl.ToString().ToLower().Contains( share + "/") &&
        //        !   HttpContext.Current.Request.RawUrl.ToString().ToLower().Contains(share+"/"+matr)
        //        )
        //    { 
        //        HttpContext.Current.RewritePath("/Home/notauth");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    var db = new myRaiData.digiGappEntities();
        //    MyRai_LogErrori err = new MyRai_LogErrori()
        //    {
        //        applicativo = "PORTALE",
        //        data = DateTime.Now,
        //        error_message = ex.ToString(),
        //        matricola = CommonHelper.GetCurrentUserMatricola(),
        //        provenienza = "GlobalAsax.BeginRequest"
        //    };
        //    db.MyRai_LogErrori.Add(err);
        //    DBHelper.Save(db);
        //}
        }
        protected void Application_Start()
        {
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ControllerBuilder.Current.SetControllerFactory(typeof(ControllerFactory));
            BundleConfig.RegisterBundles(System.Web.Optimization.BundleTable.Bundles);
        }

   
        protected void Session_Start(object sender, EventArgs e)
        {
            var db = new digiGappEntities();
            MyRai_LogAzioni a = new MyRai_LogAzioni()
            {
                applicativo = "portale",
                data = DateTime.Now,
                matricola = CommonHelper.GetCurrentUserMatricola(),
                provenienza = Logger.GetServerName() + HttpContext.Current.Request.UserHostAddress,
                operazione = "SESSION_START",
                descrizione_operazione = "UA:" + HttpContext.Current.Request.UserAgent + " - IP:"+HttpContext.Current.Request.UserHostAddress + " - Mobile:" +HttpContext.Current.Request.Browser.IsMobileDevice,
            };
            db.MyRai_LogAzioni.Add(a);
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola( ) );
        }

        protected void Session_End(object sender, EventArgs e)
        {
            if (HttpContext.Current == null) return;

            var db = new digiGappEntities();
            MyRai_LogAzioni a = new MyRai_LogAzioni()
            {
                applicativo = "portale",
                data = DateTime.Now,
                matricola = CommonHelper.GetCurrentUserMatricola( ),
                provenienza = Logger.GetServerName() + HttpContext.Current.Request.UserHostAddress,
                operazione = "SESSION_END",
                descrizione_operazione = "UA:" + HttpContext.Current.Request.UserAgent,
            };
            db.MyRai_LogAzioni.Add(a);
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola( ) );
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            //Server.ClearError();

            Type type = ex.GetType();

            


            // Session["idError"] = 2345;
            //if (type.Name=="FormatException")
            //{
            //    try
            //    {
            //        FormatException httpException = (FormatException)exc;

            //    }
            //    catch (Exception ex) { return; }
            //}

            if (type.Name != "HttpException")
            {
                //try
                //{
                //    HttpException httpException = (HttpException)exc;
                //    int httpCode = httpException.GetHttpCode();
                //    if (httpCode == 404)
                //        Response.Redirect("~/Error/NotFound");
                //    else Response.Redirect("~/Error/Error");
                //}
                //catch (Exception ex) { return; }

                var httpContext = ((MvcApplication)sender).Context;
                var currentController = " ";
                var currentAction = " ";
                var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

                if (currentRouteData != null)
                {
                    if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                    {
                        currentController = currentRouteData.Values["controller"].ToString();
                    }

                    if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                    {
                        currentAction = currentRouteData.Values["action"].ToString();
                    }
                }


                var controller = new myRai.Controllers.ErrorController();
                var routeData = new RouteData();
                var action = "Error2";

                HttpRequest sourceRequest = HttpContext.Current.Request;


                httpContext.ClearError();
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
                httpContext.Response.TrySkipIisCustomErrors = true;

                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = action;
                routeData.Values.Add("sourceRequest", sourceRequest);

                controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
                ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));




            }

            //else
            //{

            //    Response.Redirect("~/Error/Error");
            //}
        }
    }
}