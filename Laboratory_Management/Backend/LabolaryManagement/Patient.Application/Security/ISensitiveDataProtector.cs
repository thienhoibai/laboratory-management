namespace Patient.Application.Security;

public interface ISensitiveDataProtector
{
    byte[]? Encrypt(string? plain);
    string? Decrypt(byte[]? cipher);
}
