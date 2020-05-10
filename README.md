# ASP.NET Core application <i>(MSSQL + SignalR)</i>
<br>
<p>To run application, use these commands below. Database is automatically created via Entity Framework with Code First Approach</p>

```
$ dotnet restore
$ dotnet run
```

[![q.png](https://i.postimg.cc/nz3S0XKd/q.png)](https://postimg.cc/2bBx58Kv)

User can do CRUD operations with his todos. There are identity and google registration inside app. Application has neat and clear interface based on material bootstrap lib. Architecture of app is multilayer (DAL, Presentation layer and Business layer). Onion architecture allows the application to be orthogonal.
