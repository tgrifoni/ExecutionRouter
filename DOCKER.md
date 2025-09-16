# Docker Instructions

## Build and Run

```bash
# Build and run with Docker Compose
docker-compose up --build

# Or build and run separately
docker build -t executionrouter:latest .
docker run -p 7052:8080 executionrouter:latest
```

## Test

```bash
curl http://localhost:7052/health
```

The container runs HTTP on internal port 8080, accessible externally on port 7052.