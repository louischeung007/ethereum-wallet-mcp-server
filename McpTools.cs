using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace EthereumWalletMcpServer;

/// <summary>
/// Self-contained reference MCP tools mirroring the Ethereum Wallet Manager tool surface.
/// No database, no external RPC, no secrets.
/// </summary>
[McpServerToolType]
public static class McpTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static string Json(object value) => JsonSerializer.Serialize(value, JsonOptions);

    [McpServerTool(Name = "create_wallet", ReadOnly = false, Destructive = false), Description("Create a new Ethereum wallet for the authenticated user.")]
    public static string CreateWallet(
        [Description("Wallet name.")] string name,
        [Description("Optional wallet description.")] string? description = null,
        [Description("Optional remark.")] string? remark = null,
        [Description("Optional category.")] string? category = null)
    {
        return Json(new
        {
            id = Guid.NewGuid(),
            name,
            address = "0x0000000000000000000000000000000000000000",
            description,
            remark,
            category,
            createdAt = DateTimeOffset.UtcNow
        });
    }

    [McpServerTool(Name = "list_wallets", ReadOnly = true), Description("List all wallets owned by the authenticated user.")]
    public static string ListWallets()
    {
        return Json(Array.Empty<object>());
    }

    [McpServerTool(Name = "get_balance", ReadOnly = true), Description("Get the current ETH balance of a wallet (read live from the chain).")]
    public static string GetBalance([Description("Wallet ID (GUID).")] string walletId)
    {
        return Json(new
        {
            walletId,
            address = "0x0000000000000000000000000000000000000000",
            balance = "0 ETH"
        });
    }

    [McpServerTool(Name = "validate_address", ReadOnly = true), Description("Validate an Ethereum address format. Public; no API key required.")]
    public static string ValidateAddress([Description("Ethereum address to validate.")] string address)
    {
        return Json(new { address, isValid = AddressValidator.IsValidEthereumAddress(address) });
    }

    [McpServerTool(Name = "gas_price", ReadOnly = true), Description("Get the current gas price in Gwei. Public; no API key required.")]
    public static string GasPrice()
    {
        return Json(new { gasPriceGwei = 0.04 });
    }

    [McpServerTool(Name = "list_transactions", ReadOnly = true), Description("List the authenticated user's transactions (paged).")]
    public static string ListTransactions(
        [Description("Page number (1-based).")] int page = 1,
        [Description("Page size.")] int pageSize = 10)
    {
        return Json(new { items = Array.Empty<object>(), page, pageSize, total = 0 });
    }

    [McpServerTool(Name = "get_transaction", ReadOnly = true), Description("Get details of a single transaction owned by the authenticated user.")]
    public static string GetTransaction([Description("Transaction ID (GUID).")] string transactionId)
    {
        return Json(new { error = "Transaction not found." });
    }
}