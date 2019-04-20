using System;
using System.IO;
using System.Net.Sockets;

namespace WebService
{
    class FileService
    {
        public static void SendFile(TcpClient client, string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Error file path");
            }

            NetworkStream clientStream = client.GetStream();

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
                HttpServer.SendCode(client, 500);
            }

            HttpServer.SendResponseHeader(client, "application/octet-stream", fs.Length);

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
                string request = HttpServer.GetRequest(client);
                string requestUri = HttpServer.GetRequestUri(request);
                string method = HttpServer.GetHttpMethod(request);
                string filePath = HttpServer.WorkingFolder + requestUri;
                if (method == "GET")
                {
                    if (!File.Exists(filePath))
                    {
                        HttpServer.SendCode(client, 404);
                        client.Close();
                        return;
                    }

                    SendFile(client, filePath);
                }

                if (method == "PUT")
                {

                    int startIndex = request.IndexOf("CONTENT=");
                    int endIndex = request.IndexOf("\0", startIndex);
                    string content = request.Substring(startIndex + 8, endIndex - (startIndex + 8));
                    File.WriteAllText(filePath, content);
                    HttpServer.SendCode(client, 200);
                }

                if (method == "POST")
                {
                    int startIndex = request.IndexOf("CONTENT=");
                    int endIndex = request.IndexOf("\0", startIndex);
                    string content = request.Substring(startIndex + 8, endIndex - (startIndex + 8));
                    File.AppendAllText(filePath, content);
                    HttpServer.SendCode(client, 200);
                }

                if (method == "DELETE")
                {
                    if (!File.Exists(filePath))
                    {
                        HttpServer.SendCode(client, 501); //501 - not implemented
                        client.Close();
                        return;
                    }
                    File.Delete(filePath);
                    HttpServer.SendCode(client, 200);
                }

                if (method == "MOVE")
                {
                    int startIndex = request.IndexOf("DEST_FOLDER=");
                    int endIndex = request.IndexOf("\0", startIndex);
                    string folder = request.Substring(startIndex + 12, endIndex - (startIndex + 12));
                    folder = Uri.UnescapeDataString(folder);
                    string destFolder = HttpServer.WorkingFolder + "\\" + folder + "\\";
                    if (!Directory.Exists(destFolder))
                    {
                        HttpServer.SendCode(client, 501);
                        client.Close();
                        return;
                    }
                    if (!File.Exists(filePath))
                    {
                        HttpServer.SendCode(client, 404);
                        client.Close();
                        return;
                    }
                    string filename = Path.GetFileName(requestUri);
                    File.Move(filePath, destFolder + filename);
                    HttpServer.SendCode(client, 200);
                }

                if (method == "COPY")
                {
                    int startIndex = request.IndexOf("DEST_FOLDER=");
                    int endIndex = request.IndexOf("\0", startIndex);
                    string folder = request.Substring(startIndex + 12, endIndex - (startIndex + 12));
                    folder = Uri.UnescapeDataString(folder);
                    string destFolder = HttpServer.WorkingFolder + "\\" + folder + "\\";
                    if (!Directory.Exists(destFolder))
                    {
                        HttpServer.SendCode(client, 501);
                        client.Close();
                        return;
                    }
                    if (!File.Exists(filePath))
                    {
                        HttpServer.SendCode(client, 404);
                        client.Close();
                        return;
                    }
                    string filename = Path.GetFileName(requestUri);
                    string destFilename = destFolder + filename;
                    while (File.Exists(destFilename))
                    {
                        int pos = destFilename.LastIndexOf('.');
                        if (pos < 0)
                        {
                            pos = destFilename.Length;
                        }
                        destFilename = destFilename.Insert(pos, "_copy");
                    }
                    File.Copy(filePath, destFilename);
                    HttpServer.SendCode(client, 200);
                }

                client.Close();
            }
        }
    }
}
