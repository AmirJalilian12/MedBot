using MedBot.Data;
using MedBot.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedBot.Utilities
{
    public class DataInitializer
    {
        internal static void Initialize(MedCodeContext context , IConfiguration configuration) 
        {
            context.Database.Migrate();
            InitData(context, configuration);
        }
        private static void InitData(MedCodeContext context , IConfiguration configuration)
        {
            if(!context.Users.Any(x=> x.UserType == UserType.Admin))
            {
                var user = configuration.GetSection("AdminInfo").Get<User>();
                if(user != null)
                {
                    user.UserType = UserType.Admin;
                    context.Users.Add(user);
                    context.SaveChanges();
                }
            }
        }
    }
}
