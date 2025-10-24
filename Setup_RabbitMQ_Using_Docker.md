#Setup RabbitMQ Using Docker

## Prerequist
1. Install Docker Desktop

### Step 1: Create Docker Volume
*Docker volume is required to ensure that persitent message can be restored if container is turned off.*

1. Open command promt(cmd)
2. type command: docker volume create rabbitmq_volume
