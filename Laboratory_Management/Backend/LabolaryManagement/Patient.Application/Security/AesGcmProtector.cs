using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Patient.Application.Security;

public sealed class AesGcmProtector : ISensitiveDataProtector
{
    private readonly byte[] _primaryKey; // 32 bytes
    private readonly List<byte[]> _allKeys; // include primary first, then legacy keys

    public AesGcmProtector(IConfiguration cfg)
    {
        // Preferred: multiple keys for rotation (first is primary for encrypt)
        // Sources (in order): Security:PiiKeyBase64s (multiline or ; separated) -> env PATIENT_PII_KEYS
        // Fallback (single key): Security:PiiKeyBase64 -> env PATIENT_PII_KEY
        var multi = cfg["Security:PiiKeyBase64s"] ?? Environment.GetEnvironmentVariable("PATIENT_PII_KEYS");
        var single = cfg["Security:PiiKeyBase64"] ?? Environment.GetEnvironmentVariable("PATIENT_PII_KEY");

        _allKeys = new List<byte[]>();

        if (!string.IsNullOrWhiteSpace(multi))
        {
            var parts = multi
                .Split(new[] { '\n', '\r', ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            foreach (var p in parts)
            {
                var k = Convert.FromBase64String(p);
                if (k.Length != 32) throw new InvalidOperationException("PII key must be 32 bytes Base64");
                _allKeys.Add(k);
            }
        }
        else if (!string.IsNullOrWhiteSpace(single))
        {
            var k = Convert.FromBase64String(single);
            if (k.Length != 32) throw new InvalidOperationException("PII key must be 32 bytes Base64");
            _allKeys.Add(k);
        }

        if (_allKeys.Count == 0)
            throw new InvalidOperationException("Missing PII key(s)");

        _primaryKey = _allKeys[0];
    }

    public byte[]? Encrypt(string? plain)
    {
        if (string.IsNullOrEmpty(plain)) return null;
        var nonce = RandomNumberGenerator.GetBytes(12);
        var pt = Encoding.UTF8.GetBytes(plain);
        var ct = new byte[pt.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_primaryKey);
        aes.Encrypt(nonce, pt, ct, tag);

        var output = new byte[nonce.Length + tag.Length + ct.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, nonce.Length);
        Buffer.BlockCopy(tag,   0, output, nonce.Length, tag.Length);
        Buffer.BlockCopy(ct,    0, output, nonce.Length + tag.Length, ct.Length);
        return output;
    }

    public string? Decrypt(byte[]? blob)
    {
        if (blob == null || blob.Length < 12 + 16) return null;
        var nonce = new byte[12];
        var tag   = new byte[16];
        var ct    = new byte[blob.Length - 28];

        Buffer.BlockCopy(blob, 0,  nonce, 0, 12);
        Buffer.BlockCopy(blob, 12, tag,   0, 16);
        Buffer.BlockCopy(blob, 28, ct,    0, ct.Length);

        foreach (var key in _allKeys)
        {
            try
            {
                var pt = new byte[ct.Length];
                using var aes = new AesGcm(key);
                aes.Decrypt(nonce, ct, tag, pt);
                return Encoding.UTF8.GetString(pt);
            }
            catch (CryptographicException)
            {
                // try next key
            }
        }

        // none of the keys worked
        throw new CryptographicException("Decryption failed with provided PII keys");
    }
}
