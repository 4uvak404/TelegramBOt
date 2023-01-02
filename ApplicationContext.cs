using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBOt
{
    internal class ApplicationContext : DbContext
    {
        DbSet<TicTacToeGame> Games => Set<TicTacToeGame>();
        ApplicationContext() => Database.EnsureCreated();
        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=TelegramBotDB");
        }
    }
}
