using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace Xinglin.WebReportEditor.Core.Data;

public static class TemplateSeedData
{
    public static void SeedTemplates(TemplateDbContext db, IConfiguration configuration)
    {
        SeedUsers(db, configuration);

        if (db.Templates.Any()) return;

        var now = DateTime.UtcNow;

        var templates = new[]
        {
            new TemplateEntity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "门诊病历首页",
                Type = "门诊",
                Version = 1,
                HospitalId = "H001",
                IsDefault = true,
                IsPublished = true,
                CreateTime = now,
                UpdateTime = now,
                CreatedBy = "admin",
                ContentJson = BuildOutpatientRecord()
            },
            new TemplateEntity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "住院病历首页",
                Type = "住院",
                Version = 1,
                HospitalId = "H001",
                IsDefault = false,
                IsPublished = true,
                CreateTime = now,
                UpdateTime = now,
                CreatedBy = "admin",
                ContentJson = BuildInpatientRecord()
            },
            new TemplateEntity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "检验报告单",
                Type = "检验",
                Version = 1,
                HospitalId = "H001",
                IsDefault = false,
                IsPublished = true,
                CreateTime = now,
                UpdateTime = now,
                CreatedBy = "admin",
                ContentJson = BuildLabReport()
            },
            new TemplateEntity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                Name = "处方笺",
                Type = "处方",
                Version = 1,
                HospitalId = "H001",
                IsDefault = false,
                IsPublished = true,
                CreateTime = now,
                UpdateTime = now,
                CreatedBy = "admin",
                ContentJson = BuildPrescription()
            },
            new TemplateEntity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                Name = "影像报告单",
                Type = "影像",
                Version = 1,
                HospitalId = "H001",
                IsDefault = false,
                IsPublished = false,
                CreateTime = now,
                UpdateTime = now,
                CreatedBy = "admin",
                ContentJson = BuildImagingReport()
            },
            new TemplateEntity
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                Name = "护理记录单",
                Type = "护理",
                Version = 1,
                HospitalId = "H001",
                IsDefault = false,
                IsPublished = false,
                CreateTime = now,
                UpdateTime = now,
                CreatedBy = "admin",
                ContentJson = BuildNursingRecord()
            }
        };

        db.Templates.AddRange(templates);
        db.SaveChanges();
    }

    private static JsonObject MakePage(string name, string type, int w, int h, string orient, int ml, int mr, int mt, int mb)
    {
        return new JsonObject
        {
            ["name"] = name,
            ["type"] = type,
            ["pageWidth"] = w,
            ["pageHeight"] = h,
            ["orientation"] = orient,
            ["marginLeft"] = ml,
            ["marginRight"] = mr,
            ["marginTop"] = mt,
            ["marginBottom"] = mb,
            ["backgroundColor"] = "#FFFFFF",
            ["globalFontSize"] = 12
        };
    }

    private static JsonArray MakeElements(params JsonObject[] elements) => new(elements);

    private static JsonObject TextEl(string id, double x, double y, double w, double h, string text, int fontSize = 11, string fontWeight = "normal", string textAlign = "left", string fontFamily = "SimSun", string fg = "#000000", string? label = null, string? dataPath = null)
    {
        var el = new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.text",
            ["x"] = x, ["y"] = y, ["width"] = w, ["height"] = h,
            ["isVisible"] = true,
            ["text"] = text,
            ["fontSize"] = fontSize,
            ["fontWeight"] = fontWeight,
            ["textAlignment"] = textAlign,
            ["fontFamily"] = fontFamily,
            ["foregroundColor"] = fg
        };
        if (!string.IsNullOrEmpty(label)) el["label"] = label;
        if (dataPath != null) el["dataPath"] = dataPath;
        return el;
    }

    private static JsonObject LineEl(string id, double x1, double y1, double x2, double y2, string color = "#000000", double width = 1, string style = "solid")
    {
        return new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.line",
            ["x"] = x1, ["y"] = y1, ["width"] = x2 - x1, ["height"] = 0,
            ["isVisible"] = true,
            ["startX"] = x1, ["startY"] = y1, ["endX"] = x2, ["endY"] = y2,
            ["lineColor"] = color,
            ["lineWidth"] = width,
            ["lineStyle"] = style
        };
    }

    private static JsonObject DropdownEl(string id, double x, double y, double w, double h, string dataPath, string[] options, string? label = null)
    {
        var arr = new JsonArray();
        foreach (var o in options) arr.Add(o);
        var el = new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.dropdown",
            ["x"] = x, ["y"] = y, ["width"] = w, ["height"] = h,
            ["isVisible"] = true,
            ["dataPath"] = dataPath,
            ["options"] = arr
        };
        if (!string.IsNullOrEmpty(label)) el["label"] = label;
        return el;
    }

    private static JsonObject NumberEl(string id, double x, double y, double w, double h, string dataPath, string unit = "", string? label = null)
    {
        var el = new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.number",
            ["x"] = x, ["y"] = y, ["width"] = w, ["height"] = h,
            ["isVisible"] = true,
            ["dataPath"] = dataPath
        };
        if (!string.IsNullOrEmpty(label)) el["label"] = label;
        if (!string.IsNullOrEmpty(unit)) el["unit"] = unit;
        return el;
    }

    private static JsonObject DateEl(string id, double x, double y, double w, double h, string dataPath, string format = "yyyy-MM-dd", string? label = null)
    {
        var el = new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.date",
            ["x"] = x, ["y"] = y, ["width"] = w, ["height"] = h,
            ["isVisible"] = true,
            ["dataPath"] = dataPath,
            ["format"] = format
        };
        if (!string.IsNullOrEmpty(label)) el["label"] = label;
        return el;
    }

    private static JsonObject TableEl(string id, double x, double y, double w, double h, int rows, int cols, bool hasHeader, string[][] cellData)
    {
        var dataArr = new JsonArray();
        foreach (var row in cellData)
        {
            var rowArr = new JsonArray();
            foreach (var cell in row) rowArr.Add(cell);
            dataArr.Add(rowArr);
        }
        return new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.table",
            ["x"] = x, ["y"] = y, ["width"] = w, ["height"] = h,
            ["isVisible"] = true,
            ["rows"] = rows, ["columns"] = cols,
            ["hasHeader"] = hasHeader,
            ["tableBorder"] = 1,
            ["cellData"] = dataArr
        };
    }

    private static JsonObject ImageEl(string id, double x, double y, double w, double h)
    {
        return new JsonObject
        {
            ["id"] = id,
            ["$type"] = "template.element.image",
            ["x"] = x, ["y"] = y, ["width"] = w, ["height"] = h,
            ["isVisible"] = true
        };
    }

    private static string BuildOutpatientRecord()
    {
        var doc = MakePage("门诊病历首页", "门诊", 210, 297, "Portrait", 15, 15, 15, 15);
        doc["elements"] = MakeElements(
            TextEl("op_1", 15, 15, 180, 14, "杏林医院 门诊病历", 18, "bold", "center", "SimHei"),
            LineEl("op_2", 15, 32, 195, 32, "#000000", 1.5),
            TextEl("op_3", 15, 38, 40, 10, "姓名：", 11, fg: "#333333"),
            TextEl("op_4", 55, 38, 40, 10, "", 11, label: "姓名", dataPath: "PatientName"),
            TextEl("op_5", 100, 38, 20, 10, "性别：", 11, fg: "#333333"),
            DropdownEl("op_6", 120, 38, 25, 10, "Gender", new[] { "男", "女" }, label: "性别"),
            TextEl("op_7", 150, 38, 20, 10, "年龄：", 11, fg: "#333333"),
            NumberEl("op_8", 170, 38, 25, 10, "Age", "岁", label: "年龄"),
            TextEl("op_9", 15, 52, 40, 10, "就诊科室：", 11, fg: "#333333"),
            DropdownEl("op_10", 55, 52, 50, 10, "Department", new[] { "内科", "外科", "儿科", "妇科", "骨科", "眼科" }, label: "科室"),
            TextEl("op_11", 115, 52, 30, 10, "日期：", 11, fg: "#333333"),
            DateEl("op_12", 145, 52, 50, 10, "VisitDate", label: "就诊日期"),
            LineEl("op_13", 15, 66, 195, 66, "#999999", 0.5, "dashed"),
            TextEl("op_14", 15, 72, 40, 10, "主诉：", 11, "bold", fg: "#333333"),
            TextEl("op_15", 15, 84, 180, 30, "", 11, label: "主诉", dataPath: "ChiefComplaint"),
            TextEl("op_16", 15, 120, 40, 10, "现病史：", 11, "bold", fg: "#333333"),
            TextEl("op_17", 15, 132, 180, 50, "", 11, label: "现病史", dataPath: "PresentIllness"),
            TextEl("op_18", 15, 190, 40, 10, "诊断：", 11, "bold", fg: "#333333"),
            TextEl("op_19", 15, 202, 180, 30, "", 11, label: "诊断", dataPath: "Diagnosis"),
            TextEl("op_20", 15, 240, 40, 10, "处理意见：", 11, "bold", fg: "#333333"),
            TextEl("op_21", 15, 252, 180, 30, "", 11, label: "处理意见", dataPath: "Treatment"),
            LineEl("op_22", 15, 285, 195, 285, "#000000", 0.5),
            TextEl("op_23", 120, 287, 30, 8, "医师签名：", 10, fg: "#666666"),
            TextEl("op_24", 150, 287, 45, 8, "", 10, label: "医师签名", dataPath: "DoctorName")
        );
        return doc.ToJsonString();
    }

    private static string BuildInpatientRecord()
    {
        var doc = MakePage("住院病历首页", "住院", 210, 297, "Portrait", 12, 12, 12, 12);
        doc["globalFontSize"] = 11;
        doc["elements"] = MakeElements(
            TextEl("ip_1", 12, 12, 186, 14, "杏林医院 住院病历首页", 16, "bold", "center", "SimHei"),
            LineEl("ip_2", 12, 28, 198, 28, "#000000", 1.5),
            TextEl("ip_3", 12, 34, 25, 9, "姓名：", 10),
            TextEl("ip_4", 37, 34, 35, 9, "", 10, label: "姓名", dataPath: "PatientName"),
            TextEl("ip_5", 78, 34, 25, 9, "性别：", 10),
            DropdownEl("ip_6", 103, 34, 20, 9, "Gender", new[] { "男", "女" }, label: "性别"),
            TextEl("ip_7", 130, 34, 25, 9, "年龄：", 10),
            NumberEl("ip_8", 155, 34, 20, 9, "Age", "岁", label: "年龄"),
            TextEl("ip_9", 12, 48, 25, 9, "科室：", 10),
            DropdownEl("ip_10", 37, 48, 40, 9, "Department", new[] { "内科", "外科", "儿科", "妇科", "骨科", "ICU" }, label: "科室"),
            TextEl("ip_11", 85, 48, 35, 9, "住院号：", 10),
            TextEl("ip_12", 120, 48, 35, 9, "", 10, label: "住院号", dataPath: "AdmissionNo"),
            TextEl("ip_13", 160, 48, 35, 9, "床号：", 10),
            TextEl("ip_14", 175, 48, 20, 9, "", 10, label: "床号", dataPath: "BedNo"),
            TextEl("ip_15", 12, 62, 35, 9, "入院日期：", 10),
            DateEl("ip_16", 47, 62, 45, 9, "AdmissionDate", label: "入院日期"),
            TextEl("ip_17", 100, 62, 35, 9, "出院日期：", 10),
            DateEl("ip_18", 135, 62, 45, 9, "DischargeDate", label: "出院日期"),
            LineEl("ip_19", 12, 76, 198, 76, "#999999", 0.5, "dashed"),
            TextEl("ip_20", 12, 82, 50, 9, "入院诊断：", 10, "bold"),
            TextEl("ip_21", 12, 94, 186, 30, "", 10, label: "入院诊断", dataPath: "AdmissionDiagnosis"),
            TextEl("ip_22", 12, 132, 50, 9, "出院诊断：", 10, "bold"),
            TableEl("ip_23", 12, 144, 186, 80, 5, 3, true, new[] { new[] { "序号", "诊断名称", "ICD-10编码" }, new[] { "1", "", "" }, new[] { "2", "", "" }, new[] { "3", "", "" }, new[] { "4", "", "" } }),
            TextEl("ip_24", 12, 232, 50, 9, "手术及操作：", 10, "bold"),
            TableEl("ip_25", 12, 244, 186, 40, 2, 4, true, new[] { new[] { "序号", "手术名称", "手术日期", "术者" }, new[] { "1", "", "", "" } })
        );
        return doc.ToJsonString();
    }

    private static string BuildLabReport()
    {
        var doc = MakePage("检验报告单", "检验", 210, 297, "Portrait", 15, 15, 12, 12);
        doc["globalFontSize"] = 11;
        doc["elements"] = MakeElements(
            TextEl("lab_1", 15, 12, 180, 14, "杏林医院 检验报告单", 16, "bold", "center", "SimHei"),
            LineEl("lab_2", 15, 28, 195, 28, "#000000", 1.5),
            TextEl("lab_3", 15, 34, 25, 9, "姓名：", 10),
            TextEl("lab_4", 40, 34, 35, 9, "", 10, label: "姓名", dataPath: "PatientName"),
            TextEl("lab_5", 80, 34, 25, 9, "性别：", 10),
            DropdownEl("lab_6", 105, 34, 20, 9, "Gender", new[] { "男", "女" }, label: "性别"),
            TextEl("lab_7", 130, 34, 25, 9, "年龄：", 10),
            NumberEl("lab_8", 155, 34, 20, 9, "Age", "岁", label: "年龄"),
            TextEl("lab_9", 15, 48, 35, 9, "样本类型：", 10),
            DropdownEl("lab_10", 50, 48, 35, 9, "SampleType", new[] { "血液", "尿液", "粪便", "其他" }, label: "样本类型"),
            TextEl("lab_11", 95, 48, 35, 9, "报告日期：", 10),
            DateEl("lab_12", 130, 48, 45, 9, "ReportDate", label: "报告日期"),
            LineEl("lab_13", 15, 62, 195, 62, "#999999", 0.5, "dashed"),
            TextEl("lab_14", 15, 68, 50, 10, "检验项目明细", 12, "bold"),
            TableEl("lab_15", 15, 82, 180, 140, 8, 5, true, new[] {
                new[] { "项目名称", "结果", "单位", "参考范围", "标志" },
                new[] { "白细胞(WBC)", "", "10^9/L", "3.5-9.5", "" },
                new[] { "红细胞(RBC)", "", "10^12/L", "4.3-5.8", "" },
                new[] { "血红蛋白(HGB)", "", "g/L", "130-175", "" },
                new[] { "血小板(PLT)", "", "10^9/L", "125-350", "" },
                new[] { "总胆红素(TBIL)", "", "umol/L", "3.4-17.1", "" },
                new[] { "白蛋白(ALB)", "", "g/L", "40-55", "" },
                new[] { "谷丙转氨酶(ALT)", "", "U/L", "9-50", "" }
            }),
            LineEl("lab_16", 15, 230, 195, 230, "#000000", 0.5),
            TextEl("lab_17", 15, 236, 180, 8, "备注：H表示偏高 L表示偏低 *表示危急值", 9, fg: "#999999"),
            TextEl("lab_18", 120, 280, 30, 8, "检验者：", 10),
            TextEl("lab_19", 150, 280, 40, 8, "", 10, label: "检验者", dataPath: "Technician"),
            TextEl("lab_20", 120, 290, 30, 8, "审核者：", 10),
            TextEl("lab_21", 150, 290, 40, 8, "", 10, label: "审核者", dataPath: "Reviewer")
        );
        return doc.ToJsonString();
    }

    private static string BuildPrescription()
    {
        var doc = MakePage("处方笺", "处方", 148, 210, "Portrait", 10, 10, 10, 10);
        doc["globalFontSize"] = 10;
        doc["elements"] = MakeElements(
            TextEl("rx_1", 10, 10, 128, 12, "杏林医院 处方笺", 14, "bold", "center", "SimHei"),
            LineEl("rx_2", 10, 24, 138, 24, "#CC0000", 2),
            TextEl("rx_3", 10, 30, 20, 8, "姓名：", 10),
            TextEl("rx_4", 30, 30, 30, 8, "", 10, label: "姓名", dataPath: "PatientName"),
            TextEl("rx_5", 65, 30, 15, 8, "性别：", 10),
            DropdownEl("rx_6", 80, 30, 15, 8, "Gender", new[] { "男", "女" }, label: "性别"),
            TextEl("rx_7", 100, 30, 15, 8, "年龄：", 10),
            NumberEl("rx_8", 115, 30, 20, 8, "Age", "岁", label: "年龄"),
            TextEl("rx_9", 10, 42, 25, 8, "临床诊断：", 10),
            TextEl("rx_10", 35, 42, 100, 8, "", 10, label: "临床诊断", dataPath: "Diagnosis"),
            TextEl("rx_11", 10, 54, 20, 8, "日期：", 10),
            DateEl("rx_12", 30, 54, 40, 8, "PrescriptionDate", label: "处方日期"),
            LineEl("rx_13", 10, 66, 138, 66, "#999999", 0.5, "dashed"),
            TextEl("rx_14", 10, 70, 30, 8, "Rp:", 12, "bold"),
            TableEl("rx_15", 10, 82, 128, 80, 5, 4, true, new[] { new[] { "药品名称", "规格", "用法用量", "数量" }, new[] { "", "", "", "" }, new[] { "", "", "", "" }, new[] { "", "", "", "" }, new[] { "", "", "", "" } }),
            LineEl("rx_16", 10, 170, 138, 170, "#CC0000", 1),
            TextEl("rx_17", 80, 175, 25, 8, "医师：", 10),
            TextEl("rx_18", 105, 175, 30, 8, "", 10, label: "医师", dataPath: "DoctorName"),
            TextEl("rx_19", 80, 188, 25, 8, "药师：", 10),
            TextEl("rx_20", 105, 188, 30, 8, "", 10, label: "药师", dataPath: "Pharmacist")
        );
        return doc.ToJsonString();
    }

    private static string BuildImagingReport()
    {
        var doc = MakePage("影像报告单", "影像", 210, 297, "Portrait", 15, 15, 12, 12);
        doc["globalFontSize"] = 11;
        doc["elements"] = MakeElements(
            TextEl("img_1", 15, 12, 180, 14, "杏林医院 影像诊断报告", 16, "bold", "center", "SimHei"),
            LineEl("img_2", 15, 28, 195, 28, "#000000", 1.5),
            TextEl("img_3", 15, 34, 25, 9, "姓名：", 10),
            TextEl("img_4", 40, 34, 35, 9, "", 10, label: "姓名", dataPath: "PatientName"),
            TextEl("img_5", 80, 34, 25, 9, "性别：", 10),
            DropdownEl("img_6", 105, 34, 20, 9, "Gender", new[] { "男", "女" }, label: "性别"),
            TextEl("img_7", 130, 34, 25, 9, "年龄：", 10),
            NumberEl("img_8", 155, 34, 20, 9, "Age", "岁", label: "年龄"),
            TextEl("img_9", 15, 48, 30, 9, "检查部位：", 10),
            DropdownEl("img_10", 45, 48, 40, 9, "BodyPart", new[] { "头部", "胸部", "腹部", "脊柱", "四肢" }, label: "检查部位"),
            TextEl("img_11", 95, 48, 30, 9, "检查方法：", 10),
            DropdownEl("img_12", 125, 48, 30, 9, "Modality", new[] { "X线", "CT", "MRI", "超声" }, label: "检查方法"),
            LineEl("img_13", 15, 62, 195, 62, "#999999", 0.5, "dashed"),
            TextEl("img_14", 15, 68, 40, 10, "影像所见：", 11, "bold"),
            TextEl("img_15", 15, 80, 180, 60, "", 11, label: "影像所见", dataPath: "Findings"),
            TextEl("img_16", 15, 150, 40, 10, "诊断意见：", 11, "bold"),
            TextEl("img_17", 15, 162, 180, 50, "", 11, label: "诊断意见", dataPath: "Impression"),
            ImageEl("img_18", 40, 220, 130, 50),
            LineEl("img_19", 15, 278, 195, 278, "#000000", 0.5),
            TextEl("img_20", 100, 282, 30, 8, "报告医师：", 10),
            TextEl("img_21", 130, 282, 40, 8, "", 10, label: "报告医师", dataPath: "ReportingDoctor")
        );
        return doc.ToJsonString();
    }

    private static string BuildNursingRecord()
    {
        var doc = MakePage("护理记录单", "护理", 297, 210, "Landscape", 10, 10, 10, 10);
        doc["globalFontSize"] = 10;
        doc["elements"] = MakeElements(
            TextEl("nr_1", 10, 10, 277, 12, "杏林医院 护理记录单", 14, "bold", "center", "SimHei"),
            LineEl("nr_2", 10, 24, 287, 24, "#000000", 1.5),
            TextEl("nr_3", 10, 30, 20, 8, "姓名：", 10),
            TextEl("nr_4", 30, 30, 30, 8, "", 10, label: "姓名", dataPath: "PatientName"),
            TextEl("nr_5", 70, 30, 20, 8, "科室：", 10),
            TextEl("nr_6", 90, 30, 30, 8, "", 10, label: "科室", dataPath: "Department"),
            TextEl("nr_7", 130, 30, 20, 8, "床号：", 10),
            TextEl("nr_8", 150, 30, 20, 8, "", 10, label: "床号", dataPath: "BedNo"),
            TextEl("nr_9", 180, 30, 25, 8, "住院号：", 10),
            TextEl("nr_10", 205, 30, 30, 8, "", 10, label: "住院号", dataPath: "AdmissionNo"),
            TextEl("nr_11", 245, 30, 20, 8, "日期：", 10),
            DateEl("nr_12", 265, 30, 30, 8, "RecordDate", label: "记录日期"),
            TableEl("nr_13", 10, 44, 277, 140, 8, 7, true, new[] {
                new[] { "时间", "体温", "脉搏(次/分)", "呼吸(次/分)", "血压(mmHg)", "护理措施", "签名" },
                new[] { "", "", "", "", "", "", "" },
                new[] { "", "", "", "", "", "", "" },
                new[] { "", "", "", "", "", "", "" },
                new[] { "", "", "", "", "", "", "" },
                new[] { "", "", "", "", "", "", "" },
                new[] { "", "", "", "", "", "", "" },
                new[] { "", "", "", "", "", "", "" }
            }),
            LineEl("nr_14", 10, 190, 287, 190, "#000000", 0.5),
            TextEl("nr_15", 200, 194, 25, 8, "护士签名：", 10),
            TextEl("nr_16", 225, 194, 30, 8, "", 10, label: "护士签名", dataPath: "NurseName")
        );
        return doc.ToJsonString();
    }

    private static void SeedUsers(TemplateDbContext db, IConfiguration configuration)
    {
        if (db.Users.Any(u => u.Username == "admin")) return;

        var now = DateTime.UtcNow;

        var adminUsername = configuration.GetValue<string>("AdminUser:Username") ?? "admin";
        var adminPassword = configuration.GetValue<string>("AdminUser:Password") ?? "admin123";
        var editorUsername = configuration.GetValue<string>("EditorUser:Username") ?? "editor";
        var editorPassword = configuration.GetValue<string>("EditorUser:Password") ?? "editor123";

        var adminUser = new UserEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Username = adminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            DisplayName = "系统管理员",
            Role = "admin",
            HospitalId = "H001",
            IsActive = true,
            CreateTime = now
        };

        var editorUser = new UserEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Username = editorUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(editorPassword),
            DisplayName = "模板编辑员",
            Role = "editor",
            HospitalId = "H001",
            IsActive = true,
            CreateTime = now
        };

        db.Users.AddRange(adminUser, editorUser);
        db.SaveChanges();
    }
}
