# RabbitMQ-Playground
This Github Repository is used as playground to test RabbitMQ functionality in .NetCore. 

## Project Dependency
* Uses Docker to create RabbitMQ instance
* Contains 4 projects: Producer Console Application, Consumer Console Application, EvenBuRabbitMQ class library and Asp.Net Core 9 Api
* Uses RabbitMQ.Client 7.1.2 Nuget package

## Manual Test
### Step 1:
Start RabbitMQ container with a persistent volume which is able to save messages on disk if not consumed.
```bash
docker run  -d --name rabbitmq -v rabbitmq_volume: /var/lib/rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management
```

For detailed information view: [Setup RabbitMq Docker document](Setup_RabbitMQ_Using_Docker.md)

Note: After running command you can open RabbitMq web application using port created above: **[click here to open rabbimq web management app](http://localhost:15672)**

Note: default username and password is **"guest"**

### Step 2: Run Producer ConsoleApp
```bash
dotnet run RabbitMQ.Producer.ConsoleApp
```

### Step 3: Run Consumer ConsoleApp - this will consume messages published by producer
```bash
dotnet run RabbitMQ.Consumer.ConsoleApp
```

### Step 4: Run Swagger API to test durable "Order_Queue"

1. Run Web Api project which will create Thread for RabbitMQ Basic consumer
2. Open swagger: [click here to open swagger](http://localhost:5111/swagger/index.html)
3. Click on "Post" order api method to send an Order to the API
4. Click "Try it out" button
5. Click "execute" button to send order
6. The Order API method will the use "EventBus" to publish message to Queue
7. Open RabbitMQ management application to view queue and statistics of processed message. You can place breakpoints in the solution to view message before they are processed.


## Note:
If Queue does not exist message will be discarded. In the EventBus we have created an "Alternate Exchange" for message with an incorrect binding (i.e. Routing Key). This means that *"OrderShippedIntegrationEvent"* will be routed to "Alternate Exchange" since we have not setup a binding.


## Run Asp.net core API using Docker Compose

*NB: Switch to Web Api directory*
1. To create container, run the following command: dotnet publish --os linux --arch x64 /t:PublishContainer -c Release
2. Run command: docker compose up

## Todo: Kubernetes
