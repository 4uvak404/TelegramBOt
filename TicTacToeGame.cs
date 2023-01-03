using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace TelegramBOt
{
    internal class TicTacToeGame
    {
        [Key]
        public Message Message { get; set; }
        public User Player1 { get; set; }
        public User? Player2 { get; set; }
        public int CurrentPlayer { get; set; } = 0;
        public bool? FlipWinner { get; set; }
        public bool Going { get; set; } = false;
        public TicTacToeMap Map { get; set; }
    }
}
