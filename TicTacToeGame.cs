using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.BotAPI.AvailableTypes;

namespace TelegramBOt
{
    [PrimaryKey(nameof(ChatId), nameof(MessageId))]
    internal class TicTacToeGame
    {
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public long Player1Id { get; set; }
        public string Player1Username { get; set; }
        public long? Player2Id { get; set; }
        public string? Player2Username { get; set; }
        public int CurrentPlayer { get; set; } = 0;
        public bool? FlipWinner { get; set; }
        public bool Going { get; set; } = false;
        public int TicTacToeMapId { get; set; }
        public TicTacToeMap Map { get; set; }
        public static int FlipPlayer(int player)
        {
            if (player == 1) return 2;
            else if (player == 2) return 1;
            else return player;
        }
    }
}
