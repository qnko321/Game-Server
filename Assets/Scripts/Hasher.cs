using System;
using System.Security.Cryptography;
using System.Text;

public static class Hasher
{
    public static string GetHash(HashAlgorithm _hashAlgorithm, byte[] _buffer)
    {
        byte[] _data = _hashAlgorithm.ComputeHash(_buffer);
        
        StringBuilder _sBuilder = new StringBuilder();
        for (int i = 0; i < _data.Length; i++)
        {
            _sBuilder.Append(_data[i].ToString("x2"));
        }

        return _sBuilder.ToString();
    }

    public static bool Compare(string _hash1, string _hash2)
    {
        StringComparer _comparer = StringComparer.OrdinalIgnoreCase;
        return _comparer.Compare(_hash1, _hash2) == 0;
    }
}
