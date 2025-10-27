# RabbitMQ-Playground
This Github Repository is used as playground to test RabbitMQ functionality in .NetCore. 

## Project Dependency
* Uses Docker to create RabbitMQ instance
* Contains 4 projects: Producer Console Application, Consumer Console Application, EvenBuRabbitMQ class library and Asp.Net Core 9 Api
* Uses RabbitMQ.Client 7.1.2 Nuget package

## Manual Test
### Step 1:
Start RabbitMQ container with a persistent volume which is able to save messages on disk if not consumed.
docker run  -d --name rabbitmq -v rabbitmq_volume: /var/lib/rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management

For detailed information view: [Setup RabbitMq Docker document](Setup_RabbitMQ_Using_Docker.md)

Note: After running command you can open RabbitMq web application using port created above: **[click here to open rabbimq web app](http://localhost:15672)**

Note: default username and password is **"guest"**

### Step 2: Run Producer ConsoleApp
dotnet run RabbitMQ.Producer.ConsoleApp

### Step 3: Run Consumer ConsoleApp - this will consume messages published by producer
dotnet run RabbitMQ.Consumer.ConsoleApp

### Step 4: Run Swagger API to test "Order_Queue"

1. Run Web Api project which will create Thread for rabbitMQ Basic consumer
2. Open swagger: [click here to open swagger](http://localhost:5111/swagger/index.html)
3. Click on "Post" order api method to send an Order to the API
4. Click "Try it out" button
5. Click "execute" button to send order
6. The Order API method will the use "EventBus" to publish message to Queue

## Note:
If Queue does not exist message will be discarded.


## Todo
* Edit project to use Docker Compose to avoid running Docker commands manually.
