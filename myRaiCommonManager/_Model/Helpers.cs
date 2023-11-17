using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace MVCProject.Custom_Helpers
{
    public static class Custom_Helper_Class {


        public static IHtmlString TourAttributes(this HtmlHelper helper, List<myRaiData.MyRai_Tour> TourElements,
            string elementName)
        {
           var tourDb= TourElements.Where(x => x.codice_elemento == elementName).FirstOrDefault();
           if (tourDb == null || string.IsNullOrWhiteSpace(tourDb.testo)) return null;
           return MvcHtmlString.Create(" data-tourtitle=\""+tourDb.titolo+"\" data-tourstep=\"" + tourDb.step + "\" data-tourtext=\"" + tourDb.testo + "\"");
        }

        public static IHtmlString IntroTourAttributes(this HtmlHelper helper, List<myRaiData.MyRai_Tour> TourElements,
            string elementName)
        {
            var tourDb = TourElements.Where(x => x.codice_elemento == elementName).FirstOrDefault();
            if (tourDb == null) return null;
            string tit = "<h5 class=\" titolo-tour text-primary text-bold\">" + tourDb.titolo + "</h5>";

            return MvcHtmlString.Create(" data-step='" + tourDb.step + "' data-intro='" +tit+ "<p class=\"testo-tour\">"+ tourDb.testo + "</p>'");
        }

        //public static IHtmlString AsyncPartial(this HtmlHelper helper, string controller, string action, string partialName=null, TempDataDictionary TempData=null, object model=null)
        //{
            
           
        //    string g =  "container_"+controller+"_"+action ;
        //    string HtmlSkeleton = helper.Partial(partialName == null ? action + "_skeleton" : partialName, model).ToString();
        //    string js = "<script>"
        //            + "function " + g + "_refresh(){ "
        //                  + "var d=document.getElementById('" + g + "');"
        //                  + "var ds=document.getElementById('" + g + "_s');"

        //                  + "var r = new XMLHttpRequest();"
        //                  + "r.onreadystatechange  = function() {"
        //                        + "if (r.status >= 200 && r.status < 400) { "
        //                        + "var resp = r.responseText.replace('data-refresh-partial','data-refresh-partial onclick=\"" + g + "_refresh()\"');"
        //                        + "d.innerHTML = resp;"
        //                        + "ds.style.display='none';"
        //                        + "d.style.display='block';"
        //                        + "                             }"
								//+ " else if (r.status == 500) { }"
        //                 + "};"
        //            + "r.open('GET', " + "'/" + controller + "/" + action + "', true);"
        //            + "ds.style.display='block';"
        //            + "d.style.display='none';"
        //            + "r.send();"

        //        + "}"
        //        + g + "_refresh();"
        //        + "</script>";
        //    TempData["script"] += js;
        //    return  MvcHtmlString.Create(
        //        "<div id='"+g+"'>"+ HtmlSkeleton+ "</div>"
        //        +"<div style='display:none' id='"+g+"_s'>"+HtmlSkeleton+"</div>"
               
        //        );
        //}

        public static IHtmlString AsyncPartialCV(this HtmlHelper helper, string controller, string action, string partialName = null, TempDataDictionary TempData = null)
        {
            string g = "container_" + controller + "_" + action;
            string HtmlSkeleton = helper.Partial(partialName == null ? action + "_skeleton" : partialName).ToString();

            string url = "/" + controller + "/" + action;
            string js = "<script>"
                    + "function " + g + "_refresh(){ "
                          + "var d=document.getElementById('" + g + "');"
                          + "$('#" + g + "_s').show();"
                          + "$('#" + g + "').hide();"
                          + "$('#" + g + "').load(\"" + url + "\", function() { "
                          + "$('#" + g + "_s').hide();"
                          + "$('#" + g + "').show();"
                          + "});"
                + "}"
                + g + "_refresh();"
                + "</script>";
            TempData["script"] += js;
            return MvcHtmlString.Create(
                "<div id='" + g + "'>" + HtmlSkeleton + "</div>"
                + "<div style='display:none' id='" + g + "_s'>" + HtmlSkeleton + "</div>"
                );
        }


		public static IHtmlString AsyncPartial ( this HtmlHelper helper, string controller, string action, string partialName = null, TempDataDictionary TempData = null, string callBack = null, object model=null, object routeValues=null )
		{
            string urlAction = new UrlHelper(HttpContext.Current.Request.RequestContext).Action(action, controller, routeValues);

			string g = "container_" + controller + "_" + action;
            string HtmlSkeleton = helper.Partial( partialName == null ? action + "_skeleton" : partialName , model ).ToString( );
			string js = "<script>"
					+ "function " + g + "_refresh(){ "
						  + "var d=document.getElementById('" + g + "');"
						  + "var ds=document.getElementById('" + g + "_s');"

						  + "var r = new XMLHttpRequest();"
						  + "r.onreadystatechange  = function() {"
								+ "if (r.status >= 200 && r.status < 400) { "
								+ "var resp = r.responseText.replace('data-refresh-partial','data-refresh-partial onclick=\"" + g + "_refresh()\"');"
								+ "d.innerHTML = resp;"
								+ "ds.style.display='none';"
								+ "d.style.display='block';"
								+ ((callBack != null) ? callBack : "")
								+ "                             }"
								+ " else if (r.status == 500) { }"
						 + "};"
					//+ "r.open('GET', " + "'/" + controller + "/" + action + "', true);"
                    + "r.open('GET', '"+urlAction+"', true);"
					+ "ds.style.display='block';"
					+ "d.style.display='none';"
					+ "r.send();"

				+ "}"
				+ g + "_refresh();"
				+ "</script>";
			TempData["script"] += js;
			return MvcHtmlString.Create(
				"<div id='" + g + "'>" + HtmlSkeleton + "</div>"
				+ "<div style='display:none' id='" + g + "_s'>" + HtmlSkeleton + "</div>"

				);
		}

        public static IHtmlString AsyncPartial(this HtmlHelper helper, string controller, string action, string parameters, string partialName = null, TempDataDictionary TempData = null, string callBack = null, object model=null, object routeValues=null)
        {
            string g = "container_" + controller + "_" + action;
            string HtmlSkeleton = helper.Partial(partialName == null ? action + "_skeleton" : partialName, model).ToString();
            string js = "<script>"
                    + "function " + g + "_refresh(){ "
                          + "var d=document.getElementById('" + g + "');"
                          + "var ds=document.getElementById('" + g + "_s');"

                          + "var r = new XMLHttpRequest();"
                          + "r.onreadystatechange  = function() {"
                                + "if (r.status >= 200 && r.status < 400) { "
                                + "var resp = r.responseText.replace('data-refresh-partial','data-refresh-partial onclick=\"" + g + "_refresh()\"');"
                                + "d.innerHTML = resp;"
                                + "ds.style.display='none';"
                                + "d.style.display='block';"
                                + ((callBack != null) ? callBack : "")
                                + "                             }"
                                + " else if (r.status == 500) { }"
                         + "};"
                    + "r.open('GET', " + "'/" + controller + "/" + action+parameters+"', true);"
                    + "ds.style.display='block';"
                    + "d.style.display='none';"
                    + "r.send();"

                + "}"
                + g + "_refresh();"
                + "</script>";
            TempData["script"] += js;
            return MvcHtmlString.Create(
                "<div id='" + g + "'>" + HtmlSkeleton + "</div>"
                + "<div style='display:none' id='" + g + "_s'>" + HtmlSkeleton + "</div>"

                );
        }

    }  
}
namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}