using Application.Services;
using Application.SignalR;
using EFCoreSecondLevelCacheInterceptor;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Application.Domain;
using Application.Domain.Enums.User;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.Persistence;
using Application.Persistence.Repositories;

namespace Application.Helpers
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddApiVersioning(x =>
          {
              x.DefaultApiVersion = new ApiVersion(1, 0);
              x.AssumeDefaultVersionWhenUnspecified = true;
              x.ReportApiVersions = true;
              x.ApiVersionReader = new UrlSegmentApiVersionReader();
          });

            // Setup Firebase
            string firebaseCred = config.GetValue<string>("Authentication:FirebaseKey");
            // string firebaseCred = config.GetValue<string>("AIzaSyCFJOGAnHOQaWntVhN1a16QINIAjVpWaXI");
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(firebaseCred)
            }, "[DEFAULT]");

            services.AddHttpContextAccessor();
            services.AddRouting(options => options.LowercaseUrls = true);
            // services.AddHostedService<TaskRunService>();
            services.AddHostedService<BackgroundStartupTask>();
            services.AddHostedService<BackgroundHourTaskService>();
            services.AddHostedService<RedisQueueRunner>();

            // services.AddSingleton<TaskQueueService>();
            services.AddSingleton<IBoxService, BoxService>();
            services.AddSingleton<IQueueService, RedisQueueService>();

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<UnitOfWork>();

            services.AddScoped<IFirebaseService, FirebaseService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IProjectMemberService, ProjectMemberService>();
            services.AddScoped<IProjectReportService, ProjectReportService>();
            services.AddScoped<IAttributeService, AttributeService>();
            services.AddScoped<ISponsorService, SponsorService>();
            services.AddScoped<IProjectSponsorService, ProjectSponsorService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<IProjectMilestoneService, ProjectMilestoneService>();
            services.AddScoped<IPayslipService, PayslipService>();
            services.AddScoped<ISystemService, SystemService>();
            services.AddScoped<ISalaryCycleService, SalaryCycleService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IMemberVoucherService, MemberVoucherService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPayslipService, PayslipService>();
            services.AddScoped<IPayslipItemService, PayslipItemService>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<IProjectReportMemberTasksService, ProjectReportMemberTasksService>();
            services.AddScoped<IProjectEndRequestService, ProjectEndRequestService>();

            services.AddHttpContextAccessor();
            services.AddSingleton<GlobalVar>();
            // services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSignalR();
            services.AddSingleton<MemberConnections>();

            services
              .AddSwaggerGen(c =>
              {
                  c.EnableAnnotations();
                  c.OrderActionsBy((apiDesc) => $"{apiDesc.RelativePath}");

                  c.SwaggerDoc("v1", new OpenApiInfo { Title = "CnB API", Version = "v0.0.2" });

                  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                  c.IncludeXmlComments(xmlPath, true);

                  var securitySchema = new OpenApiSecurityScheme
                  {
                      Description = "JWT Authorization",
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


            // DataContext
            //var queueName = config.GetValue<string>("redis:queueName");
            //if (queueName.Contains("Test"))
            //{
            //    services
            //        .AddDbContext<DataContext>((opt) =>
            //        {
            //            opt.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            //        });
            //}
            //else
            //{
                services
                    .AddDbContext<DataContext>((prov, opt) =>
                    {
                        opt.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                                sqlOpts =>
                                {
                                    sqlOpts.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds);
                                })
                            .EnableThreadSafetyChecks(false);
                    });
            //}


            // Add Redis Cache

         //   const string _providerName = "_redis1";
         //   services.AddEasyCaching(option =>
         //   {
         //       option
         // .UseRedis(rConfig =>
         // {
         //     rConfig.DBConfig.AllowAdmin = true;
         //     rConfig.DBConfig.SyncTimeout = 10000;
         //     rConfig.DBConfig.AsyncTimeout = 10000;
         //     rConfig.DBConfig.Password = config.GetValue<string>("redis:password");
         //     rConfig.DBConfig.Endpoints.Add(
         // new EasyCaching.Core.Configurations.ServerEndPoint(
         //    config.GetValue<string>("redis:endpoint:host"),
         //   config.GetValue<int>("redis:endpoint:port")));
         //     rConfig.EnableLogging = false;
         //     rConfig.SerializerName = "Pack";
         //     rConfig.DBConfig.ConnectionTimeout = 10000;
         // }, _providerName)
         // // .UseRedis(configuration: config, _providerName)
         // .WithMessagePack(so =>
         // {
         //     so.EnableCustomResolver = true;
         //     so.CustomResolvers = CompositeResolver.Create(
         //new IMessagePackFormatter[]
         //{
         //     DBNullFormatter.Instance, // This is necessary for the null values
         //        },
         //new IFormatterResolver[]
         //{
         //     NativeDateTimeResolver.Instance,
         //     ContractlessStandardResolver.Instance,
         //     StandardResolverAllowPrivate.Instance,
         //        });
         // },
         //   "Pack");
         //   });

         //   services.AddEFSecondLevelCache(options =>
         //      {
         //          options.UseEasyCachingCoreProvider(_providerName, isHybridCache: false).DisableLogging(false)
         //              .CacheQueriesContainingTypes(CacheExpirationMode.Sliding, TimeSpan.FromDays(2), TableTypeComparison.Contains, 
         //                  typeof(SalaryCycle), 
         //                  typeof(Level), 
         //                  typeof(AttributeGroup))
         //           //.SkipCachingCommands(commandText =>
         //           //  commandText.Contains("NEWID()", StringComparison.InvariantCultureIgnoreCase));
         //           .CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromDays(2));
         //      });


            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("Authentication:JWTSecretKey"))),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/userHub"))
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = async context =>
                        {
                            var uow = context.HttpContext.RequestServices.GetRequiredService<UnitOfWork>();
                            var email = context.Principal!.GetEmail();
                            var user = await uow.UserRepository.GetQuery().Where(u => u.EmailAddress == email).FirstOrDefaultAsync();
                            if (user == null || user.UserStatus != UserStatus.Available)
                            {
                                context.Fail("Account Is Disabled!");
                            }
                            context.Success();
                        }
                        };
                    });

            return services;
        }

        public class DBNullFormatter : IMessagePackFormatter<DBNull>
        {
            public static DBNullFormatter Instance = new();

            private DBNullFormatter()
            {
            }

            public void Serialize(ref MessagePackWriter writer, DBNull value, MessagePackSerializerOptions options)
            {
                writer.WriteNil();
            }

            public DBNull Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => DBNull.Value;
        }

    }
}