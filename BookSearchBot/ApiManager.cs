using BookSearchBot.Helper;
using BookSearchBot.Model;
using Newtonsoft.Json;

namespace BookSearchBot
{
    public class ApiManager
    {
        private readonly string hostName;
        public ApiManager(string hostName)
        {
            this.hostName = hostName;
        }
        
        public async Task<BookModel> GetBooksByNameAsync(string bookName)
        {
            Uri uri = new Uri($"{hostName}/{Constants.BOOK_NAME_SEARCH}/{bookName}");
            
            string jsonResponse = await GetResponseByUriAsync(uri);

            return JsonConvert.DeserializeObject<BookModel>(jsonResponse);
        }
       
        public async Task<BookModel> GetBooksByIsbnAsync(string isbn)
        {
            Uri uri = new Uri($"{hostName}/{Constants.BOOK_ISBN_SEARCH}/{isbn}");

            string jsonResponse = await GetResponseByUriAsync(uri);

            BookModel bookModel = JsonConvert.DeserializeObject<BookModel>(jsonResponse);
            if (bookModel.totalItems == 0)
            {
                bookModel.books.Add(new Book() { title = "Fail((("});
            }
            return bookModel;
        }
       
        public async Task<BookModel> GetBooksByAuthorAsync(string author)
        {
            Uri uri = new Uri($"{hostName}/{Constants.BOOK_AUTHOR_SEARCH}/{author}");

            string jsonResponse = await GetResponseByUriAsync(uri);

            return JsonConvert.DeserializeObject<BookModel>(jsonResponse);
        }
       
        public async Task<string> GetBookCoverByIsbnAsync(string isbn)
        {
            Uri uri = new Uri($"{hostName}/{Constants.BOOK_BOOKCOVER_SEARCH}/{isbn}");

            string jsonResponse = await GetResponseByUriAsync(uri);

            return JsonConvert.DeserializeObject<Uri>(jsonResponse).ToString();
        }
       
        public async Task<Book> GetRandomBookAsync()
        {
            Uri uri = new Uri($"{hostName}/{Constants.BOOK_RANDOMBOOK}");

            string jsonResponse = await GetResponseByUriAsync(uri);

            return JsonConvert.DeserializeObject<Book>(jsonResponse);
        }
        
        public async Task<string> GetAllBooksAsync(string tgid)
        {
            Uri uri = new Uri($"{hostName}/{Constants.GET_ALL_BOOKS}/{tgid}");

            string jsonResponse = await GetResponseByUriAsync(uri);

            return JsonConvert.DeserializeObject<string>(jsonResponse);
        }
        
        public async Task<bool> AddUserAsync(string tgid, string name)
        {
            try
            {
                Uri uri = new Uri($"{hostName}/{Constants.ADD_USER}?{Constants.TG_ID}={tgid}&{Constants.NAME}={name}");
                
                string jsonResponse = await MethodRequestByUriAsync(HttpMethod.Post, uri);

                var status = JsonConvert.DeserializeObject<StatusRequest>(jsonResponse);

                return status.Status == StatusRequest.OK;
            }
            catch   
            {
                
                return false;
            }
        }
        
        public async Task<bool> AddBookAsync(string tgid, string isbn)
        {
            try
            {
                Uri uri = new Uri($"{hostName}/{Constants.ADD_BOOK}?{Constants.TG_ID}={tgid}&{Constants.ISBN}={isbn}");

                string jsonResponse = await MethodRequestByUriAsync(HttpMethod.Post, uri);

                var status = JsonConvert.DeserializeObject<StatusRequest>(jsonResponse);

                return status.Status == StatusRequest.OK;
            }
            catch  
            {
                
                return false;
            }
        }

        public async Task<bool> DeleteBookAsync(string tgid, string isbn)
        {
            try
            {
                Uri uri = new Uri($"{hostName}/{Constants.DELETE_BOOK}?{Constants.TG_ID}={tgid}&{Constants.ISBN}={isbn}");
                
                string jsonResponse = await MethodRequestByUriAsync(HttpMethod.Delete, uri);

                var status = JsonConvert.DeserializeObject<StatusRequest>(jsonResponse);

                return status.Status == StatusRequest.OK;
            }
            catch   
            {
                return false;
            }
        }
        
        public async Task<bool> DeleteAllBookAsync(string tgid)
        {
            Uri uri = new Uri($"{hostName}/{Constants.DELETE_ALL_BOOKS}?{Constants.TG_ID}={tgid}");

            string jsonResponse = await MethodRequestByUriAsync(HttpMethod.Delete, uri);
            var status = JsonConvert.DeserializeObject<StatusRequest>(jsonResponse);

            return status.Status == StatusRequest.OK;
        }
        
        private async Task<string> MethodRequestByUriAsync(HttpMethod method, Uri uri)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(method, uri);

                using (var response = await client.SendAsync(httpRequestMessage))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

       
        private async Task<string> GetResponseByUriAsync(Uri uri)
        {
            string jsonResponse;
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(uri))
                {
                    jsonResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return jsonResponse;
        }
    }
}
