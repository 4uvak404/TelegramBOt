using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace TelegramBOt
{
    internal class TicTacToeGame
    {
        public int Id { get; set; }
        public User Player1 { get; set; }
        public User Player2 { get; set; }
        public int CurrentPlayer { get; set; }
        public bool FlipWinner { get; set; }
        public bool Going { get; set; } = true;
        public Message Message { get; set; }
        public TicTacToeMap Map;
    }
}
