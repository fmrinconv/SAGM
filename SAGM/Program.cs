using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SAGM.Data;
using SAGM.Data.Entities;
using SAGM.Helpers;
using System.Text.Json.Serialization;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<SAGMContext>(o =>
        {
            o.UseSqlServer(builder.Configuration.GetConnectionString("AzureConnection"), sqlServerOptionsAction =>sqlServerOptionsAction.CommandTimeout(300));
            //o.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection"));
            //o.UseSqlServer(builder.Configuration.GetConnectionString("SomeeConnection"));
            
        });

        builder.Services.AddIdentity<User, IdentityRole>(cfg =>
        {
            cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            cfg.SignIn.RequireConfirmedEmail = true;
            cfg.User.RequireUniqueEmail = true;
            cfg.Password.RequireDigit = false;
            cfg.Password.RequiredUniqueChars = 0;
            cfg.Password.RequireLowercase = false;
            cfg.Password.RequireUppercase = false;
            cfg.Password.RequireNonAlphanumeric = false;
            cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            cfg.Lockout.MaxFailedAccessAttempts = 3;
            cfg.Lockout.AllowedForNewUsers = true;
        })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<SAGMContext>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/NotAuthorized";
            options.AccessDeniedPath = "/Account/NotAuthorized";
        });


        //Inyeccion para la inicialización de la base de datos
        builder.Services.AddTransient<SeedDb>();
        builder.Services.AddScoped<IUserHelper, UserHelper>();
        builder.Services.AddScoped<IComboHelper, ComboHelper>();
        builder.Services.AddScoped<IBlobHelper, BlobHelper>();
        builder.Services.AddScoped<IMailHelper, MailHelper>();
        builder.Services.AddScoped<IReportHelper, ReportHelper>();
        builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        
        var app = builder.Build();

        SeedData();

        void SeedData()
        {
            IServiceScopeFactory? scopedFactory = app.Services.GetService<IServiceScopeFactory>();
            using (IServiceScope? scope = scopedFactory.CreateScope())
            {
                SeedDb? service = scope.ServiceProvider.GetService<SeedDb>();
                service.SeedAsync().Wait();
            }
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/error/{0}");
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}