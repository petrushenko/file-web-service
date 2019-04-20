namespace WebService
{
    class Program
    {
        private static void Main()
        {

            HttpServer server = new HttpServer(8080, @"D:\bsuir\C#\projects\WebService\WebFolder");
            server.Start(FileService.ClientWork);
        }
    }
}
