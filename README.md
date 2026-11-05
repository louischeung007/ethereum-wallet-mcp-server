# Ethereum Wallet MCP Server

A self-contained [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server that exposes Ethereum wallet tools to AI agents over stdio.

This repository contains the complete MCP server implementation (source code + Dockerfile). No database, no external RPC provider, and no secrets are required to build and run it.

## Tools

| Tool | Description |
|------|-------------|
| `create_wallet` | Create a new Ethereum wallet |
| `list_wallets` | List all wallets |
| `get_balance` | Get the ETH balance of a wallet |
| `validate_address` | Validate an Ethereum address (full EIP-55 checksum verification) |
| `gas_price` | Get the current gas price in Gwei |
| `list_transactions` | List transactions (paged) |
| `get_transaction` | Get details of a single transaction |

## Build & run with Docker

```bash
docker build -t ethereum-wallet-mcp .
docker run -i ethereum-wallet-mcp
```

## Run locally

```bash
dotnet run
```

## Connect (e.g. Claude Desktop / Cursor)

```json
{
  "mcpServers": {
    "ethereum-wallet-mcp": {
      "command": "docker",
      "args": ["run", "-i", "ethereum-wallet-mcp"]
    }
  }
}
```

## Stack

- .NET 9
- [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) (stdio transport)