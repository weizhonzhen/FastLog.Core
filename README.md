```
//Receive log
builder.Services.AddFastLogReceive();

//client log
builder.Services.AddFastLog();

```

```
//json 
{
  "RabbitMQ": {
    "Host": "127.0.0.1",
    "PassWord": "guest",
    "UserName": "guest",
    "QueueName": "Log",
    "ExchangeName": "LogBox",
    "RouteKey": "LogKey",
    "Port": 5672,
    "VirtualHost": "/"
  },
  "Elasticsearch": {
    "Host": [ "http://127.0.0.1:9200" ],
    "UserName": "elastic",
    "PassWord": "123456"
  }
}
```


```

//log
var ifastLog = builder.Services.BuildServiceProvider().GetService<IFastLog>();
ifastLog.Save("emrlog", "test1", "message");
var count = ifastLog.Count("emrlog");
var list = ifastLog.GetList("emrlog");
var page = ifastLog.Page("emrlog", "test", string.Empty,1,10,true);
```

