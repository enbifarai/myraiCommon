using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class Cons
    {
        public static string RAI_ICON = CommonHelper.GetFilePath("~/assets/img/rai.png");
        //public static string PROFILE_IMAGE = HttpContext.Current.Server.MapPath("~/IMAGES/test.jpg");
        public static string ON = CommonHelper.GetFilePath("~/assets/img/on.png");
        public static string OFF = CommonHelper.GetFilePath("~/assets/img/off.png");

        public static string STAR = CommonHelper.GetFilePath("~/assets/img/star.png");
    }
}