using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<DataContext>(x => x.UseSqlServer(context.Configuration.GetConnectionString("Azure")));

        services.AddDefaultIdentity<UserEntity>(x =>
        {
            x.SignIn.RequireConfirmedAccount = false;
            x.SignIn.RequireConfirmedEmail = false;
            x.SignIn.RequireConfirmedPhoneNumber = false;
            x.User.RequireUniqueEmail = true;
            //x.Password.RequiredLength = 8;
        }).AddEntityFrameworkStores<DataContext>();

        services.AddAuthentication();
        services.AddAuthorization();
    })
    .Build();

host.Run();
