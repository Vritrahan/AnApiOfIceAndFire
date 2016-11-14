﻿using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using AnApiOfIceAndFire.Infrastructure;
using CacheCow.Server;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AnApiOfIceAndFire
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //Adds a message handler that takes care of all caching for us (ETag and Last-Modified)
            config.MessageHandlers.Add(new CachingHandler(config));

            //Replace the default one with our own so that we can supply a custom message instead of stack trace.
            config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());

            //Set the instrumentation key that will be used to log exceptions to the correct place. Move this out to an activator later on.
            TelemetryConfiguration.Active.InstrumentationKey = WebConfigurationManager.AppSettings["InstrumentationKey"];

            //Add a logger that logs exceptions to Application Insight
            config.Services.Add(typeof(IExceptionLogger), new ApplicationInsightExceptionLogger(new TelemetryClient()));

            //Use indented to make it more readable for the consumer, using gzip is better for bandwidth anyway.
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            //Use camelCase for naming of properties since it's more of a standard
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            //Use the ISO format instead of Microsoft format. This is to make it easier for the consumer to parse the date, especially if they don't use a Microsoft stack themselves.
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            //We want to represent our enums with their names instead of their numerical values. This is to make it more readable for the consumer.
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            //Add our own media type to enable versioning via the accept header. Make this sexier, maybe use reflection to reflect all current namespaces?
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(HeaderVersioningConfig.AllowedAcceptHeaderMediaType)
            {
                Parameters = { new NameValueHeaderValue(HeaderVersioningConfig.AllowedAcceptHeaderMediaTypeParameter, "1") }
            });

            //Remove the possibility to serialize models to XML since we don't want to support that at the moment.
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            EnableCrossSiteRequests(config);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static void EnableCrossSiteRequests(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute(origins: "*", headers: "*", methods: "GET,HEAD");
            config.EnableCors(cors);
        }
    }
}