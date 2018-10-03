using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Providers;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public ILogger<Startup> Logger { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            // Chat repository
            services.AddTransient<IChatRepository, ChatRepository>(repository => new ChatRepository(connectionString));

            //services.AddHostedService<TimedHostedService>();

            //services.AddHostedService<QueuedHostedService>();
            //services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            // Authentication service
            services.AddTransient<IAuthService, AuthService>();

            // Connections service
            services.AddSingleton<IConnectionService, ConnectionService>();

            services.AddSingleton<IClientService, ClientService>();
            // Cross-Origin Request Sharing
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins(Configuration["AllowOrigins"])
                    .AllowCredentials();
            })
            );

            // SignalR
            services.AddSignalR();

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.NameIdentifier);
                });
            });

            services.AddDistributedMemoryCache();

            services.AddSession();

            // Authentication
            var jwtConfiguration = Configuration.GetSection("JWT");

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration["SecretKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudience = jwtConfiguration["Audience"],

                        ValidateIssuer = true,
                        ValidIssuer = jwtConfiguration["Issuer"],

                        ValidateLifetime = true,
                        LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,

                        IssuerSigningKey = securityKey
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var tokenResult = context.Request.Query.TryGetValue("access_token", out var accessToken);

                            var isWebsocketRequest = context.HttpContext.WebSockets.IsWebSocketRequest;

                            var isEventStreamRequest = context.Request.Headers["Accept"] == "text/event-stream";

                            if (tokenResult && (isWebsocketRequest || isEventStreamRequest))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            // MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //
            app.UseStatusCodePages();

            // HTTPS rediretion
            app.UseHttpsRedirection();

            // Static files
            app.UseStaticFiles();

            // CORS
            app.UseCors("CorsPolicy");

            // SignalR
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chat");
                routes.MapHub<InfoHub>("/info");
            });

            app.UseSession();

            // Authentication
            app.UseAuthentication();

            var routeBuilder = new RouteBuilder(app);
            //routeBuilder.MapGet("token", context => context.Response.WriteAsync(GenerateToken(context)));
            routeBuilder.MapGet("test", Test);
            app.UseRouter(routeBuilder.Build());

            // MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute("Home", "{appealId:guid}", new { controller = "Home", action = "Index" });
            });
        }

        private Task Test(HttpContext context)
        {
            Logger.LogInformation("Page closed");
            Logger.LogDebug("Appeal ID: {0}", context.Request.Query["appeaId"]);

            return Task.CompletedTask;
        }

        //private string GenerateToken(HttpContext context)
        //{
        //    var jwtConfiguration = Configuration.GetSection("JWT");

        //    var appealId = context.Request.Query["appealId"];

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, appealId.ToString())
        //    };

        //    // Expert key
        //    var expertKey = context.Request.Query["expertKey"];

        //    if (expertKey.Count > 0)
        //    {
        //        claims.Add(new Claim("expertkey", expertKey));
        //    }

        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"]));

        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        jwtConfiguration["Issuer"],
        //        jwtConfiguration["Audience"],
        //        claims,
        //        expires: DateTime.UtcNow.AddSeconds(30),
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }
}
