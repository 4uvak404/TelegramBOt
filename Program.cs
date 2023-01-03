using System.Data.Common;
using System.Reflection;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Games;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TelegramBOt
{
    internal class Program
    {
        private readonly static string botToken = "5911163182:AAGCzUiymm5wDjMcRi3-UdTTzBv5QX_0nz0";
        private static BotClient bot;

        private static InlineKeyboardMarkup ticTacToeInvite = new InlineKeyboardMarkup
        {
            InlineKeyboard = new InlineKeyboardButton[][]{
                new InlineKeyboardButton[]{
                InlineKeyboardButton.SetCallbackData("Сразиться⚔️", "accept_game"),
                }
            }
        };
        private static InlineKeyboardMarkup ticTacToeGame = new InlineKeyboardMarkup
        {
            InlineKeyboard = new InlineKeyboardButton[][]{
                new InlineKeyboardButton[]{
                InlineKeyboardButton.SetCallbackData("Callback1", "1"),
                    InlineKeyboardButton.SetCallbackData("Callback1", "2"),
                    InlineKeyboardButton.SetCallbackData("Callback1", "3")
                },
                new InlineKeyboardButton[]{
                InlineKeyboardButton.SetCallbackData("Callback1", "4"),
                    InlineKeyboardButton.SetCallbackData("Callback1", "5"),
                    InlineKeyboardButton.SetCallbackData("Callback1", "6")
                },
                new InlineKeyboardButton[]{
                InlineKeyboardButton.SetCallbackData("Callback1", "7"),
                    InlineKeyboardButton.SetCallbackData("Callback1", "8"),
                    InlineKeyboardButton.SetCallbackData("Callback1", "9")
                }
            }
        };

        static void Main(string[] args)
        { 
            bot = new BotClient(botToken);
            Console.WriteLine("Start!");
            var updates = bot.GetUpdates();
            bot.SetMyCommands(new BotCommand("tictactoe", "Сыграть в крестики-нолики"));
            while (true)
            {
                if (updates.Any())
                {
                    foreach(var update in updates )
                    {
                        User sender;
                        switch (update.Type)
                        {
                            case UpdateType.Message:
                                var message = update.Message;
                                if (message.Text == null)
                                {
                                    break;
                                }
                                long chatId = message.Chat.Id;
                                sender = message.From;
                                
                                Console.WriteLine($"Прислали сообщение с текстом {message.Text}");

                                if (message.Text.Contains("/tictactoe"))
                                {
                                    CreateNewGame(message);
                                }
                                else if (message.Text.Contains("/command"))
                                {
                                    //var markup1 = MakeTictactoeKeyboardMarkup(new int[,] { { 0, 0, 0 }, { 1, 1, 1 }, { 2, 2, 2 } });
                                }
                                //bot.SendMessageAsync(chatId, $"Ты зачем мне это прислал?", replyToMessageId: message.MessageId);
                                break;

                            case UpdateType.CallbackQuery:
                                var query = update.CallbackQuery;
                                sender = query.From;

                                Console.WriteLine($"Прислали ответ: {query.Data}");

                                using (ApplicationContext db = new ApplicationContext())
                                {
                                    TicTacToeGame? game = db.TicTacToeGames.Find(query.Message);
                                    if (game == null)
                                    {
                                        bot.AnswerCallbackQuery(query.Id, "Игра не найдена");
                                         break;
                                    }
                                    if (!game.Going)
                                    {
                                        bot.AnswerCallbackQuery(query.Id, "Игра не активна");
                                        break;
                                    }
                                    else
                                    {
                                        int row = int.Parse(query.Data.Split(',')[0]);
                                        int column = int.Parse(query.Data.Split(',')[1]);
                                        if (game.CurrentPlayer == 0)
                                        {
                                            if (sender == game.Player1)
                                            {
                                                game.CurrentPlayer = 1;
                                                game.FlipWinner = false;
                                            }
                                            else if (sender == game.Player2)
                                            {
                                                game.CurrentPlayer = 2;
                                                game.FlipWinner = true;
                                            }
                                            db.TicTacToeGames.Update(game);
                                            db.SaveChanges();
                                        }
                                        if (game.CurrentPlayer == 1 && sender == game.Player1 || game.CurrentPlayer == 2 && sender == game.Player2)
                                        {
                                            game = MakeTurn(row, column, game);
                                            if (game != null)
                                            {
                                                db.TicTacToeGames.Update(game);
                                                db.SaveChanges();
                                            }
                                            else
                                            {
                                                bot.AnswerCallbackQuery(query.Id);
                                            }
                                            
                                        }
                                        else
                                        {
                                            bot.AnswerCallbackQuery(query.Id, "Сейчас не ваш ход");
                                        }
                                    }
                                }
                                if (query.Data == "accept_game")
                                {
                                    StartGame(query);
                                }
                                break;
                        }
                    }
                    var offset = updates.Last().UpdateId + 1;
                    updates = bot.GetUpdates(offset);
                }
                else
                {
                    updates = bot.GetUpdates();
                }
            }
        }
        private static InlineKeyboardMarkup MakeTictactoeKeyboardMarkup(int[,] values)
        {
            int height = values.GetLength(0);
            int width = values.GetLength(1);
            var markupButtons = new InlineKeyboardButton[height][];
            for (int i = 0; i < height; i++)
            {
                markupButtons[i] = new InlineKeyboardButton[width];
                for (int j = 0; j < width; j++)
                {
                    string buttonText = "Default button value";
                    switch (values[i, j])
                    {
                        case 0:
                            buttonText = "  ";
                            break;
                        case 1:
                            buttonText = "❌";
                            break;
                        case 2:
                            buttonText = "⭕";
                            break;
                    }
                    markupButtons[i][j] = InlineKeyboardButton.SetCallbackData(buttonText, $"{i},{j}");
                }
            }
            return new InlineKeyboardMarkup(markupButtons);
        }
        private static bool StartGame(CallbackQuery query)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                TicTacToeGame? game = db.TicTacToeGames.Find(query.Message);
                if (game == null)
                {
                    bot.AnswerCallbackQuery(query.Id, "Игра не найдена");
                    return false;
                }
                if (game.Going)
                {
                    bot.AnswerCallbackQuery(query.Id, "Игра уже началась");
                    return false;
                }
                else
                {
                    game.Going = true;
                    game.CurrentPlayer = 0;
                    game.Player2 = query.From;
                    bot.EditMessageText(new EditMessageTextArgs
                    {
                        ChatId = game.Message.Chat.Id,
                        MessageId = game.Message.MessageId,
                        Text = $"@{game.Player1.Username} против @{game.Player2.Username} сейчас ход {game.CurrentPlayer}",
                        ReplyMarkup = MakeTictactoeKeyboardMarkup(game.Map.Values)
                    });
                    db.TicTacToeGames.Update(game);
                    db.SaveChanges();
                    return true;
                }
            }
        }
        //private static TicTacToeGame? GetGameFromQuery(CallbackQuery query, ApplicationContext db)
        //{
        //    TicTacToeGame? game = db.TicTacToeGames.Find(query.Message);
        //    if (game == null)
        //    {
        //        bot.AnswerCallbackQuery(query.Id, "Игра не найдена");
        //        return null;
        //    }
        //    else if (!game.Going)
        //    {

        //    }
        //}
        private static TicTacToeGame? MakeTurn(int row, int column, TicTacToeGame game)
        {
            if (!game.Map.SetElement(row, column))
            {
                return null;
            }
            int result = game.Map.CheckWin();

            if (!(result >= -1 && result <= 2))
            {
                return null;
            }

            if (result == 0)
            {
                game.CurrentPlayer = FlipPlayer(game.CurrentPlayer);
                bot.EditMessageText(new EditMessageTextArgs
                {
                    ChatId = game.Message.Chat.Id,
                    MessageId = game.Message.MessageId,
                    Text = $"@{game.Player1.Username} против @{game.Player2.Username} сейчас ход {game.CurrentPlayer}",
                    ReplyMarkup = MakeTictactoeKeyboardMarkup(game.Map.Values)
                });
                return game;
            }
            if ((bool)game.FlipWinner)
            {
                result = FlipPlayer(result);
            }
            game = EndGame(game, result);
            return game;
            
        }
        private static void CreateNewGame(Message message)
        {
            Message gameMessage = bot.SendMessage(message.Chat.Id, "@" + message.From.Username.ToString() + " вызывает на дуэль в крестики-нолики", replyMarkup: ticTacToeInvite);
            if (gameMessage != null)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    TicTacToeGame newGame = new TicTacToeGame()
                    {
                        Message = message,
                        Player1 = message.From,
                        Map = new TicTacToeMap(3, 3, 3)
                    };
                    db.TicTacToeGames.Add(newGame);
                    db.SaveChanges();
                }
            }
        }

        private static TicTacToeGame EndGame(TicTacToeGame game, int winner)
        {
            string winText = "default value";
            if (winner == -1) winText = "Ничья, никто не подбедил";
            if (winner == 1) winText = $"Победил @{game.Player1.Username} ({winner})";
            if (winner == 2) winText = $"Победил @{game.Player2.Username} ({winner})";
            bot.EditMessageText(new EditMessageTextArgs
            {
                ChatId = game.Message.Chat.Id,
                MessageId = game.Message.MessageId,
                Text = winText,
                ReplyMarkup = MakeTictactoeKeyboardMarkup(game.Map.Values)
            });
            game.Going = false;
            game.CurrentPlayer = 0;
            return game;
        }
        private static int FlipPlayer(int player)
        {
            if (player == 1) return 2;
            else if (player == 2) return 1;
            else return player;
        }
    }
}