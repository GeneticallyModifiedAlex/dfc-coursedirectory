﻿using System;
using System.Linq;
using System.Reflection;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowNoCurrentProviderAttribute : Attribute
    {
    }

    public class RedirectToProviderSelectionActionFilter : IActionFilter, IOrderedFilter
    {
        public RedirectToProviderSelectionActionFilter()
        {
        }

        public int Order => 9;

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor?.MethodInfo.GetCustomAttribute<AllowNoCurrentProviderAttribute>() != null ||
                controllerActionDescriptor?.ControllerTypeInfo.GetCustomAttribute<AllowNoCurrentProviderAttribute>() != null)
            {
                return;
            }

            var hasProviderInfoParameter = context.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(ProviderInfo))
                .Any();

            var requiresProviderContext = controllerActionDescriptor
                ?.MethodInfo.GetCustomAttribute<RequireProviderContextAttribute>() != null;

            if (hasProviderInfoParameter || requiresProviderContext)
            {
                var providerContextFeature = context.HttpContext.Features.Get<ProviderContextFeature>();

                if (providerContextFeature?.ProviderInfo == null)
                {
                    context.Result = new RedirectToActionResult(
                        "Index",
                        "Home",
                        new { returnUrl = context.HttpContext.Request.GetEncodedUrl() });
                }
            }
        }
    }
}
