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
        public void EndGame(TicTacToeGame game, int winner)
        {
            string winText = "default value";
            if (winner == -1) winText = "Ничья, никто не победил";
            if (winner == 1) winText = $"Победил @{game.Player1Username} ({winner})";
            if (winner == 2) winText = $"Победил @{game.Player2Username} ({winner})";
            game.Going = false;
            game.CurrentPlayer = 0;
        }
        public bool MakeTurn(int row, int column, TicTacToeGame game)
        {
            if (!game.Map.SetElement(row, column))
            {
                return false;
            }
            int result = game.Map.CheckWin();

            if (!(result >= -1 && result <= 2))
            {
                return false;
            }

            if (result == 0)
            {
                game.CurrentPlayer = FlipPlayer(game.CurrentPlayer);
                return true;
            }
            if ((bool)game.FlipWinner!)
            {
                result = FlipPlayer(result);
            }
            EndGame(game, result);
            return true;
        }
        public static int FlipPlayer(int player)
        {
            if (player == 1) return 2;
            else if (player == 2) return 1;
            else return player;
        }
    }
}
