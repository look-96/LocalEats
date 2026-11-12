using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Restaurants.API.Security;

/// <summary>
/// Applies the destructive-command check to every controller action routed as HTTP DELETE.
/// New DELETE endpoints therefore inherit the laptop-held credential check by default.
/// </summary>
internal sealed class ProtectDeleteActionsConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var action in application.Controllers.SelectMany(controller => controller.Actions))
        {
            var isDeleteAction = action.Selectors
                .SelectMany(selector => selector.ActionConstraints ?? [])
                .OfType<HttpMethodActionConstraint>()
                .Any(constraint => constraint.HttpMethods.Contains(HttpMethods.Delete, StringComparer.OrdinalIgnoreCase));

            if (isDeleteAction)
            {
                action.Filters.Add(new ServiceFilterAttribute(typeof(DestructiveCommandAuthorizationFilter)));
            }
        }
    }
}
