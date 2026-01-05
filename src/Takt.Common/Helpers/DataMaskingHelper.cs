// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Common.Helpers
// 文件名称：DataMaskingHelper.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数据脱敏工具类（统一脱敏处理）
//
// 版权信息：Copyright (c) 2025 Takt SMEs Platform. All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Takt.Common.Helpers;

/// <summary>
/// 数据脱敏工具类
/// 统一处理所有敏感数据的脱敏，避免重复代码
/// </summary>
public static class DataMaskingHelper
{
    /// <summary>
    /// 对敏感字段进行脱敏处理
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="value">字段值</param>
    /// <returns>脱敏后的值</returns>
    public static string? MaskSensitiveField(string fieldName, string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // 定义敏感字段名称（不区分大小写）
        var sensitiveFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "pwd",
            "passwd",
            "secret",
            "token",
            "access_token",
            "refresh_token",
            "api_key",
            "apikey",
            "private_key",
            "privatekey",
            "secret_key",
            "secretkey",
            "auth_token",
            "authtoken"
        };

        // 检查字段名是否包含敏感字段关键词
        var fieldNameLower = fieldName.ToLower();
        if (sensitiveFields.Contains(fieldNameLower) || 
            fieldNameLower.Contains("password") || 
            fieldNameLower.Contains("pwd") ||
            fieldNameLower.Contains("secret") ||
            fieldNameLower.Contains("token") ||
            fieldNameLower.Contains("key"))
        {
            // 对密码等敏感字段进行脱敏：保留前2位和后2位，中间用*替代
            if (value.Length <= 4)
            {
                return "****";  // 如果长度小于等于4，全部用*替代
            }
            else if (value.Length <= 6)
            {
                return value.Substring(0, 1) + "****" + value.Substring(value.Length - 1);
            }
            else
            {
                return value.Substring(0, 2) + "****" + value.Substring(value.Length - 2);
            }
        }

        // 对邮箱进行部分脱敏：保留@前面的前2位和后1位，@后面保留域名
        if (fieldNameLower.Contains("email") && value.Contains("@"))
        {
            var parts = value.Split('@');
            if (parts.Length == 2 && parts[0].Length > 3)
            {
                var emailPrefix = parts[0];
                var emailDomain = parts[1];
                var maskedPrefix = emailPrefix.Substring(0, 2) + "***" + emailPrefix.Substring(emailPrefix.Length - 1);
                return $"{maskedPrefix}@{emailDomain}";
            }
        }

        // 对手机号进行脱敏：保留前3位和后4位，中间用*替代
        if (fieldNameLower.Contains("phone") || fieldNameLower.Contains("mobile") || fieldNameLower.Contains("tel"))
        {
            if (value.Length >= 7)
            {
                return value.Substring(0, 3) + "****" + value.Substring(value.Length - 4);
            }
            else if (value.Length >= 4)
            {
                return value.Substring(0, 1) + "****" + value.Substring(value.Length - 1);
            }
        }

        // 其他字段不脱敏
        return value;
    }

    /// <summary>
    /// 对 JSON 字符串中的敏感字段进行脱敏处理
    /// </summary>
    /// <param name="jsonString">JSON 字符串</param>
    /// <returns>脱敏后的 JSON 字符串</returns>
    public static string? MaskSensitiveJson(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return jsonString;

        try
        {
            var token = JToken.Parse(jsonString);
            MaskSensitiveToken(token);
            
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.Default
            };
            return JsonConvert.SerializeObject(token, settings);
        }
        catch
        {
            // 如果解析失败，返回原始字符串
            return jsonString;
        }
    }

    /// <summary>
    /// 递归处理 JToken，对敏感字段进行脱敏
    /// </summary>
    private static void MaskSensitiveToken(JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            foreach (var property in obj.Properties().ToList())
            {
                if (property.Value.Type == JTokenType.String)
                {
                    var stringValue = property.Value.ToString();
                    property.Value = MaskSensitiveField(property.Name, stringValue);
                }
                else if (property.Value.Type == JTokenType.Object || property.Value.Type == JTokenType.Array)
                {
                    // 递归处理嵌套对象和数组
                    MaskSensitiveToken(property.Value);
                }
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            var array = (JArray)token;
            foreach (var item in array)
            {
                if (item.Type == JTokenType.Object || item.Type == JTokenType.Array)
                {
                    MaskSensitiveToken(item);
                }
            }
        }
    }
}

