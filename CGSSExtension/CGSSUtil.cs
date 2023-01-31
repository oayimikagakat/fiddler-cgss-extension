using MsgPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CGSSExtension
{
    public static class CGSSUtil
    {
        public static string Deobfuscate(string udid)
        {
            int length = Convert.ToInt32(udid.Substring(0, 4), 16);
            char[] result = new char[length];

            for (int i = 0, j = 6; i < length; i++, j += 4)
            {
                char c = (char)(udid[j] - 10);
                result[i] = c;
            }

            return new string(result);
        }

        public static string DecryptBody(string body, string udid)
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(body);

                byte[] key = new byte[32];
                Array.Copy(decoded, decoded.Length - 32, key, 0, 32);

                string formattedUdid = udid.Replace("-", "");
                byte[] iv = Enumerable.Range(0, formattedUdid.Length)
                                    .Where(x => x % 2 == 0)
                                    .Select(x => Convert.ToByte(formattedUdid.Substring(x, 2), 16))
                                    .ToArray();

                AesCryptoServiceProvider aes = new AesCryptoServiceProvider
                {
                    Key = key,
                    IV = iv,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };

                byte[] b = new byte[decoded.Length - 32];
                Array.Copy(decoded, 0, b, 0, decoded.Length - 32);

                ICryptoTransform decryptor = aes.CreateDecryptor();
                b = decryptor.TransformFinalBlock(b, 0, b.Length);

                byte[] decrypted = Convert.FromBase64String(Encoding.UTF8.GetString(b));

                MessagePackObjectDictionary result = Unpacking.UnpackDictionary(decrypted).Value;
                Dictionary<string, object> resultDict = ConvertToNormalDict(result);
                JObject json = JObject.FromObject(resultDict);

                return json.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private static object ConvertToNormalType(MessagePackObject value)
        {
            if (value.IsDictionary)
            {
                return ConvertToNormalDict(value.AsDictionary());
            }
            else if (value.IsList)
            {
                var normalList = new List<object>();
                foreach (var item in value.AsList())
                {
                    normalList.Add(ConvertToNormalType(item));
                }
                return normalList;
            }
            else
            {
                return value.ToObject();
            }
        }

        private static Dictionary<string, object> ConvertToNormalDict(MessagePackObjectDictionary msgPackDict)
        {
            Dictionary<string, object> normalDict = new Dictionary<string, object>();
            foreach (var kvp in msgPackDict)
            {
                normalDict[kvp.Key.ToString()] = ConvertToNormalType(kvp.Value);
            }
            return normalDict;
        }
    }
}
