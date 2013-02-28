﻿using System.Web.Mvc;

namespace Glitch.Notifier.AspNet.Mvc
{
    public class MvcError : HttpError
    {
        private readonly ExceptionContext _exceptionContext;

        public MvcError(ExceptionContext exceptionContext)
            : base(exceptionContext.Exception, exceptionContext.HttpContext)
        {
            _exceptionContext = exceptionContext;
            Error.WithLocation(GetController() + "#" + GetAction());
        }

        public MvcError WithContextData()
        {
            return
                HttpContextErrorExtensions.WithContextData(this)
                    .WithRouteData();
        }


        public MvcError WithRouteData()
        {
            Error.With("RouteData", _exceptionContext.RouteData.Values);
            return this;
        }

        private string GetController()
        {
            return _exceptionContext.RouteData.Values["controller"] as string;
        }

        private string GetAction()
        {
            return _exceptionContext.RouteData.Values["action"] as string;
        }
    }
}
