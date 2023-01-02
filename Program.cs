using System.Data.Common;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.UpdatingMessages;

namespace TelegramBOt
{
    internal class Program
    {
        private readonly static string botToken = "5911163182:AAGCzUiymm5wDjMcRi3-UdTTzBv5QX_0nz0";
        private static BotClient bot;
        private static User player1, player2;
        private static int currentPlayer;
        private static bool gameGoing = false;
        private static Message gameMessage;
        private static TicTacToeMap map;
        private static bool flipWinner;

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
                                    if (!gameGoing)
                                    {
                                        gameMessage = bot.SendMessage(chatId, "@" + sender.Username.ToString() + " вызывает на дуэль в крестики-нолики", replyMarkup: ticTacToeInvite);
                                        if (gameMessage != null)
                                        {
                                            player1 = sender;
                                            map = new TicTacToeMap(12, 8, 3);
                                        }
                                    }
                                    else
                                    {
                                        bot.SendMessageAsync(chatId, "Игра уже идёт, я не могу за двумя играми сразу следить");
                                    }
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

                                if (gameGoing && query.Message.MessageId == gameMessage.MessageId)
                                {
                                    int row = int.Parse(query.Data.Split(',')[0]);
                                    int column = int.Parse(query.Data.Split(',')[1]);
                                    if (currentPlayer == 0)
                                    {
                                        if(sender == player1)
                                        {
                                            currentPlayer = 1;
                                            flipWinner = false;
                                        }
                                        else if (sender == player2)
                                        {
                                            currentPlayer = 2;
                                            flipWinner = true;
                                        }
                                    }
                                    if (currentPlayer == 1 && sender == player1)
                                    {
                                        if (!MakeTurn(row, column))
                                        {
                                            bot.AnswerCallbackQuery(query.Id);
                                        }
                                    }
                                    else if (currentPlayer == 2 && sender == player2)
                                    {
                                        if (!MakeTurn(row, column))
                                        {
                                            bot.AnswerCallbackQuery(query.Id);
                                        }
                                    }
                                    else
                                    {
                                        bot.AnswerCallbackQuery(query.Id, "Сейчас не ваш ход");
                                    }
                                    
                                }
                                if (query.Data == "accept_game")
                                {
                                    if (!gameGoing)
                                    {
                                        StartGame(query.From);
                                    }
                                    else
                                    {
                                        bot.AnswerCallbackQueryAsync(query.Id, "Игра уже началась");
                                    }
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
        private static void StartGame(User playerTwo)
        {
            gameGoing = true;
            currentPlayer = 0;
            player2 = playerTwo;
            bot.EditMessageText(new EditMessageTextArgs
            {
                ChatId = gameMessage.Chat.Id,
                MessageId = gameMessage.MessageId,
                Text = $"@{player1.Username} против @{player2.Username} сейчас ход {currentPlayer}",
                ReplyMarkup = MakeTictactoeKeyboardMarkup(map.Values)
            });
        }
        private static bool MakeTurn(int row, int column)
        {
            if (!map.SetElement(row, column))
            {
                return false;
            }
            int result = map.CheckWin();

            if (!(result >= -1 && result <= 2))
            {
                return true;
            }

            if (result == 0)
            {
                currentPlayer = FlipPlayer(currentPlayer);
                bot.EditMessageText(new EditMessageTextArgs
                {
                    ChatId = gameMessage.Chat.Id,
                    MessageId = gameMessage.MessageId,
                    Text = $"@{player1.Username} против @{player2.Username} сейчас ход {currentPlayer}",
                    ReplyMarkup = MakeTictactoeKeyboardMarkup(map.Values)
                });
                return true;
            }
            if (flipWinner)
            {
                result = FlipPlayer(result);
            }
            EndGame(result);
            return true;
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
                    db.Add(newGame);
                    db.SaveChanges();
                }
            }
        }
        
        private static void EndGame(int winner)
        {
            string winText = "default value";
            if (winner == -1) winText = "Ничья, никто не подбедил";
            if (winner == 1) winText = $"Победил @{player1.Username} ({winner})";
            if (winner == 2) winText = $"Победил @{player2.Username} ({winner})";
            bot.EditMessageText(new EditMessageTextArgs
            {
                ChatId = gameMessage.Chat.Id,
                MessageId = gameMessage.MessageId,
                Text = winText,
                ReplyMarkup = MakeTictactoeKeyboardMarkup(map.Values)
            });
            gameGoing = false;
            currentPlayer = 0;
            player1 = null;
            player2 = null;
        }
        private static int FlipPlayer(int player)
        {
            if (player == 1) return 2;
            else if (player == 2) return 1;
            else return player;
        }
    }
}