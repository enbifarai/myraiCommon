using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace myRaiHelper
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
