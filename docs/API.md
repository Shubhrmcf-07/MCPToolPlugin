# API Reference

## Base URL
```
http://localhost:5000
```

## Endpoints

### 1. Health Check
Check if the server is running.

**Endpoint**: `GET /health`

**Response**:
```json
{
  "status": "healthy"
}
```

---

### 2. Get All Tools
Retrieve a list of all registered tools.

**Endpoint**: `GET /api/tools`

**Response**:
```json
{
  "tools": [
    {
      "toolId": "text-processing",
      "name": "Text Processing Tool",
      "version": "1.0.0",
      "author": "MCP Team",
      "description": "Provides text processing capabilities..."
    }
  ]
}
```

---

### 3. Get Tool Details
Retrieve detailed information about a specific tool.

**Endpoint**: `GET /api/tools/{toolId}`

**Parameters**:
- `toolId` (string, path): The unique identifier of the tool

**Response**:
```json
{
  "toolId": "text-processing",
  "name": "Text Processing Tool",
  "version": "1.0.0",
  "author": "MCP Team",
  "description": "Provides text processing capabilities...",
  "functions": [
    {
      "name": "ToUpperCase",
      "description": "Converts text to uppercase",
      "parameters": [
        {
          "name": "text",
          "type": "string",
          "description": "The text to convert",
          "required": true
        }
      ]
    }
  ]
}
```

---

### 4. Get Tool Functions
Get the list of functions exposed by a specific tool.

**Endpoint**: `GET /api/tools/{toolId}/functions`

**Parameters**:
- `toolId` (string, path): The unique identifier of the tool

**Response**:
```json
{
  "functions": [
    {
      "name": "ToUpperCase",
      "description": "Converts text to uppercase",
      "parameters": [
        {
          "name": "text",
          "type": "string",
          "description": "The text to convert",
          "required": true
        }
      ]
    },
    {
      "name": "CountWords",
      "description": "Counts the number of words in text",
      "parameters": [...]
    }
  ]
}
```

---

### 5. Execute Tool Function
Execute a function on a specific tool.

**Endpoint**: `POST /api/tools/{toolId}/execute`

**Parameters**:
- `toolId` (string, path): The unique identifier of the tool

**Request Body**:
```json
{
  "functionName": "string",
  "parameters": {
    "key": "value"
  }
}
```

**Response (Success)**:
```json
{
  "success": true,
  "data": {
    "result": "HELLO WORLD"
  },
  "error": null,
  "executionTimeMs": 15
}
```

**Response (Error)**:
```json
{
  "success": false,
  "data": null,
  "error": "Missing required parameter: text",
  "executionTimeMs": 5
}
```

---

## Examples

### Example 1: Get All Tools
```bash
curl http://localhost:5000/api/tools
```

### Example 2: Get Text Processing Tool Details
```bash
curl http://localhost:5000/api/tools/text-processing
```

### Example 3: Convert Text to Uppercase
```bash
curl -X POST http://localhost:5000/api/tools/text-processing/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "ToUpperCase",
    "parameters": {
      "text": "hello world"
    }
  }'
```

**Response**:
```json
{
  "success": true,
  "data": {
    "result": "HELLO WORLD",
    "originalLength": 11
  },
  "error": null,
  "executionTimeMs": 12
}
```

### Example 4: Count Words
```bash
curl -X POST http://localhost:5000/api/tools/text-processing/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "CountWords",
    "parameters": {
      "text": "the quick brown fox"
    }
  }'
```

**Response**:
```json
{
  "success": true,
  "data": {
    "wordCount": 4,
    "text": "the quick brown fox"
  },
  "error": null,
  "executionTimeMs": 8
}
```

### Example 5: Reverse Text
```bash
curl -X POST http://localhost:5000/api/tools/text-processing/execute \
  -H "Content-Type: application/json" \
  -d '{
    "functionName": "Reverse",
    "parameters": {
      "text": "hello"
    }
  }'
```

**Response**:
```json
{
  "success": true,
  "data": {
    "reversed": "olleh",
    "originalLength": 5
  },
  "error": null,
  "executionTimeMs": 10
}
```

---

## Data Models

### ToolResult
Response from executing a tool function.

```typescript
{
  success: boolean           // Whether execution succeeded
  data: object | null        // Result payload
  error: string | null       // Error message if failed
  executionTimeMs: number    // Execution duration in milliseconds
}
```

### ToolFunction
Description of a function exposed by a tool.

```typescript
{
  name: string                    // Function identifier
  description: string             // What it does
  parameters: ToolParameter[]     // List of parameters
}
```

### ToolParameter
Description of a function parameter.

```typescript
{
  name: string          // Parameter name
  type: string          // Data type (string, int, bool, etc.)
  description: string   // Usage information
  required: boolean     // Whether it's mandatory
}
```

### ToolMetadata
Information about a tool.

```typescript
{
  toolId: string        // Unique identifier
  name: string          // Display name
  version: string       // Semantic version
  author: string        // Creator information
  description: string   // What it does
}
```

---

## Error Handling

### Tool Not Found
```json
{
  "success": false,
  "data": null,
  "error": "Tool not found: invalid-tool-id",
  "executionTimeMs": 2
}
```

### Missing Required Parameters
```json
{
  "success": false,
  "data": null,
  "error": "Missing required parameters",
  "executionTimeMs": 1
}
```

### Function Not Found
```json
{
  "success": false,
  "data": null,
  "error": "Function not found: InvalidFunction",
  "executionTimeMs": 3
}
```

### Invalid Request
```
HTTP/1.1 400 Bad Request
```

---

## Best Practices

1. **Always check `success` field** before using `data`
2. **Validate parameters** before sending
3. **Handle timeouts** for long-running operations
4. **Cache tool metadata** to reduce API calls
5. **Use appropriate HTTP methods** (GET for queries, POST for mutations)
6. **Include Content-Type header** for POST requests

---

## Rate Limiting (Production)
While not enforced in development, production deployments may include rate limiting. Check with your administrator for limits.

---

## Versioning
Current API Version: **1.0**

Breaking changes will increment the major version (e.g., v2.0).
