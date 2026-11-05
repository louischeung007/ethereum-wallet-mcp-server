using System.Text;

namespace EthereumWalletMcpServer;

/// <summary>
/// Ethereum address (EIP-55) validation. Pure managed implementation, no external deps.
/// </summary>
public static class AddressValidator
{
    private static readonly ulong[] RoundConstants =
    {
        0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808aUL, 0x8000000080008000UL,
        0x000000000000808bUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
        0x000000000000008aUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000aUL,
        0x000000008000808bUL, 0x800000000000008bUL, 0x8000000000008089UL, 0x8000000000008003UL,
        0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800aUL, 0x800000008000000aUL,
        0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL
    };

    private static readonly int[] RhoOffsets =
    {
        0, 1, 62, 28, 27, 36, 44, 6, 55, 20, 3, 10, 43, 25, 39,
        41, 45, 15, 21, 8, 18, 2, 61, 56, 14
    };

    /// <summary>
    /// Returns true when the address has a valid 40-hexchar format and, when mixed-case,
    /// its EIP-55 checksum matches. All-lowercase / all-uppercase are accepted as valid.
    /// </summary>
    public static bool IsValidEthereumAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        if (!address.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return false;

        var hex = address.Substring(2);
        if (hex.Length != 40) return false;
        foreach (var c in hex)
        {
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                return false;
        }

        var lower = hex.ToLowerInvariant();
        if (hex == lower || hex == hex.ToUpperInvariant()) return true;

        var hashHex = BytesToHexLower(Keccak256(Encoding.ASCII.GetBytes(lower)));
        for (var i = 0; i < 40; i++)
        {
            var c = hex[i];
            if (!char.IsLetter(c)) continue;
            var nibble = Convert.ToInt32(hashHex[i].ToString(), 16);
            var shouldBeUpper = nibble >= 8;
            if (char.IsUpper(c) != shouldBeUpper) return false;
        }
        return true;
    }

    private static string BytesToHexLower(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private static byte[] Keccak256(byte[] message)
    {
        const int rate = 136;
        const int outputLen = 32;
        var state = new ulong[25];

        var i = 0;
        while (true)
        {
            var chunk = Math.Min(rate, message.Length - i);
            for (var j = 0; j < chunk; j++)
            {
                state[j >> 3] ^= (ulong)message[i + j] << ((j & 7) * 8);
            }

            if (chunk < rate)
            {
                state[chunk >> 3] ^= 0x01UL << ((chunk & 7) * 8);
                state[(rate - 1) >> 3] ^= 0x80UL << (((rate - 1) & 7) * 8);
                KeccakF1600(state);
                break;
            }

            i += chunk;
            KeccakF1600(state);
        }

        var output = new byte[outputLen];
        for (var j = 0; j < outputLen; j++)
        {
            output[j] = (byte)((state[j >> 3] >> ((j & 7) * 8)) & 0xff);
        }
        return output;
    }

    private static void KeccakF1600(ulong[] a)
    {
        var b = new ulong[25];
        var c = new ulong[5];
        var d = new ulong[5];

        for (var round = 0; round < 24; round++)
        {
            // Theta
            for (var x = 0; x < 5; x++)
                c[x] = a[x] ^ a[x + 5] ^ a[x + 10] ^ a[x + 15] ^ a[x + 20];
            for (var x = 0; x < 5; x++)
                d[x] = c[(x + 4) % 5] ^ ROL(c[(x + 1) % 5], 1);
            for (var x = 0; x < 5; x++)
                for (var y = 0; y < 5; y++)
                    a[x + 5 * y] ^= d[x];

            // Rho and Pi
            for (var x = 0; x < 5; x++)
                for (var y = 0; y < 5; y++)
                    b[y + 5 * ((2 * x + 3 * y) % 5)] = ROL(a[x + 5 * y], RhoOffsets[x + 5 * y]);

            // Chi
            for (var x = 0; x < 5; x++)
                for (var y = 0; y < 5; y++)
                    a[x + 5 * y] = b[x + 5 * y] ^ ((~b[(x + 1) % 5 + 5 * y]) & b[(x + 2) % 5 + 5 * y]);

            // Iota
            a[0] ^= RoundConstants[round];
        }
    }

    private static ulong ROL(ulong value, int offset)
        => offset == 0 ? value : (value << offset) | (value >> (64 - offset));
}