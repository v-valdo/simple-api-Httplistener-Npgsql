	# prep
- create db "todo"

- set dbUri

- create table list(id serial primary key, date date, description text);

- create table log(id serial primary key, date date, method text); 

- insert into table list(date, description) values ('2024-06-01', 'feed the hamster!')
	
	# bash #1
> dotnet run
> 1
	starts http listener

	# bash #2
> curl -X GET http://localhost:3000/todo/list/all
	prints all entries in table "todo"

> curl -d "date=dateValue&description=descriptionValue" -X POST http://localhost:3000/
	posts & parses new entry

> curl -X GET http://localhost:3000/todo/<date>
	prints value at <date>

> curl -X GET http://localhost:3000/todo/2024-06-01
	prints "feed the hamster!""