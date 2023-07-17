using Application.Domain.Constants;
using Application.Domain.Models;
using Application.DTOs.ProjectReport;
using Application.Services;
using org.mariuszgromada.math.mxparser;

namespace Application.Helpers
{
    public static class BenefitHelper
    {
        public static (double, double, double) CalculateP2_P3_XP(
            double pointPerHour,
            double softSkill, double hardSkill,
            double workHours, double workRealHours,
            int numberOfTask
        )
        {
            var sysCfg = GlobalVar.SystemConfig;
            //var workEffort = 0d;
            //if (workHours == 0 || workRealHours == 0)
            //{
            //    workEffort = 0d;
            //}
            //else if (workHours != 0 && workRealHours != 0)
            //{
            //    workEffort = workHours / workRealHours;
            //    if (workEffort > 1.25) workEffort = 1.25;
            //    if (workEffort < 0.75) workEffort = 0.75;
            //}

            #region Calculate P2
            var personPointEq = sysCfg.P2Equation;
            personPointEq = personPointEq
              .Replace("{pointPerHour}", pointPerHour.ToString())
              .Replace("{softSkill}", softSkill.ToString())
              .Replace("{hardSkill}", hardSkill.ToString())
              .Replace("{workHour}", workHours.ToString())
              .Replace("{workRealHour}", workRealHours.ToString())
              .Replace("{taskCount}", numberOfTask.ToString());

            var p2Eq = new Expression(personPointEq);
            var personPoint = p2Eq.calculate();

            #endregion

            #region Calculate P3
            var performancePointEq = sysCfg.P3Equation;
            performancePointEq = performancePointEq
                .Replace("{pointPerHour}", pointPerHour.ToString())
                .Replace("{softSkill}", softSkill.ToString())
                .Replace("{hardSkill}", hardSkill.ToString())
                .Replace("{workHour}", workHours.ToString())
                .Replace("{workRealHour}", workRealHours.ToString())
                .Replace("{taskCount}", numberOfTask.ToString());

            var p3Eq = new Expression(performancePointEq);
            var performancePoint = p3Eq.calculate();
            #endregion

            #region Calculate XP
            var xpEq = sysCfg.XPEquation;
            xpEq = xpEq
                .Replace("{pointPerHour}", pointPerHour.ToString())
                .Replace("{softSkill}", softSkill.ToString())
                .Replace("{hardSkill}", hardSkill.ToString())
                .Replace("{workHour}", workHours.ToString())
                .Replace("{workRealHour}", workRealHours.ToString())
                .Replace("{taskCount}", numberOfTask.ToString());

            var xpExpression = new Expression(xpEq);
            var xp = xpExpression.calculate();
            #endregion

            return (personPoint, performancePoint, xp);
        }

        // public static double EstimateRewardsToPoint(ProjectReport reportFull)
        // {
        //   var res = EstimateRewards(reportFull);
        //   return res.TotalNeeded;
        // }

        public static ProjectReportEstimateDTO EstimateRewardsForReport(ProjectReport reportFull)
        {
            ProjectReportEstimateDTO result = new ProjectReportEstimateDTO();
            var salaryCycle = reportFull.SalaryCycle;

            reportFull.ProjectReportMembers.ForEach((projectReportMember) =>
            {
                var workHours = 0d;
                var workRealHours = 0d;

                // Setup tasks for each members
                var tasks = projectReportMember.ProjectReportMemberTasks;

                var totalTaskPoint = 0d;
                var totalBonusPoint = 0d;

                tasks.ForEach(task =>
                  {
                      workHours += task.TaskHour;
                      workRealHours += task.TaskRealHour;
                      totalTaskPoint += task.TaskPoint * (task.TaskEffort / 100);
                      totalBonusPoint += task.TaskBonus;
                  }
                );

                var projectMember = reportFull.Project.ProjectMember.First(m => m.MemberId == projectReportMember.ProjectMember.MemberId);

                var memberPayslip = salaryCycle.Payslips.FirstOrDefault(x => x.MemberId == projectMember.MemberId);
                var memberAttsInPayslip = memberPayslip.PayslipAttributes.Select(pma => pma.Attribute).ToList();

                var basePointPerHour = double.Parse(memberAttsInPayslip.First(a => a.AttributeGroupId == AttributeGroupNameValues.PointPerHour).Value);
                var basePoint = double.Parse(memberAttsInPayslip.First(a => a.AttributeGroupId == AttributeGroupNameValues.BasePoint).Value);

                //var memberAttsInProject = projectMember.ProjectMemberAttributes.Select(pma => pma.Attribute).ToList();

                //var softSkill = double.Parse(memberAttsInProject.First(a => a.AttributeGroupId == AttributeGroupNameValues.SoftSkill).Value);
                //var hardSkill = double.Parse(memberAttsInProject.First(a => a.AttributeGroupId == AttributeGroupNameValues.HardSkill).Value);

                var projectMemberReportAtts = projectReportMember.ProjectReportMemberAttributes.Select(pmra => pmra.Attribute).ToList();

                double softSkill = 0;
                double hardSkill = 0;

                var softSkillRaw = projectMemberReportAtts.FirstOrDefault(a => a.AttributeGroupId == AttributeGroupNameValues.SoftSkill);
                if (softSkillRaw != null) softSkill = double.Parse(softSkillRaw.Value);

                var hardSkillRaw = projectMemberReportAtts.FirstOrDefault(a => a.AttributeGroupId == AttributeGroupNameValues.HardSkill);
                if (hardSkillRaw != null) hardSkill = double.Parse(hardSkillRaw.Value);

                var (personPoint, performancePoint, xp) = CalculateP2_P3_XP(
                    basePointPerHour,
                    softSkill,
                    hardSkill,
                    workHours,
                    workRealHours,
                    tasks.Count);

                // var taskPointSum = tasks.Sum(t => t.TaskPoint);
                // performancePoint += taskPointSum;


                //performancePoint += totalTaskPoint;
                //var totalPointForMember = personPoint + performancePoint;

                result.MemberRewards.Add(new ProjectReportEstimateDTO_Member
                {
                    MemberId = projectReportMember.ProjectMember.MemberId,
                    MemberEmail = projectReportMember.ProjectMember.Member.EmailAddress,
                    // P1 = basePoint,
                    P2 = personPoint,
                    P3 = performancePoint,
                    TaskPoint = totalTaskPoint,
                    XP = xp,
                    Bonus = totalBonusPoint
                });

                result.TotalP2 += personPoint;
                result.TotalP3 += performancePoint;
                result.TotalTaskPoint += totalTaskPoint;
                result.TotalBonusPoint += totalBonusPoint;
            });

            return result;
        }
    }
}