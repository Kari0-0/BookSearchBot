namespace BookSearchBot.Helper
{
    
    public static class Constants
    {
        //Menu
        public const string SEARCH_BY_AUTHOR = "🔍Search by Author";
        public const string SEARCH_BY_NAME = "Serach by Name🔎";
        public const string SEARCH_BY_ISBN = "🔍Search by ISBN🔎";
        public const string MANAGE_ARCHIVE = "Manage Archive❤️";
        public const string GET_A_RANDOM_BOOK = "Get a random Book🎲";
        public const string BOOKCOVER_INPUTNUMBER = "bookCover/InputNum";
        public const string BOOKCOVER_PICTURENUMBER = "bookCover/PictureNum";
        public const string BOOKINFO_INPUTNUM = "bookInfo/InputNum";
        public const string BOOKINFO_PICTURENUM = "bookInfo/PictureNum";


        //Command
        public const string START = "/start";

        //File Path
        public static string TEMP_PATH { get; } = "temp/temp.png";
        public static string CONFIG { get; } = "Settings/config.json";

        //Api Path
        public const string BOOK_NAME_SEARCH = "book_name_search";
        public const string BOOK_ISBN_SEARCH = "book_isbn_search";
        public const string BOOK_AUTHOR_SEARCH = "book_author_search";
        public const string BOOK_BOOKCOVER_SEARCH = "book_bookcover_search";
        public const string BOOK_RANDOMBOOK = "book_randombook";
        public const string ADD_USER = "addUser";
        public const string ADD_BOOK = "addBook";
        public const string DELETE_BOOK = "deleteBook";
        public const string DELETE_ALL_BOOKS = "deleteAllBooks";
        public const string GET_ALL_BOOKS = "getAllBooks";

        //Key
        public const string BOOK_NAME = "bookName";
        public const string ISBN = "isbn";
        public const string AUTHOR = "author";
        public const string TG_ID = "tgid";
        public const string NAME = "name";

        //Confi key
        public const string HOST = "host";
        public const string HOME_HOST = "HomeHost";
        public const string TELEGRAM_TOKEN = "telegram_token";

    }
}
