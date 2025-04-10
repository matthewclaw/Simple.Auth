
using System.Security.Cryptography;

namespace Simple.Auth.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string dummySecret = "yily6zpSs4B4smwRlKtpxzbabSlq/A9fiHZdTxyQl2A=";
            builder.Services.AddSimpleAuth(options =>
            {
                options.UseCookies();
                options.WithConfiguration(builder.Configuration);
                options.WithDefaultTokenService(s => new Services.JwtTokenService(dummySecret, "me", "you"));
            });
            builder.Services.AddControllers();
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
