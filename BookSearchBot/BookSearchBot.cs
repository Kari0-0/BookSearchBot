using BookSearchBot.Helper;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;
using System;
using System.Linq;

namespace BookSearchBot
{
    public class BookSearchBot
    {
        private readonly TelegramBotClient botClient;   
        private CancellationTokenSource tokenSource;    
        private readonly ApiManager apiManager;         
        Dictionary<string, string> action = new();      
                                                        
         
        public BookSearchBot(string token, string apiHost) 
        {
            botClient = new TelegramBotClient(token);  
            tokenSource = new CancellationTokenSource();
            apiManager = new ApiManager(apiHost);
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
                cancellationToken: tokenSource.Token);  
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
            
            var id = update?.Message?.From?.Id;    

            if (update?.Type == UpdateType.CallbackQuery)   
            {
                      await HandlerCallbackQueryAsync(botClient, update.CallbackQuery, update);

                return;
            }

            if(id == null) return;                  


            if (!action.ContainsKey(id.ToString()))  
            {
               action.Add(id.ToString(), string.Empty);
            }

            
            if (update?.Type == UpdateType.Message && !string.IsNullOrEmpty(action[id.ToString()]))
            {               
                await ActionHandler(update);
                return;
            }

            
            if (update?.Type == UpdateType.Message)
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

        #region Handler Message
        
        public async Task HandlerMessageAsync(Update update)
        {
            string msg = update.Message.Text;   
            switch (msg)  
            {
                case Constants.START:   
                    await StartAnswerAsync(update.Message.Chat);
                    break;
                case Constants.SEARCH_BY_AUTHOR:    
                    await SearchByAuthorAnswerAsync(update.Message.Chat);
                    break;
                case Constants.SEARCH_BY_NAME:      
                    await SearchByNameAnswerAsync(update.Message.Chat);
                    break;
                case Constants.SEARCH_BY_ISBN:      
                    await SearchByISBNAnswerAsync(update.Message.Chat.Id);
                    break;
                case Constants.MANAGE_ARCHIVE:      
                    await ManageArchiveAnswerAsync(update.Message.Chat.Id);
                    break;
                case Constants.GET_A_RANDOM_BOOK:   
                    await GetRandomBookAsync(update.Message.Chat.Id);
                    break;
                default:                            
                    await DefaultAnswerAsync(update.Message.Chat.Id);
                    break;
            }
        }

        
        private async Task HandlerCallbackQueryAsync(ITelegramBotClient botClient,
                                                     CallbackQuery callbackQuery,
                                                     Update update)
        {
            ChatId id = callbackQuery.From.Id;

            switch (callbackQuery.Data)
            {
                case Constants.BOOKCOVER_INPUTNUMBER:   
                    await SetActionAndSendTextMessage(id, Constants.BOOKCOVER_INPUTNUMBER, "Enter Num:");
                    break;
                case Constants.BOOKCOVER_PICTURENUMBER: 
                    await SetActionAndSendTextMessage(id, Constants.BOOKCOVER_PICTURENUMBER, "Enter Picture:");
                    break;
                case Constants.BOOKINFO_INPUTNUM:       
                    await SetActionAndSendTextMessage(id, Constants.BOOKINFO_INPUTNUM, "Enter Num:");
                    break;
                case Constants.BOOKINFO_PICTURENUM:     
                    await SetActionAndSendTextMessage(id, Constants.BOOKINFO_PICTURENUM, "Enter Picture:");
                    break;
                case Constants.ADD_BOOK:                
                    await SetActionAndSendTextMessage(id, Constants.ADD_BOOK, "Enter Num:");
                    break;
                case Constants.DELETE_BOOK:            
                    await SetActionAndSendTextMessage(id, Constants.DELETE_BOOK, "Enter Num:");
                    break;
                case Constants.GET_ALL_BOOKS:           
                    await ActionGetAllBook(update);
                    break;
                case Constants.DELETE_ALL_BOOKS:        
                    await ActionDeleteAllBook(update);
                    break;
            }
        }
        #endregion

        #region ANSWERS
        
        private async Task DefaultAnswerAsync(ChatId ID )
        {
           await botClient.SendTextMessageAsync(ID, "I don`t understand  you");
        }

        
        private async Task StartAnswerAsync(Chat chat)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new KeyboardButton[][]{
                new KeyboardButton[] { Constants.SEARCH_BY_AUTHOR,Constants.SEARCH_BY_NAME },
                new KeyboardButton[] { Constants.SEARCH_BY_ISBN },
                new KeyboardButton[] { Constants.MANAGE_ARCHIVE},
                new KeyboardButton[] { Constants.GET_A_RANDOM_BOOK }
            })
            {
                ResizeKeyboard = true
            };

