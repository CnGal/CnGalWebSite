using System;
using System.Security.Cryptography;
using System.Text;

namespace CnGalWebSite.Core.Helpers
{
    /// <summary>
    /// 票根ID加密工具类，防止用户遍历访问所有票根
    /// </summary>
    public static class TicketIdEncryption
    {
        // 加密密钥（实际项目中应该从配置文件读取）
        // AES-256需要32字节的密钥
        private static readonly byte[] EncryptionKeyBytes = GetKeyBytes("CnGalExpoTicket2024SecretKey!@#");

        /// <summary>
        /// 生成固定长度的密钥字节数组
        /// </summary>
        /// <param name="keyString">原始密钥字符串</param>
        /// <returns>32字节的密钥数组</returns>
        private static byte[] GetKeyBytes(string keyString)
        {
            var keyBytes = Encoding.UTF8.GetBytes(keyString);

            // 如果密钥长度不足32字节，使用SHA256填充
            if (keyBytes.Length < 32)
            {
                using (var sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(keyBytes);
                }
            }

            // 如果密钥长度超过32字节，截取前32字节
            if (keyBytes.Length > 32)
            {
                var result = new byte[32];
                Array.Copy(keyBytes, result, 32);
                return result;
            }

            return keyBytes;
        }

        /// <summary>
        /// 加密票根ID
        /// </summary>
        /// <param name="ticketId">原始票根ID</param>
        /// <returns>加密后的字符串</returns>
        public static string EncryptTicketId(long ticketId)
        {
            var plainText = ticketId.ToString();

            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKeyBytes;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                    var cipherBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

                    // 将IV和加密数据组合
                    var result = new byte[aes.IV.Length + cipherBytes.Length];
                    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                    Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        /// <summary>
        /// 解密票根ID
        /// </summary>
        /// <param name="encryptedTicketId">加密的票根ID字符串</param>
        /// <returns>原始票根ID，如果解密失败返回0</returns>
        public static long DecryptTicketId(string encryptedTicketId)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedTicketId);

            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKeyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // 提取IV和加密数据
                var iv = new byte[aes.IV.Length];
                var cipherBytes = new byte[encryptedBytes.Length - aes.IV.Length];

                Buffer.BlockCopy(encryptedBytes, 0, iv, 0, aes.IV.Length);
                Buffer.BlockCopy(encryptedBytes, aes.IV.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                    if (long.TryParse(decryptedText, out long ticketId))
                    {
                        return ticketId;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// 生成票根访问令牌（用于URL中的参数）
        /// </summary>
        /// <param name="ticketId">票根ID</param>
        /// <param name="activityId">活动ID</param>
        /// <returns>访问令牌</returns>
        public static string GenerateTicketToken(long ticketId, long activityId)
        {
            var combined = $"{ticketId}:{activityId}:{DateTime.UtcNow.Ticks}";
            var combinedBytes = Encoding.UTF8.GetBytes(combined);
            return Convert.ToBase64String(combinedBytes);
        }

        /// <summary>
        /// 验证票根访问令牌
        /// </summary>
        /// <param name="token">访问令牌</param>
        /// <param name="ticketId">票根ID</param>
        /// <param name="activityId">活动ID</param>
        /// <param name="maxAgeMinutes">令牌最大有效期（分钟）</param>
        /// <returns>是否有效</returns>
        public static bool ValidateTicketToken(string token, long ticketId, long activityId, int maxAgeMinutes = 60)
        {
            try
            {
                var combinedBytes = Convert.FromBase64String(token);
                var combined = Encoding.UTF8.GetString(combinedBytes);
                var parts = combined.Split(':');

                if (parts.Length != 3)
                    return false;

                if (!long.TryParse(parts[0], out long tokenTicketId) || tokenTicketId != ticketId)
                    return false;

                if (!long.TryParse(parts[1], out long tokenActivityId) || tokenActivityId != activityId)
                    return false;

                if (!long.TryParse(parts[2], out long ticks))
                    return false;

                var tokenTime = new DateTime(ticks);
                var maxAge = TimeSpan.FromMinutes(maxAgeMinutes);

                return DateTime.UtcNow - tokenTime <= maxAge;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
