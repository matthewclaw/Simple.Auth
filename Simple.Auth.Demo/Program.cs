
using Simple.Auth.Controllers.Conventions;
using Simple.Auth.Demo.Services;
using Simple.Auth.Requirements;
using System.Security.Cryptography;

namespace Simple.Auth.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string dummySecret = "yily6zpSs4B4smwRlKtpxzbabSlq/A9fiHZdTxyQl2A=";
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSimpleAuthentication(options =>
            {
                options.DisableCaching();
                options.WithConfiguration(builder.Configuration);
                options.WithDefaultTokenService(s => new Auth.Services.JwtTokenService(dummySecret, "me", "you"));
                options.WithUserAuthenticator<UserAuthenticator>();
            });
            builder.Services.AddControllers(options =>
            {
                options.AddSimpleAuthControllers(x =>
                {
                    x.WithClassic();
                });
            });
            builder.Services.AddHttpContextAccessor();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
