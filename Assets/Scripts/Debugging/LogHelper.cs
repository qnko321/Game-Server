using System.Collections.Generic;
using System.Text;
using Terrain;
using UnityEngine;

namespace Debugging
{
    public static class LogHelper
    {
        public static void LogDictionary(Dictionary<string, string> _dict)
        {
            string _log = "{ ";
            foreach (var (_key, _value) in _dict)
            {
                _log += $"[{_key}: {_value}]";
            }
            _log += " }";
            Debug.Log(_log);
        }
        
        public static void LogDictionary(Dictionary<ChunkCoord, (string, int)> _dict)
        {
            string _log = "{ ";
            foreach (var (_key, _value) in _dict)
            {
                _log += $"[{_key.x + "." + _key.z}: {_value}]";
            }
            _log += " }";
            Debug.Log(_log);
        }

        public static void LogByteArray(byte[] _bytes)
        {
            var _sb = new StringBuilder("new byte[] { ");
            foreach (var _byte in _bytes)
            {
                _sb.Append(_byte + ", ");
            }

            _sb.Append("}");
            Debug.Log(_sb.ToString());
        }

        public static void LogDictionary(Dictionary<ChunkCoord, Chunk> _dict)
        {
            string _log = "{ ";
            foreach (var (_key, _value) in _dict)
            {
                _log += $"[{_key.x + "." + _key.z}: {_value}]";
            }
            _log += " }";
            Debug.Log(_log);
        }
        
        // Probably causes the game (editor) to crash 
        public static void LogByte3DArray(byte[,,] _arr)
        {
            string _log = "{ ";
            for (int i = 0; i < _arr.GetLength(0); i++)
            {
                for (int j = 0; j < _arr.GetLength(1); j++)
                {
                    for (int k = 0; k < _arr.GetLength(2); k++)
                    {
                        _log += $"[{_arr[i, j, k]}], ";
                    }
                }
            }

            _log += " }";
            Debug.Log(_log);
        }
    }
}