using MedBot.Data;
using MedBot.Entities;
using MedBot.Repositories;
using MedBot.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.AspNetCore;

namespace MedBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. ثبت سرویس‌ها
            builder.Services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

            builder.Services.AddDbContext<MedCodeContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnectoin"));
            });

            var botToken = "8235803348:AAEtW00KnpNuDUqP8JMkJaRsH5MlYgl_arY";

            // ۲. یک نمونه از کلاینت بسازید
            var botClient = new TelegramBotClient(botToken);

            // ۳. هر دو حالت را در سرویس‌ها ثبت کنید (این کلید حل مشکل شماست)
            builder.Services.AddSingleton<ITelegramBotClient>(botClient);
            builder.Services.AddSingleton(botClient);

            // ثبت Repository ها
            builder.Services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
            builder.Services.AddScoped<IBaseRepository<News>, BaseRepository<News>>();
            builder.Services.AddScoped<IBaseRepository<NewsKeyWord>, BaseRepository<NewsKeyWord>>();
            builder.Services.AddScoped<IBaseRepository<KeyWord>, BaseRepository<KeyWord>>();
            builder.Services.AddScoped<IBaseRepository<NewsUserCollection>, BaseRepository<NewsUserCollection>>();
            builder.Services.AddScoped<IBaseRepository<UserActivity>, BaseRepository<UserActivity>>();
            builder.Services.AddControllers();
            builder.Services.ConfigureTelegramBotMvc();
            builder.Services.AddSwaggerGen();

            // این خط را هم برای حل مشکل ModelValidation (ارور 400) که قبلاً داشتیم اضافه کنید
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            var app = builder.Build();

            // 2. مقداردهی اولیه دیتابیس
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<MedCodeContext>();
                    DataInitializer.Initialize(context, app.Configuration);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error at {DateTime.Now}: {ex.Message} \n {ex.InnerException?.Message}";
                    File.AppendAllText("database_log.txt", errorMessage);
                }
            }

            // 3. تنظیمات خط لوله (نمایش Swagger در هاست)
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseRouting();
            app.MapControllers();

            app.Run();
        }
    }
}