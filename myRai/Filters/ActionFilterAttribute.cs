using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Filters
{
	/// <summary>
	/// Filter attribute che consente di non memorizzare in cache i dati di una chiamata a metodo
	/// </summary>
	public class NoCacheAttribute : ActionFilterAttribute
	{
		public override void OnResultExecuting ( ResultExecutingContext filterContext )
		{
			filterContext.HttpContext.Response.Cache.SetExpires( DateTime.UtcNow.AddDays( -1 ) );
			filterContext.HttpContext.Response.Cache.SetValidUntilExpires( false );
			filterContext.HttpContext.Response.Cache.SetRevalidation( HttpCacheRevalidation.AllCaches );
			filterContext.HttpContext.Response.Cache.SetCacheability( HttpCacheability.NoCache );
			filterContext.HttpContext.Response.Cache.SetNoStore();

			base.OnResultExecuting( filterContext );
		}
	}
}