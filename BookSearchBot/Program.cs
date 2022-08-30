using BookSearchBot;   
using BookSearchBot.Helper;


Directory.CreateDirectory("temp");

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(Constants.CONFIG);

var app = builder.Build();

var token = app.Configuration[Constants.TELEGRAM_TOKEN];
var apiHost = app.Configuration[Constants.HOST];
try
{
    BookSearchBot.BookSearchBot bot = new BookSearchBot.BookSearchBot(token, apiHost);
    await bot.StartAsync();                              
    var testConnection = await bot.TestConectionAsync();

    app.MapGet("/", () => testConnection);  
}catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}
catch
{
    Console.WriteLine("Error");
}
app.Run();  
