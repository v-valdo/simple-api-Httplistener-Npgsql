using Npgsql;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Http_Cs_Server;
public class Server
{
    private readonly NpgsqlDataSource _db;
    public int port = 3000;
    private HttpListener _listener = new();
    private string[] logs = File.ReadAllLines("logs.txt");

    public Server(NpgsqlDataSource db)
    {
        _db = db;
    }

    public void Start()
    {
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _listener.Prefixes.Add($"http://localhost:{port}/list/");
        _listener.Prefixes.Add($"http://localhost:{port}/list/all/");
        _listener.Start();

        Console.WriteLine("Server listening on port: " + port);

        _listener.BeginGetContext(new AsyncCallback(Route), _listener);
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async void Route(IAsyncResult result)
    {
        if (result.AsyncState is HttpListener listener)
        {

            HttpListenerContext context = _listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            HttpListenerResponse response = context.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/plain";

            // Log -> logs.txt
            string[] requestData =
            {
            "**************************************",
            DateTime.Now.ToShortDateString(),
            request.HttpMethod,
            request.Url?.ToString() ?? "/",
            request.UserAgent, request.UserHostName,
            "**************************************"
            };

            File.AppendAllLines("logs.txt", requestData);
            Console.WriteLine($"{request.HttpMethod} request logged");

            // insert logic for db insert -> logs table

            if (request.HasEntityBody && request.HttpMethod == "POST")
            {
                using (var body = request.InputStream)
                {
                    var encoder = request.ContentEncoding;

                    using (var reader = new StreamReader(body, encoder))
                    {
                        var cmd = _db.CreateCommand("insert into list (date, description) values ($1, $2)");

                        string postBody = reader.ReadToEnd();
                        Console.WriteLine(postBody);

                        string[] parts = postBody.Split("&");

                        foreach (var part in parts)
                        {
                            string[] dateDescription = part.Split("=");

                            string column = dateDescription[0];
                            string value = dateDescription[1];

                            //date
                            //value1
                            //description
                            //value2

                            if (column == "date")
                            {
                                cmd.Parameters.AddWithValue(Convert.ToDateTime(value));
                            }
                            else if (column == "description")
                            {
                                cmd.Parameters.AddWithValue(value);
                            }
                        }
                        await cmd.ExecuteNonQueryAsync();
                    }
                    // insert logic for post (curl -d "date=value1&description=value2" -X POST http://localhost:3000/data)
                }
            }

            else
            {
                string path = request.Url?.AbsolutePath ?? "/";
                string responseString = "";

                // view all entries
                if (request.HttpMethod == "GET" && path.Contains("todo/list/all"))
                {
                    const string qTodo = "select * from list";

                    var cmd = _db.CreateCommand(qTodo);
                    var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        responseString += reader.GetInt32(0) + ", ";
                        responseString += reader.GetDateTime(1).ToShortDateString() + ", ";
                        responseString += reader.GetString(2) + ", ";
                    }
                }

                // Query specific dates
                else if (request.HttpMethod == "GET" && path.Contains("todo/list/"))
                {
                    const string qTodo = "select description from list where date = $1";

                    var cmd = _db.CreateCommand(qTodo);

                    var date = DateTime.Parse(path.Split("/").Last());

                    cmd.Parameters.AddWithValue(date);

                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        responseString += reader.GetString(0);
                    }

                }
                else
                {
                    responseString = "Nothing here...";
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }

            // Loopback
            _listener.BeginGetContext(new AsyncCallback(Route), _listener);
        }
    }
}
