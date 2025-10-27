#Setup RabbitMQ Using Docker

## Prerequisite
1. Install Docker Desktop

### Step 1: Create Docker Volume
*Docker volume is required to ensure that persistent message can be restored if container is turned off.*

1. Open command prompt (cmd)
2. Type command: docker volume create rabbitmq_volume

### Step 2: Select RabbitMQ image you want to use and pull image
docker pull rabitmq:3.9_management

### Step 3: Create RabbitMQ container with volume
docker run -d --name rabbitmq -v rabbitmq_volume: /var/lib/rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.9-management

### Note:
When container is successfully created you will be able to open RabbitMQ management aplication on **[http://localhost:15672](http://localhost:15672)**

**Additional Commands once container is created on your local machine:**
docker start rabbitmq
docker stop rabbitmq
