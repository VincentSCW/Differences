﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Differences.Api.Model;
using Differences.Api.Mutations;
using Differences.Api.Queries;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Differences.Api
{
    public partial class Startup
    {
        private static void InjectGraphQL(IServiceCollection services)
        {
            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();

            services.AddTransient<UserType>();
            services.AddTransient<UserInputType>();
            services.AddTransient<AnswerType>();
            services.AddTransient<AnswerLikeType>();
            services.AddTransient<QuestionType>();
            services.AddTransient<LikeInputType>();
            services.AddTransient<ReplyInputType>();
            services.AddTransient<SubjectInputType>();
            services.AddTransient<CriteriaInputType>();
            services.AddTransient<CategoryType>();
            services.AddTransient<CategoryGroupType>();
            services.AddTransient<AuthResponseType>();

            services.AddScoped<DifferencesQuery>();
            services.AddScoped<DifferencesMutation>();
            services.AddScoped<ISchema>(
                s => new GraphQLSchema(new FuncDependencyResolver(type => (GraphType) s.GetService(type))));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
