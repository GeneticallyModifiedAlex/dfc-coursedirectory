﻿using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBehaviors(this IServiceCollection services)
        {
            foreach (var type in typeof(ServiceCollectionExtensions).Assembly.GetTypes())
            {
                RegisterRestrictQAStatusBehavior(type);
            }

            return services;

            void RegisterRestrictQAStatusBehavior(Type type)
            { 
                // For any type implementing IRestrictQAStatus<TRequest, TResponse>
                // add a RestrictQAStatusBehavior behavior for its request & response types

                var restrictQaStatusType = typeof(IRestrictQAStatus<,>);

                var restrictTypes = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == restrictQaStatusType)
                    .ToList();

                foreach (var t in restrictTypes)
                {
                    var requestType = t.GenericTypeArguments[0];
                    var responseType = t.GenericTypeArguments[1];

                    var pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
                    var behaviourType = typeof(RestrictQAStatusBehavior<,>).MakeGenericType(requestType, responseType);
                    services.AddScoped(pipelineBehaviorType, behaviourType);

                    services.AddScoped(t, type);
                }
            }
        }
    }
}
