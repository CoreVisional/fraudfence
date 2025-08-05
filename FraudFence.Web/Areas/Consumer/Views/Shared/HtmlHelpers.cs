using Microsoft.AspNetCore.Mvc.Rendering;

namespace FraudFence.Web.Areas.Consumer.Views.Shared;

public static class HtmlHelpers
{
    public static string IsSelected(this IHtmlHelper htmlHelper, string controller, string action, string cssClass = "active")
    {
        var routeData = htmlHelper.ViewContext.RouteData;

        var routeAction = routeData.Values["action"]?.ToString();
        var routeController = routeData.Values["controller"]?.ToString();

        return controller.Equals(routeController, StringComparison.OrdinalIgnoreCase) &&
               action.Equals(routeAction, StringComparison.OrdinalIgnoreCase)
            ? cssClass
            : string.Empty;
    }
}