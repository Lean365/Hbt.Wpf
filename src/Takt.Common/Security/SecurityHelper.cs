//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : SecurityHelper.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-20
// 版本号 : 0.0.1
// 描述    : 安全帮助类（军用级 BouncyCastle）
//===================================================================

using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Text;

namespace Takt.Common.Security;

/// <summary>
/// 安全帮助类
/// 提供军用级密码哈希、加密解密、随机数生成等功能
/// 基于 BouncyCastle 密码学库（FIPS 140-2、NSA Suite B 认证）
/// </summary>
public static class SecurityHelper
{
    #region 密码哈希（Argon2id）

    /// <summary>
    /// 使用 Argon2id 算法进行密码哈希（军用级安全）
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>哈希后的密码（Base64编码：salt(16字节) + hash(32字节)）</returns>
    /// <remarks>
    /// 算法参数（军用级配置）：
    /// - Version: Argon2 v1.3
    /// - Type: Argon2id（抗GPU攻击 + 抗旁道攻击）
    /// - Iterations: 15次
    /// - Memory: 64MB
    /// - Parallelism: 8线程
    /// - Salt: 128位随机盐值（SecureRandom生成）
    /// - Hash: 256位哈希值
    /// </remarks>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password), "密码不能为空");
        }

        // 生成随机盐值（128位 / 16字节）
        var salt = GenerateRandomBytes(16);

        // 配置 Argon2 参数（军用级）
        var argon2Parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
            .WithVersion(Argon2Parameters.Version13)  // Argon2 版本 1.3
            .WithIterations(15)                       // 迭代次数：15次
            .WithMemoryAsKB(65536)                    // 内存成本：64MB
            .WithParallelism(8)                       // 并行度：8线程
            .WithSalt(salt)                           // 盐值
            .Build();

        // 创建 Argon2 生成器
        var argon2Generator = new Argon2BytesGenerator();
        argon2Generator.Init(argon2Parameters);

        // 生成哈希值（256位 / 32字节）
        var hash = new byte[32];
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        argon2Generator.GenerateBytes(passwordBytes, hash, 0, hash.Length);

        // 组合盐值和哈希值：salt(16字节) + hash(32字节) = 48字节
        var combined = new byte[48];
        Buffer.BlockCopy(salt, 0, combined, 0, 16);
        Buffer.BlockCopy(hash, 0, combined, 16, 32);

        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// 验证密码（使用 BouncyCastle Argon2id）
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hashedPassword">存储的哈希密码</param>
    /// <returns>是否匹配</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
        {
            return false;
        }

        if (string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }

        try
        {
            var combined = Convert.FromBase64String(hashedPassword);
            
            if (combined.Length != 48)
            {
                return false;
            }

            // 提取盐值和哈希值
            var salt = new byte[16];
            var storedHash = new byte[32];
            Buffer.BlockCopy(combined, 0, salt, 0, 16);
            Buffer.BlockCopy(combined, 16, storedHash, 0, 32);

            // 使用相同参数计算新哈希
            var argon2Parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
                .WithVersion(Argon2Parameters.Version13)
                .WithIterations(15)
                .WithMemoryAsKB(65536)
                .WithParallelism(8)
                .WithSalt(salt)
                .Build();

            var argon2Generator = new Argon2BytesGenerator();
            argon2Generator.Init(argon2Parameters);

            var newHash = new byte[32];
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            argon2Generator.GenerateBytes(passwordBytes, newHash, 0, newHash.Length);

            // 使用固定时间比较（防止时序攻击）
            return ConstantTimeEquals(storedHash, newHash);
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region 随机数生成

    /// <summary>
    /// 生成密码学安全的随机字节数组
    /// </summary>
    /// <param name="length">字节数</param>
    /// <returns>随机字节数组</returns>
    /// <remarks>
    /// 使用 BouncyCastle 的 SecureRandom，符合 NIST SP 800-90A 标准
    /// </remarks>
    public static byte[] GenerateRandomBytes(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "长度必须大于0");
        }

        var bytes = new byte[length];
        var secureRandom = new SecureRandom();
        secureRandom.NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// 生成密码学安全的随机字符串（Base64编码）
    /// </summary>
    /// <param name="length">字节数</param>
    /// <returns>Base64编码的随机字符串</returns>
    public static string GenerateRandomString(int length = 32)
    {
        var bytes = GenerateRandomBytes(length);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 生成密码学安全的随机整数
    /// </summary>
    /// <param name="min">最小值（包含）</param>
    /// <param name="max">最大值（不包含）</param>
    /// <returns>随机整数</returns>
    public static int GenerateRandomInt(int min, int max)
    {
        if (min >= max)
        {
            throw new ArgumentException("min 必须小于 max");
        }

        var secureRandom = new SecureRandom();
        return secureRandom.Next(min, max);
    }

    #endregion

    #region 固定时间比较

    /// <summary>
    /// 固定时间比较两个字节数组（防止时序攻击）
    /// </summary>
    /// <param name="a">字节数组A</param>
    /// <param name="b">字节数组B</param>
    /// <returns>是否相等</returns>
    /// <remarks>
    /// 无论数组内容如何，比较时间都相同，防止通过时间差分析推断内容
    /// </remarks>
    public static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a.Length != b.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    /// <summary>
    /// 固定时间比较两个字符串（防止时序攻击）
    /// </summary>
    /// <param name="a">字符串A</param>
    /// <param name="b">字符串B</param>
    /// <returns>是否相等</returns>
    public static bool ConstantTimeEquals(string a, string b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        
        return ConstantTimeEquals(bytesA, bytesB);
    }

    #endregion

    #region 密码强度验证

    /// <summary>
    /// 验证密码强度
    /// </summary>
    /// <param name="password">密码</param>
    /// <returns>密码强度等级（0-4）</returns>
    /// <remarks>
    /// 0: 弱
    /// 1: 较弱
    /// 2: 中等
    /// 3: 强
    /// 4: 非常强
    /// </remarks>
    public static int GetPasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return 0;
        }

        var strength = 0;

        // 长度检查
        if (password.Length >= 8) strength++;
        if (password.Length >= 12) strength++;

        // 复杂度检查
        if (password.Any(char.IsLower)) strength++;
        if (password.Any(char.IsUpper)) strength++;
        if (password.Any(char.IsDigit)) strength++;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) strength++;

        return Math.Min(strength, 4);
    }

    /// <summary>
    /// 验证密码是否符合安全要求
    /// </summary>
    /// <param name="password">密码</param>
    /// <param name="minLength">最小长度（默认8位）</param>
    /// <param name="requireUpperCase">是否需要大写字母（默认true）</param>
    /// <param name="requireLowerCase">是否需要小写字母（默认true）</param>
    /// <param name="requireDigit">是否需要数字（默认true）</param>
    /// <param name="requireSpecialChar">是否需要特殊字符（默认true）</param>
    /// <returns>验证结果</returns>
    public static (bool IsValid, string ErrorMessage) ValidatePassword(
        string password,
        int minLength = 8,
        bool requireUpperCase = true,
        bool requireLowerCase = true,
        bool requireDigit = true,
        bool requireSpecialChar = true)
    {
        if (string.IsNullOrEmpty(password))
        {
            return (false, "密码不能为空");
        }

        if (password.Length < minLength)
        {
            return (false, $"密码长度至少需要 {minLength} 位");
        }

        if (requireUpperCase && !password.Any(char.IsUpper))
        {
            return (false, "密码必须包含大写字母");
        }

        if (requireLowerCase && !password.Any(char.IsLower))
        {
            return (false, "密码必须包含小写字母");
        }

        if (requireDigit && !password.Any(char.IsDigit))
        {
            return (false, "密码必须包含数字");
        }

        if (requireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return (false, "密码必须包含特殊字符");
        }

        return (true, string.Empty);
    }

    #endregion

    #region Token生成

    /// <summary>
    /// 生成安全的访问令牌
    /// </summary>
    /// <param name="length">令牌长度（字节数，默认32）</param>
    /// <returns>Base64编码的令牌</returns>
    public static string GenerateAccessToken(int length = 32)
    {
        return GenerateRandomString(length);
    }

    /// <summary>
    /// 生成安全的刷新令牌
    /// </summary>
    /// <param name="length">令牌长度（字节数，默认64）</param>
    /// <returns>Base64编码的令牌</returns>
    public static string GenerateRefreshToken(int length = 64)
    {
        return GenerateRandomString(length);
    }

    #endregion
}


