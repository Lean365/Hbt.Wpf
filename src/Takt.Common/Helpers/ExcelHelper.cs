//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : ExcelHelper.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-30
// 版本号 : 0.0.1
// 描述    : 基于 EPPlus 的通用 Excel 导入导出帮助类
//===================================================================

using System.ComponentModel;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Takt.Common.Helpers;

public static class ExcelHelper
{
    /// <summary>
    /// 单个导入文件允许的最大数据行数（不含表头）
    /// </summary>
    public const int MaxImportRows = 1000;

    /// <summary>
    /// 单个工作表最多记录数（不含表头）
    /// </summary>
    public     const int MaxRowsPerSheet = 5000;
    static ExcelHelper()
    {
#pragma warning disable CS0618 // EPPlus 8.2.1 中 LicenseContext 已过时，但 License 属性是只读的，因此继续使用 LicenseContext
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
#pragma warning restore CS0618
    }

    /// <summary>
    /// 导出为 Excel（返回字节数组）
    /// </summary>
    public static byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1")
    {
        using var package = new ExcelPackage();
        var list = data.ToList();
        var props = GetExportProperties(typeof(T));

        // 分 sheet 导出，每个 sheet 最多 MaxRowsPerSheet 条
        int total = list.Count;
        int sheetIndex = 0;
        for (int offset = 0; offset < total; offset += MaxRowsPerSheet)
        {
            var ws = package.Workbook.Worksheets.Add(sheetIndex == 0 ? sheetName : $"{sheetName}_{sheetIndex + 1}");

            // 头：第一行 ColumnDescription，第二行 属性名
            for (int i = 0; i < props.Count; i++)
            {
                var desc = GetColumnDescription(props[i]) ?? GetDisplayName(props[i]) ?? props[i].Name;
                ws.Cells[1, i + 1].Value = desc;
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240,240,240));

                ws.Cells[2, i + 1].Value = props[i].Name;
                ws.Cells[2, i + 1].Style.Font.Bold = true;
                ws.Cells[2, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[2, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(250,250,250));
            }

            // 数据（从第3行开始）
            int row = 3;
            foreach (var item in list.Skip(offset).Take(MaxRowsPerSheet))
            {
                for (int col = 0; col < props.Count; col++)
                {
                    var val = props[col].GetValue(item);
                    ws.Cells[row, col + 1].Value = val;
                }
                row++;
            }

            ws.Cells.AutoFitColumns();
            sheetIndex++;
        }

        return package.GetAsByteArray();
    }

    /// <summary>
    /// 从 Excel 导入为对象集合（首行作为表头）
    /// </summary>
    public static List<T> ImportFromExcel<T>(Stream excelStream, string sheetName = "Sheet1") where T : new()
    {
        using var package = new ExcelPackage(excelStream);
        var ws = package.Workbook.Worksheets.FirstOrDefault(x => x.Name == sheetName)
                 ?? package.Workbook.Worksheets.First();

        var props = GetExportProperties(typeof(T));
        // 第二行是属性名作为绑定依据
        var headerToProp = props.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var result = new List<T>();
        int colCount = ws.Dimension.End.Column;
        int rowCount = ws.Dimension.End.Row;

        // 限制导入总量（不含两行表头）
        int dataRows = Math.Max(0, rowCount - 2);
        if (dataRows > MaxImportRows)
        {
            throw new InvalidOperationException($"导入数据行数 {dataRows} 超出上限 {MaxImportRows}，请分批导入。");
        }

        // 构建列映射（读取第2行的属性名）
        var colToProp = new Dictionary<int, PropertyInfo>();
        for (int c = 1; c <= colCount; c++)
        {
            var header = ws.Cells[2, c].Text?.Trim();
            if (string.IsNullOrEmpty(header)) continue;
            if (headerToProp.TryGetValue(header, out var prop))
                colToProp[c] = prop;
        }

        // 数据从第3行开始
        for (int r = 3; r <= rowCount; r++)
        {
            var item = new T();
            foreach (var kv in colToProp)
            {
                var text = ws.Cells[r, kv.Key].Text;
                if (string.IsNullOrEmpty(text)) continue;
                object? converted = ConvertTo(text, kv.Value.PropertyType);
                kv.Value.SetValue(item, converted);
            }
            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// 导出空模板（只有表头，无数据）
    /// </summary>
    public static byte[] ExportTemplate<T>(string sheetName = "Sheet1", IEnumerable<string>? headers = null)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add(sheetName);

        var props = GetExportProperties(typeof(T));
        var headerList = headers?.ToList();

        for (int i = 0; i < props.Count; i++)
        {
            // 第一行：ColumnDescription/显示名，第二行：属性名
            var header = headerList != null && i < headerList.Count
                ? headerList[i]
                : (GetColumnDescription(props[i]) ?? GetDisplayName(props[i]) ?? props[i].Name);
            ws.Cells[1, i + 1].Value = header;
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240,240,240));

            ws.Cells[2, i + 1].Value = props[i].Name;
            ws.Cells[2, i + 1].Style.Font.Bold = true;
            ws.Cells[2, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[2, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(250,250,250));
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    private static object? ConvertTo(string text, Type targetType)
    {
        if (targetType == typeof(string)) return text;
        if (targetType == typeof(int) || targetType == typeof(int?))
            return int.TryParse(text, out var i) ? i : null;
        if (targetType == typeof(long) || targetType == typeof(long?))
            return long.TryParse(text, out var l) ? l : null;
        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            return DateTime.TryParse(text, out var d) ? d : null;
        if (targetType.IsEnum)
        {
            if (int.TryParse(text, out var ei)) return Enum.ToObject(targetType, ei);
            if (Enum.TryParse(targetType, text, true, out var ev)) return ev;
        }
        return text;
    }

    private static List<PropertyInfo> GetExportProperties(Type t)
    {
        return t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();
    }

    private static string? GetDisplayName(PropertyInfo p)
    {
        var dn = p.GetCustomAttribute<DisplayNameAttribute>();
        return dn?.DisplayName;
    }

    private static string? GetColumnDescription(PropertyInfo p)
    {
        // 兼容 SqlSugar 的 [SugarColumn(ColumnDescription=..)]，避免强依赖，用反射读取
        var attrs = p.GetCustomAttributes(true);
        foreach (var a in attrs)
        {
            var t = a.GetType();
            if (t.Name == "SugarColumn" || t.Name == "SugarColumnAttribute")
            {
                var prop = t.GetProperty("ColumnDescription");
                var val = prop?.GetValue(a) as string;
                if (!string.IsNullOrWhiteSpace(val)) return val;
            }
        }
        // 兼容 DescriptionAttribute
        var da = p.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return da?.Description;
    }
}


