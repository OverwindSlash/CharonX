using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.ExceptionHandling;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Runtime.Validation;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CharonX.ResulFilter
{
    public class CustomExceptionFilter : AbpExceptionFilter
    {
        public CustomExceptionFilter(IErrorInfoBuilder errorInfoBuilder, IAbpAspNetCoreConfiguration configuration) : base(errorInfoBuilder, configuration)
        {
        }
        protected override int GetStatusCode(ExceptionContext context,bool wrapOnError)
        {
            if (context.Exception is AbpAuthorizationException)
            {
                return context.HttpContext.User.Identity.IsAuthenticated
                    ? (int)HttpStatusCode.Forbidden
                    : (int)HttpStatusCode.Unauthorized;
            }

            if (context.Exception is AbpValidationException)
            {
                return (int)HttpStatusCode.BadRequest;
            }

            if (context.Exception is EntityNotFoundException)
            {
                return (int)HttpStatusCode.NotFound;
            }

            if (wrapOnError)
            {
                if (context.Exception is AppUserFriendlyException)
                {
                    return (int)HttpStatusCode.OK;
                }
                return (int)HttpStatusCode.InternalServerError;
            }

            return context.HttpContext.Response.StatusCode;
        }
    }
}
