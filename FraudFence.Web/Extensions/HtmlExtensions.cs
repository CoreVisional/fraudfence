using Microsoft.AspNetCore.Mvc.Rendering;

namespace FraudFence.Web.Extensions
{
    public static class HtmlExtensions
    {
        public static string IsSelected(this IHtmlHelper html, string controller, string action, string htmlClass)
        {
            string? currentAction = html.ViewContext.RouteData.Values["action"]?.ToString();
            string? currentController = html.ViewContext.RouteData.Values["controller"]?.ToString();

            if (string.IsNullOrEmpty(controller))
            {
                controller = currentController ?? string.Empty;
            }

            if (string.IsNullOrEmpty(action))
            {
                action = currentAction ?? string.Empty;
            }

            return string.Equals(controller, currentController, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(action, currentAction, StringComparison.OrdinalIgnoreCase) ?
                htmlClass : string.Empty;
        }

        public static string IsSelected(this IHtmlHelper html, string controller, string htmlClass)
        {
            string? currentController = html.ViewContext.RouteData.Values["controller"]?.ToString();

            if (string.IsNullOrEmpty(controller))
            {
                controller = currentController ?? string.Empty;
            }

            return string.Equals(controller, currentController, StringComparison.OrdinalIgnoreCase) ?
                htmlClass : string.Empty;
        }
    }
}