            if (await apiManager.AddUserAsync(chat.Id.ToString(), chat?.FirstName ?? "Nan"))
            {
                await botClient.SendTextMessageAsync(
               chatId: chat.Id,
               text: "Thanks for choose!");
            }

            await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: "Choose a response",
                replyMarkup: replyKeyboardMarkup);

        }

       
        private async Task SearchByAuthorAnswerAsync(Chat chat)
        {
            await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: "Enter Author:");

            action[chat.Id.ToString()] = Constants.SEARCH_BY_AUTHOR;
        }
      
        private async Task SearchByNameAnswerAsync(Chat chat)
        {
            await botClient.SendTextMessageAsync(
                chatId: chat.Id,
                text: "Enter Name:");

            action[chat.Id.ToString()] = Constants.SEARCH_BY_NAME;
        }
       
        private async Task GetRandomBookAsync(ChatId id)
        {
            
            await botClient.SendTextMessageAsync(
                chatId: id,
                text: "Random book:");

            var book = await apiManager.GetRandomBookAsync();
            if (book == null) return;

            await botClient.SendTextMessageAsync(
                chatId: id,
                text: book.ToString());
        }
        
        private async Task SearchByISBNAnswerAsync(ChatId ID)
        {
            
            InlineKeyboardMarkup inlineKeyboardBookCover = new InlineKeyboardMarkup( new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Input number ISBN", Constants.BOOKCOVER_INPUTNUMBER),
                    InlineKeyboardButton.WithCallbackData("Input picture ISBN ", Constants.BOOKCOVER_PICTURENUMBER)
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
                    InlineKeyboardButton.WithCallbackData("Input number ISBN", Constants.BOOKINFO_INPUTNUM),
                    InlineKeyboardButton.WithCallbackData("Input picture ISBN ", Constants.BOOKCOVER_PICTURENUMBER)
                }
            });

            
            await botClient.SendTextMessageAsync(
                chatId: ID,
                text: "You want to find book INFO by:",
                replyMarkup: inlineKeyboardBookInfo);

            
            action[ID.ToString()] = Constants.SEARCH_BY_ISBN;
        }

        
        private async Task ManageArchiveAnswerAsync(ChatId id)
        {
            
            InlineKeyboardMarkup inlineKeyboardBookCover = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Add book", Constants.ADD_BOOK),
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Delete book", Constants.DELETE_BOOK),
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Delete all book", Constants.DELETE_ALL_BOOKS),
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Get my book", Constants.GET_ALL_BOOKS),
                }
            });

            
            await botClient.SendTextMessageAsync(
                chatId: id,
                text: "Chooce the action:",
                replyMarkup: inlineKeyboardBookCover);
        }
        #endregion

        #region ACTIONS Handler
       
        private async Task ActionHandler(Update update)
        {
            switch (action[update?.Message?.Chat?.Id.ToString()])
            {
                case Constants.SEARCH_BY_AUTHOR:
                    await ActionSearchByAuthor(update);
                    break;
                case Constants.SEARCH_BY_NAME:
                    await ActionSearchByName(update);
                    break;
                case Constants.BOOKCOVER_INPUTNUMBER:
                    await ActionBookCoverByNum(update);
                    break;
                case Constants.BOOKCOVER_PICTURENUMBER:
                    await ActionBookCoverByPicture(update);
                    break;
                case Constants.BOOKINFO_INPUTNUM:
                    await ActionBookInfoByNum(update);
                    break;
                case Constants.BOOKINFO_PICTURENUM:
                    await ActionBookInfoByPicture(update);
                    break;
                case Constants.ADD_BOOK:
                    await ActionAddBook(update);
                    break;
                case Constants.DELETE_BOOK:
                    await ActionDeleteBook(update);
                    break;
                default:
                    break;
            }            
            action[update.Message.Chat.Id.ToString()] = String.Empty;
        }

       
        private async Task ActionAddBook(Update update)
        {
            if (update?.Message?.Type != MessageType.Text)
            {
                return;
            }
            var books = await apiManager.AddBookAsync(update.Message.Chat.Id.ToString(), update.Message.Text);

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Book added!");

        }
       
        private async Task ActionDeleteBook(Update update)
        {
            if (update?.Message?.Type != MessageType.Text)
            {
                return;
            }
            var books = await apiManager.DeleteBookAsync(update.Message.Chat.Id.ToString(), update.Message.Text);

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"ISBN: {update.Message.Text} is deleted");

        }
       
        private async Task ActionDeleteAllBook(Update update)
        {
            if (update?.Type != UpdateType.CallbackQuery)
            {
                return;
            }

            var isDeleted = await apiManager.DeleteAllBookAsync(update.CallbackQuery.From.Id.ToString());

            if (isDeleted == true)
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, $"You delete your book!");
            }
            else
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, $"Sorry, Error");
            }
        }
        
        private async Task ActionGetAllBook(Update update)
        {
            
            if (update?.Type != UpdateType.CallbackQuery)
            {
                return;
            }

            var books = await apiManager.GetAllBooksAsync(update.CallbackQuery.From.Id.ToString());

            await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, (String.IsNullOrEmpty(books))? "You don`t have any books!" : books);

        }
        
        private async Task ActionBookInfoByPicture(Update update)
        {
            if (update.Message.Type != MessageType.Photo)
            {
                return;
            }
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
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"ISBN: {result}");

            var books = await apiManager.GetBooksByIsbnAsync(result);
            StringBuilder sb = new StringBuilder();
            foreach (var book in books.books.Take(3))
            {
                if ((sb.ToString() + book.ToString()).Length > 4000)
                {
                    break;
                }
                sb.AppendLine(book.ToString());
                sb.AppendLine("* * * *");
            }

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: sb.ToString()
                );
        }
        
        private async Task ActionBookInfoByNum(Update update)
        {
            if (update?.Message?.Type != MessageType.Text)
            {
                return;
            }
            
            var isbn = update.Message.Text;
            var books = await apiManager.GetBooksByIsbnAsync(isbn);
            StringBuilder sb = new StringBuilder();
            foreach (var book in books.books.Take(3))
            {
                if ((sb.ToString() + book.ToString()).Length > 4000)
                {
                    break;
                }
                sb.AppendLine(book.ToString());
                sb.AppendLine("* * * *");
            }

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: sb.ToString());
        }
        
        private async Task ActionSearchByAuthor(Update update)
        {
            if (update?.Message?.Type != MessageType.Text)
            {
                return;
            }
            var author = update.Message.Text;
            var books = await apiManager.GetBooksByAuthorAsync(author);
            if (books == null) return;

            StringBuilder sb = new StringBuilder();
            foreach (var book in books.books.Take(3))
            {
                if ((sb.ToString() + book.ToString()).Length > 4000)
                {
                    break;
                }
                sb.AppendLine(book.ToString());
                sb.AppendLine("* * * *");
            }

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: sb.ToString()
                );
        }
       
        private async Task ActionSearchByName(Update update)
        {
            if (update?.Message?.Type != MessageType.Text)
            {
                return;
            }
            var name = update.Message.Text;
            var books = await apiManager.GetBooksByNameAsync(name);
            if (books == null) return;

            StringBuilder sb = new StringBuilder();
            foreach (var book in books.books.Take(3))
            {
                if ((sb.ToString() + book.ToString()).Length > 4000)
                {
                    break;
                }
                sb.AppendLine(book.ToString());
                sb.AppendLine("* * * *");
            }

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: sb.ToString()
                );
        }
        
        private async Task ActionBookCoverByNum(Update update)
        {
            if (update.Message.Type != MessageType.Text)
            {
                return;
            }

            var isbn = update.Message.Text;
            var imgLink = await apiManager.GetBookCoverByIsbnAsync(isbn);
            if (imgLink == null) return;

            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat.Id,
                photo: imgLink);
        }
       
        private async Task ActionBookCoverByPicture(Update update)
        {
            if(update.Message.Type!= MessageType.Photo)
            {
                return;
            }

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

            result = "9"+result.Substring(1);

            await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"ISBN: {result}");

            var imgLink = await apiManager.GetBookCoverByIsbnAsync(result);
            await botClient.SendPhotoAsync(
                chatId: update.Message.Chat.Id,
                photo: imgLink);
        }
        #endregion
        #region -- Private --
        private async Task SetActionAndSendTextMessage(ChatId id, string action, string msg)
        {
            this.action[id.ToString()] = action;
            await botClient.SendTextMessageAsync(
                chatId: id,
                text: msg);
        }
        #endregion
    }
}