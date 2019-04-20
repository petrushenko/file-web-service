using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebService
{
    class HttpServer
    {
        public delegate void ClientWorkDelegate(object clientObj);

        public static string WorkingFolder { get; set; }

        private int Port { get; set; }

        private bool IsActive { get; set; }

        private TcpListener Listener { get; set; }

        public static void SendCode(TcpClient client, int code)
        {
            string codeStr = code.ToString() + " " + ((HttpStatusCode)code).ToString();

            string html = "<html><body><h1>" + codeStr + "</h1></body></html>";

            string response = "HTTP/1.1 " + codeStr + "\nContent-type: text/html\nContent-Length:" + html.Length.ToString() + "\n\n" + html;

            byte[] buffer = Encoding.ASCII.GetBytes(response);

            NetworkStream clientStream = client.GetStream();

            clientStream.Write(buffer, 0, buffer.Length);

            clientStream.Dispose();

            client.Close();
        }

        public static string GetContentType(string extension)
        {
            string contentType;
            switch (extension)
            {
                case ".htm":
                case ".html":
                    contentType = "text/html";
                    break;
                case ".css":
                    contentType = "text/stylesheet";
                    break;
                case ".js":
                    contentType = "text/javascript";
                    break;
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                    contentType = "image/" + extension.Substring(1);
                    break;
                default:
                    if (extension.Length > 1)
                    {
                        contentType = "application/" + extension.Substring(1);
                    }
                    else
                    {
                        contentType = "application/octet-stream";
                    }
                    break;
            }
            return contentType;
        }

        public HttpServer(int port, string workingFolder)
        {
            Port = port;
            IsActive = false;
            if (Directory.Exists(workingFolder))
            {
                WorkingFolder = workingFolder;
            }
            else
            {
                throw new Exception("Bad server folder");
            }
        }

        public void Start(ClientWorkDelegate clientWork)
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
            IsActive = true;
            Thread thread = new Thread(WaitConnections);
            //try
            //ClientWorkDelegate clientWork = new ClientWorkDelegate(ClientWork);
            thread.Start(clientWork);
        }

        public void WaitConnections(object clientWorkDelegateObj)
        {
            ClientWorkDelegate clientWork = clientWorkDelegateObj as ClientWorkDelegate;
            Console.WriteLine("Waiting for connections...");
            while (IsActive)
            {
                TcpClient client = Listener.AcceptTcpClient();
                Console.WriteLine("New connection!");
                //try
                Thread thread = new Thread(clientWork.Invoke);
                thread.Start(client);
            }
        }

        public static string GetRequest(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();
            string request = "";
            byte[] buffer = new byte[1024];

            while ((clientStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                request += Encoding.UTF8.GetString(buffer);
                // конец http запроса
                if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096)
                {
                    break;
                }
            }
            return request;
        }

        public static void SendResponseHeader(TcpClient client, string contentType, long contentLength)
        {
            NetworkStream clientStream = client.GetStream();
            string headers = "HTTP/1.1 200 OK\nContent-Type: " + contentType + "\nContent-Length: " + contentLength + "\n\n";
            byte[] headersBuffer = Encoding.UTF8.GetBytes(headers);
            if (client.Connected)
            {
                clientStream.Write(headersBuffer, 0, headersBuffer.Length);
            }
        }

        public static string GetHttpMethod(string request)
        {
            //if (request == null || request == "")
            //    return null;
            string method;
            string[] requestArr = request.Split(' ');
            method = requestArr[0];
            return method;
        }

        public static string GetRequestUri(string request)
        {
            if (request == null || request == "")
                return "";

            Match match = Regex.Match(request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");
            if (match == Match.Empty)
            {
                return "";
            }

            string uri = match.Groups[1].Value;
            uri = Uri.UnescapeDataString(uri);
            uri = uri.Replace('+', ' ');
            return uri;
        }

        public static void SendFileContent(TcpClient client, string path)
        {

            if (!File.Exists(path))
            {
                throw new Exception("Error file path");
            }

            NetworkStream clientStream = client.GetStream();
            string extension = path.Substring(path.LastIndexOf('.'));
            string contentType = GetContentType(extension);

            FileStream fs = null;

            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
                SendCode(client, 500);
            }

            SendResponseHeader(client, contentType, fs.Length);

            byte[] buffer = new byte[1024];

            while (fs.Position < fs.Length)
            {
                fs.Read(buffer, 0, buffer.Length);
                clientStream.Write(buffer, 0, buffer.Length);
            }
            fs.Close();
        }

        public static void ClientWork(object clientObj)
        {
            using (TcpClient client = clientObj as TcpClient)
            {
                string request = GetRequest(client);

                string requestUri = GetRequestUri(request);

                if (requestUri != null && requestUri.IndexOf("..") >= 0)
                {
                    SendCode(client, 400);
                    return;
                }

                if (requestUri != null && requestUri.EndsWith("/"))
                {
                    requestUri += "index.html";
                }

                string filePath = WorkingFolder + requestUri;

                if (!File.Exists(filePath))
                {
                    SendCode(client, 404);
                    return;
                }

                SendFileContent(client, filePath);
                client.Close();
            }
        }
    }
}
