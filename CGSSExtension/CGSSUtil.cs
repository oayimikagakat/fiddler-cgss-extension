using MsgPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

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

        public static JObject DecryptBody(string body, string udid)
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

                return json;
            }
            catch (Exception e)
            {
                Dictionary<string, string> errorDict = new Dictionary<string, string>
                {
                    { "Error", e.Message },
                    { "Message", "This is because it is not a valid CGSS body or decrypter is missing UDID. Run CGSS Request once if you didn't." }
                };
                return JObject.FromObject(errorDict);
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

        public static void SetJsonTreeView(TreeView treeView, JObject json)
        {
            treeView.BeginUpdate();
            try
            {
                treeView.Nodes.Clear();
                var tNode = treeView.Nodes[treeView.Nodes.Add(new TreeNode("Body"))];
                tNode.Tag = json.Root;

                AddNode(tNode, json.Root);

                treeView.ExpandAll();
            }
            finally
            {
                treeView.EndUpdate();
                treeView.Nodes[0].EnsureVisible();
            }
        }

        public static void AddNode(TreeNode treeNode, JToken token)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    if (property.Value is JValue)
                    {
                        treeNode.Nodes.Add(new TreeNode(string.Format("{0} : {1}", property.Name, property.Value)));
                    }
                    else
                    {
                        var childNode = treeNode.Nodes[treeNode.Nodes.Add(new TreeNode(property.Name))];
                        childNode.Tag = property;
                        AddNode(childNode, property.Value);
                    }
                }
            }
            else if (token is JArray array)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i] is JValue)
                    {
                        treeNode.Nodes.Add(new TreeNode(string.Format("[{0}] {1}", i, array[i])));
                    }
                    else
                    {
                        var childNode = treeNode.Nodes[treeNode.Nodes.Add(new TreeNode(string.Format("[{0}]", i)))];
                        childNode.Tag = array[i];
                        AddNode(childNode, array[i]);
                    }
                }
            }
            else
            {
                var childNode = treeNode.Nodes[treeNode.Nodes.Add(new TreeNode(token.ToString()))];
                childNode.Tag = token;
            }

        }
    }
}
