namespace BookSearchBot.Model
{
    public class BookModel
    {
        public int totalItems { get => books.Count; }
        public List<Book> books { get; set; } = new List<Book>();

    }

    public class Book
    {
        public string id { get; set; }          = string.Empty;
        public string selfLink { get; set; }    = string.Empty;
        public string title { get; set; } = string.Empty;
        public string publisher { get; set; } = string.Empty;
        public string publishedDate { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int? pageCount { get; set; } 
        public List<string> categories { get; set; } = new List<string>();
        public string language { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Name: {title}\n" +
                   $"Categories: {string.Join(", ", categories)}\n" +
                   $"Description: {description}\n" +
                   $"Count of Page: {pageCount}";
        }
    }
}
