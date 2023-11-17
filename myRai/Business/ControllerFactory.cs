using myRai.Controllers;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace myRai
{
    public class ControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType != null)
                return base.GetControllerInstance(requestContext, controllerType);

            return new BaseCommonController();
        }

    }
}
