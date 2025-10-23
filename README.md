# RabbitMQ-Playground
This Github Repository is used as playground to test RabbitMQ functionality in .NetCore. 

## Project Dependency
* Uses Docker to create RabbitMQ instance
* Contain 3 projects: Producer, Consumer and EvenBuRabbitMQ
* Uses RabbitMQ.Client 7.1.2 Nuget package

## Manual Test
### Step 1:
Start RabbitMQ container with a persistent volume which is able to save messages on disk if not consumed.
docker run  -d --name rabbitmq -v rabbitmq_volume: /var/lib/rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management

Note: After running command you can open RabbitMq web application using port created above: **[click here to open rabbimq web app](http://localhost:15672)**

Note: default username and password is **"guest"**

### Step 2: Run Producer ConsoleApp
dotnet run RabbitMQ.Producer.ConsoleApp

### Step 3: Run Consumer ConsoleApp - this will consume messages published by producer
dotnet run RabbitMQ.Consumer.ConsoleApp

### Step 4: Run Swagger API to test "Order_Queue"


## Todo
* Edit project to use Docker Compose to avoid running Docker commands manually.
