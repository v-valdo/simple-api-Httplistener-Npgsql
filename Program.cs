using Http_Cs_Server;
using System.Net;

// Database goes here
const string dbUri = "Host=localhost;Port=5455;Username=postgres;Password=postgres;Database=todo;";

Database todoDb = new(dbUri);

if (!File.Exists("logs.txt"))
{
    File.Create("logs.txt");
}

bool listen = false;

Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    listen = false;
};


// server header
bool header = true;
while (header)
{
    Console.WriteLine(@"
-----------------
1. start server |
2. read logs    |
3. ciao         |
-----------------"
);

    if (int.TryParse(Console.ReadLine(), out int input))
        switch (input)
        {
            case 1:
                Server server = new(todoDb.Connector());
                server.Start();
                listen = true;
                Console.Clear();
                while (listen) { }
                server.Stop();
                break;

            case 2:
                using (var reader = new StreamReader("logs.txt"))
                {
                    // Read all lines from the file
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine() ?? "nada";
                        Console.WriteLine(line);
                    }
                }
                break;
            case 3:
                listen = false;
                break;
            default:
                break;
        }
    else
    {
        Console.WriteLine("invalid input");
    }
}