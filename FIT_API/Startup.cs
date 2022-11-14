using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;

using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;

namespace FIT_API
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

            #region Á¾¿µ
            services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddControllers()
           .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            string _a = "thisisasecretkeyanddontsharewithanyone";

            var key = Encoding.ASCII.GetBytes(_a);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
             .AddJwtBearer(x =>
             {
                 x.RequireHttpsMetadata = true;
                 x.SaveToken = true;
                 x.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(key),
                     ValidateIssuer = false,
                     ValidateAudience = false,
                     ClockSkew = TimeSpan.Zero
                 };
             });

            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
            services.AddSwaggerGen(c =>
            {

                c.SwaggerDoc("v1.0",
                        new OpenApiInfo
                        {
                            Title = "Tripbox Supply API",
                            Version = "v1.0",
                            Description = "SignInOAuth Test Page Link <br> https://tripboxsupplyapi.azurewebsites.net/SignIn.html",
                            TermsOfService = new Uri("https://tripboxsupplyapi.azurewebsites.net//SignIn.html"),
                            Contact = new OpenApiContact
                            {
                                Name = "Publishing 220607",
                                Url = new Uri("https://tripboxsupplyapi.azurewebsites.net/publishing/220607_work/index.html")
                            }
                        });
                c.ExampleFilters();
                c.OperationFilter<SwaggerParameterFilters>();
                c.DocumentFilter<SwaggerVersionMapping>();

                c.EnableAnnotations();

                var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
                c.IncludeXmlComments(filePath);

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                };

                c.AddSecurityRequirement(securityRequirement);

            });
            //services.AddSwaggerExamplesFromAssemblyOf<ProductInCartExample>();

            #endregion

            //services.AddScoped<IDapper, Dapperr>();

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials();
            }));

            #region jun
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder.AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader());
            //});

            ////services.AddControllers()
            ////.AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            //services.AddControllers().AddNewtonsoftJson();

            //services.AddMvc(option => option.EnableEndpointRouting = false);

            //services.AddControllers()
            //.AddNewtonsoftJson(options =>
            //{
            //    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //});


            //services.AddSignalR();

            //services.AddSwaggerGen(c =>
            //{

            //    c.SwaggerDoc("v1.0", 
            //            new OpenApiInfo { 
            //                    Title = "Tripbox Supply API", 
            //                    Version = "v1.0" 
            //                    ,
            //                    Contact = new OpenApiContact
            //                    {
            //                        Name = "Example Contact",
            //                        Url = new Uri("https://example.com/contact")
            //                    },
            //                    License = new OpenApiLicense
            //                    {
            //                        Name = "Example License",
            //                        Url = new Uri("https://example.com/license")
            //                    }
            //            });
            //    c.EnableAnnotations();
            //    var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
            //    c.IncludeXmlComments(filePath);

            //});
            #endregion

            #region API versioning

            //services.AddApiVersioning(options =>
            //{
            //    options.ReportApiVersions = true;
            //    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            //});

            //services.AddVersionedApiExplorer(options =>
            //{
            //    options.GroupNameFormat = "'v'VVV";
            //    options.SubstituteApiVersionInUrl = true;
            //});

            #endregion

            #region Swagger

            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            //services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();
            //services.AddSwaggerGen(options =>
            //{
            //    options.IncludeXmlComments(xmlFilePath);
            //});

            #endregion


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Tripbox API Endpoint");
                ui.InjectStylesheet("/swagger-ui/theme-feeling-blue.css");
                ui.RoutePrefix = string.Empty;
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
