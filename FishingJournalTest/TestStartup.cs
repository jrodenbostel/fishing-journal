using System;
using FishingJournal;
using FishingJournal.Data;
using FishingJournal.Models;
using FishingJournal.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FishJournalTest
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            const string connectionString = "DataSource=:memory:";
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            services.AddDbContext<DefaultContext>(options =>
                options.UseSqlite(connection));

            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlite(connection));

            services.AddIdentity<User, IdentityRole>(config => { config.SignIn.RequireConfirmedEmail = false; })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, MockEmailSender>();

            services.AddAuthorization();
        }
    }

    public class MockEmailSender : EmailSender
    {
        public MockEmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor) : base(optionsAccessor)
        {
        }
    }
}