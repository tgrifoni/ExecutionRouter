# PowerShell Execution Request Example

## Actual curl command:
```bash
curl -X POST "http://localhost:7052/api/admin/mailboxes" \
  -H "X-ExecutionRouter-ExecutorType: powershell" \
  -H "X-Request-Id: req_ps123eml456" \
  -H "X-Correlation-Id: corr_eml789def" \
  -H "Content-Type: application/json" \
  -d '{ "command": "Get-Mailbox" }'
```

## What happens:
1. ExecutionRouter receives request for `/api/admin/mailboxes`
2. Detects `powershell` executor from header
3. Parses JSON body to extract PowerShell command
4. Executes the PowerShell script with specified parameters
5. Returns script output as JSON response

## Query parameter alternative:
```bash
curl -X POST "http://localhost:7052/api/admin/processes?executor=powershell" \
  -H "Content-Type: application/json" \
  -d '{
    "command": "Get-Process | Where-Object {$_.CPU -gt 100} | Select-Object Name, CPU",
    "filter": "CPU -gt 100",
    "maxResults": 20
  }'
```