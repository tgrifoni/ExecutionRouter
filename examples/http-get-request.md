# HTTP GET Request Example

## Actual curl command:
```bash
curl -X GET "http://localhost:7052/api/https://api.github.com/users/octocat" \
  -H "X-ExecutionRouter-ExecutorType: http" \
  -H "X-Request-Id: req_abc123def456" \
  -H "X-Correlation-Id: corr_xyz789uvw" \
  -H "Accept: application/json"
```

## What happens:
1. ExecutionRouter receives the request for path `/api/https://api.github.com/users/octocat`
2. Sees `X-ExecutionRouter-ExecutorType: http` header
3. Extracts full URL `https://api.github.com/users/octocat` from the path
4. Forwards GET request to `https://api.github.com/users/octocat`
5. Returns the GitHub API response

## Alternative using query parameter:
```bash
curl -X GET "http://localhost:7052/api/https://api.github.com/users/octocat?executor=http" \
  -H "Accept: application/json"
```