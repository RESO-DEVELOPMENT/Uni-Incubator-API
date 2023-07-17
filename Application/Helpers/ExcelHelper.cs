using Application.Domain.Enums.ProjectMember;
using Application.Domain.Models;
using Application.Domain;
using Application.DTOs.MemberExport;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.DataValidation;
using AutoMapper.Execution;
using Member = Application.Domain.Models.Member;

namespace Application.Helpers
{
    public static class ExcelHelper
    {
        public static ExcelPackage GetMemberCertificate(Member memberFull)
        {
            MemberExportDTO result = new();

            var totalHour = 0;

            Dictionary<Guid, List<ProjectReportMemberTask>> projectIdAndTasks = new();

            var projectMembers = memberFull.ProjectMembers;

            projectMembers.ForEach(pm =>
            {
                var project = pm.Project;
                if (!projectIdAndTasks.ContainsKey(project.ProjectId))
                {
                    projectIdAndTasks.Add(project.ProjectId, new List<ProjectReportMemberTask>());
                }

                var memberReports = pm.ProjectMemberReports;
                memberReports.ForEach(mr =>
                {
                    var tasks = mr.ProjectReportMemberTasks;
                    projectIdAndTasks[project.ProjectId].AddRange(tasks);
                });
            });

            projectIdAndTasks.Keys.ToList().ForEach(projectId =>
            {
                var projectResult = new MemberExportDTO_Project();

                var project = projectMembers.First(x => x.ProjectId == projectId).Project;
                var tasks = projectIdAndTasks[projectId];

                projectResult.ProjectName = project.ProjectName;
                projectResult.ProjectDescription = project.ProjectLongDescription;
                projectResult.ProjectRole = project.ProjectMember.First().Role == ProjectMemberRole.Manager ? "Quản Lý" : "Thành Viên";
                projectResult.ProjectShortName = project.ProjectShortName;
                projectResult.Status = project.ProjectStatus;

                tasks.ForEach(task =>
                {
                    projectResult.Tasks.Add(new MemberExportDTO_ProjectTask
                    {
                        TaskName = task.TaskName,
                        TaskDescription = task.TaskDescription
                    });

                    projectResult.TotalTaskDone += 1;
                    projectResult.TotalWorkHours += (int)task.TaskHour;
                });
                result.Projects.Add(projectResult);
            });

            result.MemberEmail = memberFull.EmailAddress;
            result.MemberName = memberFull.FullName;

            // Export
            ExcelPackage excel = new ExcelPackage();
            var memberWs = excel.Workbook.Worksheets.Add("Chứng nhận kinh nghiệm làm việc");

            memberWs.SelectedRange[1, 1, 1, 2].Merge = true;

            memberWs.SelectedRange[1, 3, 1, 6].Merge = true;
            try
            {
                //WebClient webClient = new WebClient();
                //var st = webClient.OpenRead("https://i.imgur.com/W8bp7Pn.png");
                //st.Position = 0;

                //var image = Bitmap.FromFile("./uniLogo.png");
                var imageFile = File.OpenRead("./uniLogo.png");
                var picture = memberWs.Drawings.AddPicture("Logo", imageFile);

                picture.SetPosition(0, 25);
                picture.SetSize(250, 50);
            }
            catch (Exception ex)
            {
                memberWs.Cells[1, 1].Value = "UNICARE";

            }


            memberWs.Cells[1, 3].Value = "CHỨNG NHẬN KINH NGHIỆM LÀM VIỆC";
            memberWs.Cells[1, 3].Style.Font.Bold = true;
            memberWs.Cells[1, 3].Style.Font.Size = 25;


            memberWs.Row(1).Height = 40;
            memberWs.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            memberWs.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;


            //memberWs.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //memberWs.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

            memberWs.Column(1).Width = 10;
            memberWs.Column(2).Width = 30;
            memberWs.Column(3).Width = 40;
            memberWs.Column(4).Width = 20;
            memberWs.Column(5).Width = 30;
            memberWs.Column(6).Width = 25;

            // BOLD LEFT
            memberWs.SelectedRange[2, 1, 5, 1].Style.Font.Bold = true;

            // NAME
            memberWs.SelectedRange[2, 1, 2, 2].Merge = true;
            memberWs.SelectedRange[2, 3, 2, 6].Merge = true;
            memberWs.Cells[2, 1].Value = "Họ Và Tên";
            memberWs.Cells[2, 3].Value = memberFull.FullName;
            memberWs.Cells[2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // EMAIL
            memberWs.SelectedRange[3, 1, 3, 2].Merge = true;
            memberWs.SelectedRange[3, 3, 3, 6].Merge = true;
            memberWs.Cells[3, 1].Value = "Email";
            memberWs.Cells[3, 3].Value = memberFull.EmailAddress;
            memberWs.Cells[3, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // SDT
            memberWs.SelectedRange[4, 1, 4, 2].Merge = true;
            memberWs.SelectedRange[4, 3, 4, 6].Merge = true;
            memberWs.Cells[4, 1].Value = "SĐT";
            memberWs.Cells[4, 3].Value = memberFull.PhoneNumber;
            memberWs.Cells[4, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


            //memberWs.SelectedRange[5, 2, 5, 3].Merge = true;
            //memberWs.Cells[5, 1].Value = "Total Work Hours";
            //memberWs.Cells[5, 2].Value = result.Projects.Sum(x => x.TotalWorkHours);
            //memberWs.Cells[5, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // EXPORT DATE
            memberWs.SelectedRange[5, 1, 5, 2].Merge = true;
            memberWs.SelectedRange[5, 3, 5, 6].Merge = true;
            memberWs.Cells[5, 1].Value = "Ngày xuất dữ liệu";
            memberWs.Cells[5, 3].Value = DateTimeHelper.Now().ToString("MM/dd/yy");
            memberWs.Cells[5, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            memberWs.SelectedRange[1, 1, 5, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[1, 1, 5, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[1, 1, 5, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[1, 1, 5, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;


            memberWs.SelectedRange[6, 1, 6, 6].Merge = true;
            memberWs.Cells[6, 1].Value = "BÁO CÁO CHI TIẾT KINH NGHIỆM LÀM VIỆC CỦA THÀNH VIÊN TẠI UNI INCUBATOR";
            memberWs.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            memberWs.Cells[6, 1].Style.Font.Bold = true;
            memberWs.Row(6).Height = 20;

            memberWs.SelectedRange[7, 1, 7, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[7, 1, 7, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[7, 1, 7, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[7, 1, 7, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            memberWs.SelectedRange[7, 1, 7, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
            memberWs.SelectedRange[7, 1, 7, 6].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
            memberWs.SelectedRange[7, 1, 7, 6].Style.Font.Bold = true;

            memberWs.Cells[7, 1].Value = "STT";
            memberWs.Cells[7, 2].Value = "Tên dự án";
            memberWs.Cells[7, 3].Value = "Mô Tả";
            memberWs.Cells[7, 4].Value = "Vai Trò";
            memberWs.Cells[7, 5].Value = "Số Task Hoàn Thành";
            memberWs.Cells[7, 6].Value = "Tổng giờ làm";

            for (var i = 0; i < result.Projects.Count(); i++)
            {
                var project = result.Projects[i];

                memberWs.Cells[8 + i, 1].Value = i + 1;
                memberWs.Cells[8 + i, 2].Value = project.ProjectName;
                memberWs.Cells[8 + i, 3].Value = project.ProjectDescription;
                memberWs.Cells[8 + i, 4].Value = project.ProjectRole;
                memberWs.Cells[8 + i, 5].Value = project.TotalTaskDone;
                memberWs.Cells[8 + i, 6].Value = project.TotalWorkHours;

                memberWs.Cells[8 + i, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                memberWs.Cells[8 + i, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                memberWs.SelectedRange[8 + i, 2, 8 + i, 3].Style.WrapText = true;

                memberWs.SelectedRange[8 + i, 4, 8 + i, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                memberWs.SelectedRange[8 + i, 4, 8 + i, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                memberWs.SelectedRange[8 + i, 1, 8 + i, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                memberWs.SelectedRange[8 + i, 1, 8 + i, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                memberWs.SelectedRange[8 + i, 1, 8 + i, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                memberWs.SelectedRange[8 + i, 1, 8 + i, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // Task Worksheet
            var taskWs = excel.Workbook.Worksheets.Add("Chi Tiết Công Việc");

            taskWs.SelectedRange[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            taskWs.SelectedRange[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            taskWs.SelectedRange[1, 1, 1, 3].Style.Font.Bold = true;
            taskWs.Cells[1, 1].Value = "Tên Dự Án";
            taskWs.Cells[1, 2].Value = "Tên CÔng Việc";
            taskWs.Cells[1, 3].Value = "Mô Tả";

            taskWs.Column(1).Width = 50;
            taskWs.Column(2).Width = 40;
            taskWs.Column(3).Width = 60;

            var currentRow = 2;

            result.Projects.ForEach(project =>
            {
                taskWs.Cells[currentRow, 1].Value = project.ProjectName;
                taskWs.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var additionRows = project.TotalTaskDone <= 1 ? 0 : project.TotalTaskDone - 1;

                for (int j = 0; j < project.Tasks.Count; j++)
                {
                    var task = project.Tasks[j];
                    taskWs.Cells[j + currentRow, 2].Value = task.TaskName;
                    taskWs.Cells[j + currentRow, 3].Value = task.TaskDescription;
                }

                taskWs.SelectedRange[currentRow, 1, currentRow + additionRows, 1].Merge = true;
                currentRow += 1 + additionRows;

            });

            return excel;
        }

        public static ExcelPackage GetReportTemplate(Project projectWithMember, SalaryCycle salaryCycle)
        {
            var projectMembers = projectWithMember.ProjectMember.ToList();
            var pm = projectMembers.First(x => x.Role == ProjectMemberRole.Manager);
            // Export
            ExcelPackage excel = new ExcelPackage();
            var memberWs = excel.Workbook.Worksheets.Add("Bảng Báo Cáo Công Việc");

            memberWs.SelectedRange[1, 1, 1, 2].Merge = true;
            memberWs.SelectedRange[1, 3, 1, 9].Merge = true;
            try
            {
                //WebClient webClient = new WebClient();
                //var st = webClient.OpenRead("https://i.imgur.com/W8bp7Pn.png");
                //st.Position = 0;

                //var image = Bitmap.FromFile("./uniLogo.png");
                var imageFile = File.OpenRead("./uniLogo.png");
                var picture = memberWs.Drawings.AddPicture("Logo", imageFile);

                picture.SetPosition(0, 25);
                picture.SetSize(250, 50);
            }
            catch (Exception ex)
            {
                memberWs.Cells[1, 1].Value = "UNICARE";

            }


            memberWs.Cells[1, 3].Value = "BẢNG BÁO CÁO CÔNG VIỆC";
            memberWs.Cells[1, 3].Style.Font.Bold = true;
            memberWs.Cells[1, 3].Style.Font.Size = 30;


            memberWs.Row(1).Height = 40;
            memberWs.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            memberWs.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;


            //memberWs.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //memberWs.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

            memberWs.Column(1).Width = 10;
            memberWs.Column(2).Width = 30;
            memberWs.Column(3).Width = 25;
            memberWs.Column(4).Width = 20;
            memberWs.Column(5).Width = 20;
            memberWs.Column(6).Width = 25;
            memberWs.Column(7).Width = 25;
            memberWs.Column(8).Width = 15;
            memberWs.Column(9).Width = 15;

            // BOLD TITLE
            memberWs.SelectedRange[2, 1, 4, 1].Style.Font.Bold = true;
            memberWs.SelectedRange[2, 5, 4, 5].Style.Font.Bold = true;

            // NAME
            memberWs.SelectedRange[2, 1, 2, 2].Merge = true;
            memberWs.SelectedRange[2, 3, 2, 4].Merge = true;
            memberWs.Cells[2, 1].Value = "Tên Dự Án";
            memberWs.Cells[2, 3].Value = projectWithMember.ProjectName;
            memberWs.Cells[2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // SHORTNAME
            memberWs.SelectedRange[3, 1, 3, 2].Merge = true;
            memberWs.SelectedRange[3, 3, 3, 4].Merge = true;
            memberWs.Cells[3, 1].Value = "Mã dự án (Tên viết tắt)";
            memberWs.Cells[3, 3].Value = projectWithMember.ProjectShortName;
            memberWs.Cells[3, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Salary Cycle
            memberWs.SelectedRange[4, 1, 4, 2].Merge = true;
            memberWs.SelectedRange[4, 3, 4, 4].Merge = true;
            memberWs.Cells[4, 1].Value = "Kỳ Lương";
            memberWs.Cells[4, 3].Value = salaryCycle.Name;
            memberWs.Cells[4, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // PM
            memberWs.SelectedRange[2, 5, 2, 6].Merge = true;
            memberWs.SelectedRange[2, 7, 2, 9].Merge = true;
            memberWs.Cells[2, 5].Value = "Quản lý dự án";
            memberWs.Cells[2, 7].Value = pm.Member.EmailAddress;
            memberWs.Cells[2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // EXPORT DATE
            memberWs.SelectedRange[3, 5, 3, 6].Merge = true;
            memberWs.SelectedRange[3, 7, 3, 9].Merge = true;
            memberWs.Cells[3, 5].Value = "Ngày lập báo cáo";
            memberWs.Cells[3, 7].Value = DateTimeHelper.Now().ToString("MM/dd/yy");
            memberWs.Cells[3, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Salary Cycle DATE
            memberWs.SelectedRange[4, 5, 4, 6].Merge = true;
            memberWs.SelectedRange[4, 7, 4, 9].Merge = true;
            memberWs.Cells[4, 5].Value = "Ngày bắt đầu kỳ lương";
            memberWs.Cells[4, 7].Value = salaryCycle.StartedAt.ToString("MM/dd/yy");
            memberWs.Cells[4, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            memberWs.SelectedRange[1, 1, 4, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[1, 1, 4, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[1, 1, 4, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[1, 1, 4, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;


            memberWs.SelectedRange[6, 1, 6, 8].Merge = true;
            memberWs.Cells[6, 1].Value = "BÁO CÁO CHI TIẾT LƯƠNG THƯỞNG CỦA THÀNH VIÊN THAM GIA";
            memberWs.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            memberWs.Cells[6, 1].Style.Font.Bold = true;
            memberWs.Row(6).Height = 20;

            memberWs.SelectedRange[7, 1, 7, 8].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[7, 1, 7, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[7, 1, 7, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[7, 1, 7, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            memberWs.SelectedRange[7, 1, 7, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            memberWs.SelectedRange[7, 1, 7, 8].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
            memberWs.SelectedRange[7, 1, 7, 8].Style.Font.Bold = true;

            memberWs.Cells[7, 1].Value = "Task code";
            memberWs.Cells[7, 2].Value = "Tên công việc";
            memberWs.Cells[7, 3].Value = "Thành viên thực hiện";
            //memberWs.Cells[7, 4].Value = "Ngày ghi nhận";
            memberWs.Cells[7, 4].Value = "Điểm công việc";
            memberWs.Cells[7, 5].Value = "Giờ làm dự kiến";
            memberWs.Cells[7, 6].Value = "Giờ làm thực tế";
            memberWs.Cells[7, 7].Value = "Đánh giá";
            memberWs.Cells[7, 8].Value = "Thưởng";

            var extendRow = 6;
            // Validations
            var memberEmailListVal = memberWs.SelectedRange[8, 3, 8 + extendRow, 3].DataValidation.AddListDataValidation();
            //memberEmailListVal.ShowErrorMessage = true;
            //memberEmailListVal.ErrorStyle = ExcelDataValidationWarningStyle.information;
            //memberEmailListVal.ErrorTitle = "Thành viên không tồn tại trong dự án";
            //memberEmailListVal.Error = "Thành viên không tồn tại trong dự án";
            //memberEmailListVal.AllowBlank = false;

            projectMembers.ForEach(member =>
            {
                memberEmailListVal.Formula.Values.Add(member.Member.EmailAddress);
            });

            var efforListVal = memberWs.SelectedRange[8, 7, 8 + extendRow, 7].DataValidation.AddListDataValidation();
            //efforListVal.ShowErrorMessage = true;
            //efforListVal.ErrorStyle = ExcelDataValidationWarningStyle.information;
            //efforListVal.ErrorTitle = "Đánh giá không hợp lệ";
            //efforListVal.Error = "Đánh giá không hợp lệ";
            //memberEmailListVal.AllowBlank = false;

            efforListVal.Formula.Values.Add("A");
            efforListVal.Formula.Values.Add("B");
            efforListVal.Formula.Values.Add("C");
            efforListVal.Formula.Values.Add("D");


            memberWs.SelectedRange[8, 1, 8 + extendRow, 8].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[8, 1, 8 + extendRow, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[8, 1, 8 + extendRow, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            memberWs.SelectedRange[8, 1, 8 + extendRow, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            return excel;
        }
    }
}
