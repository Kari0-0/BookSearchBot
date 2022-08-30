using BookSearchBot.Helper;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace BookSearchBot
{
    public class BookSearchBook
    {
        //обьект бота
        private TelegramBotClient botClient;
        private CancellationTokenSource tokenSource;
        public BookSearchBook(string token) 
        {
            botClient = new TelegramBotClient(token);   
            tokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync() 
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            
            botClient.StartReceiving(
                updateHandler: UpdateAsync, 
                pollingErrorHandler: PollingErrorAsync,    
                receiverOptions: receiverOptions,
                cancellationToken: tokenSource.Token
                );
        }

        public async Task<string> TestConectionAsync()   
        {
            var result = await botClient.GetMeAsync();  
            string str = $"Bot {result.Username} is start!";
            Console.WriteLine(str);
            return str;
        }

        private async Task UpdateAsync(ITelegramBotClient botClient, 
                                       Update update,   
                                       CancellationToken cancellation)
        {
            if(update?.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackQueryAsync(botClient, update.CallbackQuery);
                return;
            }
            if(update?.Type == UpdateType.Message && update?.Message?.Type == MessageType.Photo)
            {
                var fileId = update.Message.Photo.Last().FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePth = fileInfo.FilePath;


                string result;
                using (FileStream fileStream = System.IO.File.Create(Constants.TEMP_PATH))
                {
                    await botClient.DownloadFileAsync(
                        filePath: filePth,
                        destination: fileStream);

                    result = BarcodeFileReader.Read(fileStream);
                }

                await botClient.SendTextMessageAsync(update.Message.From.Id, $"ISBN: {result}");

                return;
            }
            if(update?.Type == UpdateType.Message)
            {
                await HandlerMessageAsync(update);
                return;
            }

            
        }

        
        private Task PollingErrorAsync(ITelegramBotClient botClient, 
                                             Exception  exception, 
                                             CancellationToken cancellation)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }

        #region Message Type
        public async Task HandlerMessageAsync(Update update)
        {
            string msg = update.Message.Text;   
            switch (msg)   
            {
                case Constants.START:
                    await StartAnswerAsync(update.Message.Chat.Id);
                    break;
                case Constants.SEARCH_BY_AUTHOR:

                    break;
                case Constants.SEARCH_BY_NAME:

                    break;
                case Constants.SEARCH_BY_ISBN:
                    await SearchByISBNAnswerAsync(update.Message.Chat.Id);
                    break;
                case Constants.MANAGE_ARCHIVE:

                    break;
                case Constants.GET_A_RANDOM_BOOK:

                    break;
                default:
                    await DefaultAnswerAsync(update.Message.Chat.Id);
                    break;
            }
        }

        private async Task HandlerCallbackQueryAsync(ITelegramBotClient botClient,
                                                   CallbackQuery callbackQuery)
        {


            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: callbackQuery.Data
                );
        }
        #endregion

        #region ANSWERS
        private async Task DefaultAnswerAsync(ChatId ID )
        {
           await botClient.SendTextMessageAsync(ID, "I don`t understand  you");
        }
        private async Task StartAnswerAsync(ChatId ID)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new KeyboardButton[][]{
                new KeyboardButton[] { Constants.SEARCH_BY_AUTHOR,Constants.SEARCH_BY_NAME },
                new KeyboardButton[] { Constants.SEARCH_BY_ISBN },
                new KeyboardButton[] { Constants.MANAGE_ARCHIVE},
                new KeyboardButton[] { Constants.GET_A_RANDOM_BOOK }
            }
                )
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: ID,
                text: "Choose a response",
                replyMarkup: replyKeyboardMarkup
                );
        }
        private async Task SearchByISBNAnswerAsync(ChatId ID)
        {
            InlineKeyboardMarkup inlineKeyboardBookCover = new InlineKeyboardMarkup( new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Input number ISBN", "bookCover/InputNum"),
                    InlineKeyboardButton.WithCallbackData("Input picture ISBN ", "bookCover/PictureNum")
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: ID,
                text: "You want to find book COVER by:",
                replyMarkup:inlineKeyboardBookCover
                );

            InlineKeyboardMarkup inlineKeyboardBookInfo = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Input number ISBN", "bookInfo/InputNum"),
                    InlineKeyboardButton.WithCallbackData("Input picture ISBN ", "bookInfo/PictureNum")
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: ID,
                text: "You want to find book INFO by:",
                replyMarkup: inlineKeyboardBookInfo
                );

        }
        #endregion

    }
}


