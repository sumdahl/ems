# Docker Deployment Guide

This guide will help you build, run, and deploy the Employee Management System using Docker.

## Prerequisites

- Docker installed on your system
- Docker Hub account (for pushing images)

## Quick Start with Docker Compose

The easiest way to run the entire application stack (app + database):

```bash
# Build and start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: This deletes all data)
docker-compose down -v
```

The application will be available at: `http://localhost:8080`

## Building the Docker Image

### Build locally

```bash
# Build the image
docker build -t ems-app:latest .

# Build with a specific tag
docker build -t yourusername/ems-app:v1.0.0 .
```

### Run the container (standalone)

If you want to run just the app container (you'll need a separate PostgreSQL instance):

```bash
docker run -d \
  --name ems-app \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=ems;Username=postgres;Password=yourpassword" \
  ems-app:latest
```

## Pushing to Docker Hub

### 1. Login to Docker Hub

```bash
docker login
```

Enter your Docker Hub username and password when prompted.

### 2. Tag your image

```bash
# Replace 'yourusername' with your Docker Hub username
docker tag ems-app:latest yourusername/employee-management-system:latest
docker tag ems-app:latest yourusername/employee-management-system:v1.0.0
```

### 3. Push to Docker Hub

```bash
# Push latest tag
docker push yourusername/employee-management-system:latest

# Push version tag
docker push yourusername/employee-management-system:v1.0.0
```

### 4. Pull and run from Docker Hub

Anyone can now pull and run your image:

```bash
docker pull yourusername/employee-management-system:latest

docker run -d \
  --name ems-app \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=db-host;Database=ems;Username=postgres;Password=password" \
  yourusername/employee-management-system:latest
```

## Docker Compose for Production

Create a `docker-compose.prod.yml` for production deployment:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: ems
      POSTGRES_USER: ${DB_USER:-postgres}
      POSTGRES_PASSWORD: ${DB_PASSWORD:-changeme}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    restart: always

  web:
    image: yourusername/employee-management-system:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=ems;Username=${DB_USER:-postgres};Password=${DB_PASSWORD:-changeme}
    ports:
      - "80:8080"
    depends_on:
      - postgres
    restart: always

volumes:
  postgres-data:
```

Run with:

```bash
docker-compose -f docker-compose.prod.yml up -d
```

## Environment Variables

Key environment variables you can configure:

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` or `Production`
- `ASPNETCORE_URLS` - URL bindings (default: `http://+:8080`)
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string

## Useful Docker Commands

```bash
# View running containers
docker ps

# View all containers (including stopped)
docker ps -a

# View logs
docker logs ems-app
docker logs -f ems-app  # Follow logs

# Execute commands in container
docker exec -it ems-app bash

# Stop container
docker stop ems-app

# Remove container
docker rm ems-app

# Remove image
docker rmi ems-app:latest

# View images
docker images

# Clean up unused resources
docker system prune -a
```

## Troubleshooting

### Database connection issues

If the app can't connect to the database:

1. Ensure PostgreSQL is running: `docker-compose ps`
2. Check database logs: `docker-compose logs postgres`
3. Verify connection string in environment variables

### Port already in use

If port 8080 is already in use, change it in `docker-compose.yml`:

```yaml
ports:
  - "8081:8080"  # Use 8081 instead
```

### Rebuild after code changes

```bash
# Rebuild and restart
docker-compose up -d --build

# Or rebuild specific service
docker-compose build web
docker-compose up -d web
```

## Security Best Practices

For production deployment:

1. **Change default passwords** - Never use default PostgreSQL passwords
2. **Use secrets** - Store sensitive data in Docker secrets or environment files
3. **Enable HTTPS** - Add a reverse proxy (nginx) with SSL certificates
4. **Limit exposed ports** - Only expose necessary ports
5. **Regular updates** - Keep base images updated

## Next Steps

- Set up CI/CD pipeline (GitHub Actions, GitLab CI)
- Deploy to cloud platforms (AWS, Azure, Google Cloud)
- Add monitoring and logging (Prometheus, Grafana)
- Implement backup strategies for PostgreSQL data

---

**Developed by Sumiran Dahal**
