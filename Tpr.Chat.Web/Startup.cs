using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Services;

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

            // Authentication service
            services.AddTransient<IAuthService, AuthService>();

            // Client service
            services.AddSingleton<IClientService, ClientService>();

            // Task service
            services.AddSingleton<ITaskService, TaskService>();

            // Background task queue service
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            // Background task hosted service
            services.AddHostedService<QueuedHostedService>();

            // SignalR
            services.AddSignalR();

            // CORS (Cross-Origin Request Sharing)
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins(Configuration["AllowOrigins"])
                    .AllowCredentials();
            })
            );

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.NameIdentifier);
                });
            });

            // Session
            //services.AddDistributedMemoryCache();

            //services.AddSession();

            // Authentication
            var jwtConfiguration = Configuration.GetSection("JWT");

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration["SecretKey"]));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Audience
                        ValidateAudience = true,
                        ValidAudience = jwtConfiguration["Audience"],

                        // Issuer
                        ValidateIssuer = true,
                        ValidIssuer = jwtConfiguration["Issuer"],

                        // Lifetime
                        ValidateLifetime = true,
                        LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,

                        // Signing key
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

            //app.UseSession();

            // Authentication
            app.UseAuthentication();

            // MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute("Home", "{appealId:guid}", new { controller = "Home", action = "Index" });
                routes.MapRoute("Modal", "{controller=Modal}/{action}");
            });
        }
    }
}
