# HTTP POST Request Example

## Actual curl command:
```bash
curl -X POST "http://localhost:7052/api/https://httpbin.org/post" \
  -H "X-ExecutionRouter-ExecutorType: http" \
  -H "X-Request-Id: req_def456ghi789" \
  -H "X-Correlation-Id: corr_mno123pqr" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john.doe@example.com",
    "role": "developer"
  }'
```

## What happens:
1. ExecutionRouter receives POST to `/api/https://httpbin.org/post`
2. Detects `http` executor from header
3. Extracts full URL `https://httpbin.org/post` from the path
4. Forwards POST with JSON body to `https://httpbin.org/post`
5. Returns the response from the website