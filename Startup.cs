using AutoMapper;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//using CourseLibrary.API.OperationFilters;

//[assembly: ApiConventionType(typeof(DefaultApiConventions))] //такой вариант Convention (глобальный)
namespace CourseLibrary.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddControllers(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.EnableEndpointRouting = false; //должно быть 'false' при 'app.UseMvc()'

                //setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest)); //такой
                //setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable)); //вариант
                //setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError)); //Convention (глобальный)
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized));

                //setupAction.OutputFormatters.Add(new XmlSerializerOutputFormatter()); //странно, но без этой строки тоже работает)

                setupAction.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
            }
            ).AddNewtonsoftJson(setupAction =>
             {
                 setupAction.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
             })
             .AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(setupAction =>
            {
                setupAction.InvalidModelStateResponseFactory = context =>
                {
                    // create a problem details object
                    var problemDetailsFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();
                    var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                            context.HttpContext, 
                            context.ModelState); 

                    // add additional info not added by default
                    problemDetails.Detail = "See the errors field for details.";
                    problemDetails.Instance = context.HttpContext.Request.Path;

                    // find out which status code to use
                    var actionExecutingContext =
                          context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors & all keys were correctly
                    // found/parsed we're dealing with validation errors
                    //
                    // if the context couldn't be cast to an ActionExecutingContext
                    // because it's a ControllerContext, we're dealing with an issue 
                    // that happened after the initial input was correctly parsed.  
                    // This happens, for example, when manually validating an object inside
                    // of a controller action.  That means that by then all keys
                    // WERE correctly found and parsed.  In that case, we're
                    // thus also dealing with a validation error.
                    if (context.ModelState.ErrorCount > 0 &&
                        (context is ControllerContext ||
                         actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
                    {
                        problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                        problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                        problemDetails.Title = "One or more validation errors occurred.";

                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    }

                    // if one of the keys wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparsable input
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "One or more errors on input occurred.";
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                };
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            #region ConfigSpec
            services.AddVersionedApiExplorer(setupAction =>
            {
                setupAction.GroupNameFormat = "'v'VV"; //формат версий API
            });

            services.AddApiVersioning(setupAct =>
            {
                setupAct.AssumeDefaultVersionWhenUnspecified = true;
                setupAct.DefaultApiVersion = new ApiVersion(1, 1);
                setupAct.ReportApiVersions = true;
                //setupAct.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //setupAct.ApiVersionReader = new MediaTypeApiVersionReader();
            });

            services.AddSwaggerGen(se =>
            {
                var ApiVDP = services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();
                foreach (var description in ApiVDP.ApiVersionDescriptions) //foreach перебирает все варианты названий спецификаций и версий API
                {
                    se.SwaggerDoc($"LibraryOpenAPISpecification{description.GroupName}", new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Title = "API",
                        Version = description.ApiVersion.ToString(), //якобы версия, а настоящие версии указываются с помощью 'new ApiVersion(1, 0)'
                        Description = "Access to authors and courses",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact
                        {
                            Email = "maxim@gmail.com",
                            Name = "Maxim",
                            Url = new Uri("http://maxim.com")
                        }
                    });
                }

                se.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
                {
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "basic",
                    Description = "Login 'n' Password"
                });

                se.AddSecurityRequirement(new OpenApiSecurityRequirement //как работает???!!!
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basicAuth"
                            }
                        },
                        new List<string>()
                    }
                });

                #region SwaggerDoc
                //se.SwaggerDoc("LibraryOpenAPISpecificationCourses", new Microsoft.OpenApi.Models.OpenApiInfo пока уберем 2 спецификацию
                //{
                //    Title = "Courses API",
                //    Version = "1.2",
                //    Description = "Access to authors' courses",
                //    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                //    {
                //        Email = "maxim@gmail.com",
                //        Name = "Maxim",
                //        Url = new Uri("http://maxim.com")
                //    }
                //});
                #endregion

                //обязательная часть к 'IApiVersionDescriptionProvider' с циклом foreach (выше)
                se.DocInclusionPredicate((docName, ApiDesc) =>
                {
                    var actApiVersionModel = ApiDesc.ActionDescriptor.GetApiVersionModel
                    (ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                    if (actApiVersionModel == null)
                        return true;

                    if (actApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actApiVersionModel.DeclaredApiVersions.Any(apiV => $"LibraryOpenAPISpecificationv{apiV}" == docName);
                    }

                    return actApiVersionModel.ImplementedApiVersions.Any(apiV => $"LibraryOpenAPISpecificationv{apiV}" == docName);
                });


                se.OperationFilter<OperationFilters.GetCourseFilter>();


                //разрешение xml комментариев
                string xmlComment = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlComment);
                se.IncludeXmlComments(xmlPath);
            });
            #endregion

            services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, Auth.Authentication>("Basic", null);

            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options => options.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=Library;Trusted_Connection=False;"));
        }


        internal static IActionResult ProblemDetailsInvalidModelStateResponse(
            ProblemDetailsFactory problemDetailsFactory, ActionContext context)
        {
            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            ObjectResult result;
            if (problemDetails.Status == 400)
            {
                // For compatibility with 2.x, continue producing BadRequestObjectResult instances if the status code is 400.
                result = new BadRequestObjectResult(problemDetails);
            }
            else
            {
                result = new ObjectResult(problemDetails);
            }
            result.ContentTypes.Add("application/problem+json");
            result.ContentTypes.Add("application/problem+xml");

            return result;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider AVDP)
        {
            //app.UseHsts();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(setupAct =>
            {
                //запись всех спецификаций, сгенерированных в 'services.AddSwaggerGen'
                foreach (var description in AVDP.ApiVersionDescriptions)
                {
                    setupAct.SwaggerEndpoint($"/swagger/LibraryOpenAPISpecification{description.GroupName}/swagger.json", description.GroupName); //здесь 2 аргумент - всего лишь якобы имя спец-ии, выводимое в UI
                    //setupAct.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationCourses/swagger.json", "Courses API"); для ручной генерации спецификаций!

                    //setupAct.RoutePrefix = ""; //не надо!
                }
            });

            app.UseStaticFiles();

            app.UseMvc();

            app.UseAuthentication();

            //app.UseRouting();

            //app.UseAuthorization();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
