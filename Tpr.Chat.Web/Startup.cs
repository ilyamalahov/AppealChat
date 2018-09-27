﻿using System;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Providers;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web
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
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            // Chat repository
            services.AddTransient<IChatRepository, ChatRepository>(repository => new ChatRepository(connectionString));

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            // Authentication service
            services.AddTransient<IAuthService, AuthService>();

            // Connections service
            services.AddSingleton<IConnectionService, ConnectionService>();

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
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol();

            //services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            // Session
            services.AddDistributedMemoryCache();

            services.AddSession();

            // Authentication
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.NameIdentifier);
                });
            });

            // Authentication
            var jwtConfiguration = Configuration.GetSection("JWT");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Validate the issuer when validating the token
                        ValidateIssuer = true,
                        // Token issuer
                        ValidIssuer = jwtConfiguration["Issuer"],

                        // Validate the token audience
                        ValidateAudience = true,
                        // Token audience
                        ValidAudience = jwtConfiguration["Audience"],

                        ValidateActor = false,

                        // Validate signing key
                        //ValidateIssuerSigningKey = true,
                        // Issuer Signing Key
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration["SecretKey"])),

                        // Validate token lifetime
                        ValidateLifetime = true,

                        LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var tokenResult = context.Request.Query.TryGetValue("access_token", out var accessToken);

                            var isWebsocketRequest = context.HttpContext.WebSockets.IsWebSocketRequest;

                            var isEventStreamRequest = context.Request.Headers["Accept"] == "text/event-stream";

                            // If the request is for our hub...
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

            // MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
