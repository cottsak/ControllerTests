using System;
using System.Web;
using System.Web.Mvc;

namespace MVCControllerTestsWithLocalDb.Web.Helpers
{
    public static class PageAlertHelper
    {
        public const string PageAlertTempDataKey = "PageNotification";

        public static IHtmlString RenderPageAlert(this HtmlHelper helper, bool useInlineElement = false)
        {
            var alert = helper.ViewContext.TempData[PageAlertTempDataKey] as Tuple<string, AlertType>;
            if (alert == null)
                return new HtmlString(string.Empty);

            var e = useInlineElement ? "span" : "p";
            var cssSelector = "-" + alert.Item2.ToString().ToLower();
            var result = string.Format("<{0} class='alert alert{1}'>{2}</{0}>", e, cssSelector, alert.Item1);
            helper.ViewContext.TempData[PageAlertTempDataKey] = null;
            return new HtmlString(result);
        }

        /// <summary>
        /// This should be used in the root layout view simply to ensure that a dev doesn't forget to
        /// show a message using RenderPageAlert in a view.
        /// </summary>
        public static IHtmlString ThrowIfPageAlertPresentAndNotRendered(this HtmlHelper helper)
        {
            if (helper.ViewContext.TempData[PageAlertTempDataKey] != null)
                throw new ApplicationException("A page alert was found. Please make sure it is rendered in a view. Use the RenderPageAlert helper.");

            return new HtmlString(string.Empty);
        }
    }

    public enum AlertType
    {
        Warning,
        Info,
        Success,
        Danger
    }
}