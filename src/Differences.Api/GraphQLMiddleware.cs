﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataLoader;
using Differences.Api.Model;
using Differences.Api.Rules;
using Differences.Common;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace Differences.Api
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GraphQLSettings _settings;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;

        public GraphQLMiddleware(
            RequestDelegate next,
            GraphQLSettings settings,
            IDocumentExecuter executer,
            IDocumentWriter writer)
        {
            _next = next;
            _settings = settings;
            _executer = executer;
            _writer = writer;
        }

        public async Task Invoke(HttpContext context, ISchema schema)
        {
            if (!IsGraphQLRequest(context))
            {
                await _next(context);
                return;
            }

            await ExecuteAsync(context, schema);
        }

        private bool IsGraphQLRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_settings.Path)
                   && string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteAsync(HttpContext context, ISchema schema)
        {
            string body;
            using (var streamReader = new StreamReader(context.Request.Body))
            {
                body = await streamReader.ReadToEndAsync().ConfigureAwait(true);
            }

            var request = JsonConvert.DeserializeObject<GraphQLRequest>(body);

            var validationRules = new List<IValidationRule>()
            {
                new RequiresAuthValidationRule()
            };

            var result = await DataLoaderContext.Run(ctx => _executer.ExecuteAsync(_ =>
            {
                _.OperationName = request.OperationName;
                _.Schema = schema;
                _.Query = request.Query;
                _.Inputs = request.Variables.ToInputs();
                _.ValidationRules = validationRules.Concat(DocumentValidator.CoreRules());
                _.UserContext = new GraphQLUserContext(ctx, context.User);
            }));

            await WriteResponseAsync(context, result);
        }

        private async Task WriteResponseAsync(HttpContext context, ExecutionResult result)
        {
            var json = _writer.Write(result);

            context.Response.ContentType = "application/json";
            if (result.Errors != null && result.Errors.Any() && !result.Errors.All(x => x is DefinedException))
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            else
                context.Response.StatusCode =  (int)HttpStatusCode.OK;

            await context.Response.WriteAsync(json);
        }
    }
}
