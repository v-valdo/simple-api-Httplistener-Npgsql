using Npgsql;
namespace Http_Cs_Server;

/* Db table-queries
create table list(id serial primary key, date date, description text);
create table log(id serial primary key, date date, method text); 
*/

public class Database
{
    private string _dbUri;
    public Database(string dbUri)
    {
        _dbUri = dbUri;
    }

    public NpgsqlDataSource Connector() => NpgsqlDataSource.Create(_dbUri);
}
