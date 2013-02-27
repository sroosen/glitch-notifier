using System;
using System.Web;

namespace Glitch.Notifier.AspNet
{
    public static class GlitchErrorFactoryExtensions
    {
        public static HttpError HttpContextError(this GlitchErrorFactory factory, Exception exception, HttpContext context)
        {
           return HttpContextError(factory, exception, new HttpContextWrapper(context));
        }

        public static HttpError HttpContextError(this GlitchErrorFactory factory, Exception exception, HttpContextBase context)
        {
            if (context == null) throw new ArgumentNullException("context");
            return new HttpError(exception, context);
        }

        public static HttpError HttpContextError(this GlitchErrorFactory factory, Exception exception, string errorProfile)
        {
            return HttpContextError(factory, exception, HttpContext.Current).WithErrorProfile(errorProfile);
        }
    }
}