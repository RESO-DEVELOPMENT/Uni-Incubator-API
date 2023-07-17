using Application.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Application.Domain;
using Application.Domain.Constants;
using Application.Domain.Enums;
using Application.Domain.Enums.Member;
using Application.Domain.Enums.MemberVoucher;
using Application.Domain.Enums.Notification;
using Application.Domain.Enums.Payslip;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.Project;
using Application.Domain.Enums.ProjectReport;
using Application.Domain.Enums.ProjectSponsorTransaction;
using Application.Domain.Enums.SalaryCycle;
using Application.Domain.Enums.Wallet;
using Application.Domain.Models;
using Application.Persistence.Repositories;
using Attribute = Application.Domain.Models.Attribute;
using AutoMapper.Execution;
using System.Composition;
using Application.Domain.Enums.ProjectMember;
using Application.Domain.Enums.ProjectEndRequest;
using org.mariuszgromada.math.mxparser;

namespace Application.Services
{
    public class RedisQueueRunner : BackgroundService
    {
        private readonly ILogger<RedisQueueRunner> _logger;
        private readonly INotificationService _notificationService;

        private readonly IQueueService _redisQueueService;
        private readonly IFirebaseService _firebaseService;
        private readonly IWalletService _walletService;
        private readonly UnitOfWork _unitOfWork;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private bool _isRunning = false;

        private readonly string _eventName;

        public RedisQueueRunner(IServiceProvider services, IConfiguration configuration, ILogger<RedisQueueRunner> logger)
        {
            _logger = logger;
            var scope = services.CreateScope();

            _walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
            _unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
            _redisQueueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
            _firebaseService = scope.ServiceProvider.GetRequiredService<IFirebaseService>();
            _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            _mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
            _webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            _eventName = configuration.GetValue<string>("redis:eventName");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _redisQueueService.GetConnection().GetSubscriber().Subscribe(_eventName, async (channel, message) =>
            {
                _logger.LogInformation($"=== Task Recieved From {_eventName}");

                if (!_isRunning)
                {
                    string? taskInPub = null;
                    if (!message.IsNull)
                    {
                        taskInPub = message.ToString();
                        try
                        {
                            if (taskInPub != null)
                            {
                                var qt = JsonSerializer.Deserialize<QueueTask>(taskInPub);
                                await DoNextWork(qt);
                            }

                            await DoNextWork();
                        }
                        catch
                        {
                            await DoNextWork();
                        }
                    }
                    else
                    {
                        await DoNextWork();
                    }
                };
            });

            return Task.CompletedTask;
        }

        public async Task DoNextWork(QueueTask? newQueueTask = null)
        {
            if (!_isRunning)
            {
                _isRunning = true;

                try
                {
                    var nextTask = newQueueTask ?? await _redisQueueService.GetFromQueue();
                    if (nextTask != null)
                    {

                        var task = nextTask;
                        var data = task.TaskData;

                        _logger.LogInformation($"=== Run Task {task.TaskName}");

                        // if (task.TaskName == TaskName.CheckSalaryCycle)
                        // {
                        //   await CheckSalaryCycle();
                        // }
                        // if (task.TaskName == TaskName.CheckProjectSalaryCycles)
                        // {
                        //   await CheckProjectSalaryCycles();
                        // }
                        if (task.TaskName == TaskName.ProcessSalaryCycleCreate)
                        {
                            await ProcessSalaryCycle_Create(data);
                        }
                        else if (task.TaskName == TaskName.UpdateProjectSalaryCycleStatus)
                        {
                            await UpdateProjectSalaryCycleStatus(data);
                        }
                        else if (task.TaskName == TaskName.ProcessSalaryCyclePaid)
                        {
                            await ProcessSalaryCycle_Paid(data);
                        }
                        else if (task.TaskName == TaskName.ProcessSalaryCycleCancel)
                        {
                            await ProcessSalaryCycle_Cancel(data);
                        }
                        //else if (task.TaskName == TaskName.ProcessProjectEnd)
                        //{
                        //    await ProcessProject_End(data);
                        //}
                        else if (task.TaskName == TaskName.CheckExpiredWallets)
                        {
                            await CheckExpiredWallets();
                        }
                        else if (task.TaskName == TaskName.CheckMembersLevel)
                        {
                            await CheckMembersLevel();
                        }
                        else if (task.TaskName == TaskName.CheckDisabledMemberWallet)
                        {
                            await CheckDisabledMemberWallet();
                        }
                        else if (task.TaskName == TaskName.CheckEndedProjectWallet)
                        {
                            await CheckEndedProjectWallet();
                        }
                        else if (task.TaskName == TaskName.CheckExpiredProject)
                        {
                            await CheckExpiredProjects();
                        }
                        else if (task.TaskName == TaskName.CheckExpiredVoucher)
                        {
                            await CheckExpiredVouchers();
                        }
                        else if (task.TaskName == TaskName.SendNotification)
                        {
                            await SendNotification(data);
                        }
                        else if (task.TaskName == TaskName.MemberBuyVoucher)
                        {
                            await ProcessBuyVoucher(data);
                        }
                        else if (task.TaskName == TaskName.SendMail)
                        {
                            await SendMail(data);
                        }
                        else if (task.TaskName == TaskName.SendPoint)
                        {
                            await SendPoint(data);
                        }
                        else if (task.TaskName == TaskName.SendXP)
                        {
                            await SendXP(data);
                        }
                        // else if (task.TaskName == TaskName.SetupWallet)
                        // {
                        //   await SetupWallet(data);
                        // }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.StackTrace);
                }
                finally
                {
                    _isRunning = false;
                    await DoNextWork();
                }
            }
        }

        private async Task UpdateProjectSalaryCycleStatus(Dictionary<string, string> data)
        {
            var salaryCycleId = Guid.Parse(data["SalaryCycleId"]);
            var salaryCycleNewStatus = Enum.Parse<SalaryCycleStatus>(data["SalaryCycleNewStatus"]);
            var salaryCycleOldStatus = Enum.Parse<SalaryCycleStatus>(data["SalaryCycleOldStatus"]);
            var memberId = Guid.Parse(data["MemberId"]);

            try
            {
                var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository
                        .GetQuery()
                        .Include(x => x.Payslips)
                        .FirstOrDefaultAsync(sc => sc.SalaryCycleId == salaryCycleId);
                if (currentSalaryCycle == null) return;

                // if (currentSalaryCycle.SalaryCycleStatus != salaryCycleOldStatus)
                // {
                //   throw new Exception("Request is not valid or already processed!");
                // };

                // var newType = "";

                if (salaryCycleNewStatus == SalaryCycleStatus.Paid)
                {
                    // newType = "SALARY_CYCLE_STATUS_UPDATED_TO_PAID";
                    await ProcessSalaryCycle_Paid(data);
                }
                else if (salaryCycleNewStatus == SalaryCycleStatus.Cancelled)
                {
                    // newType = "SALARY_CYCLE_STATUS_CANCELED";
                    await ProcessSalaryCycle_Cancel(data);
                }

                currentSalaryCycle.SalaryCycleStatus = salaryCycleNewStatus;
                await _unitOfWork.SaveAsync();

                var targetList = new Dictionary<string, string> { { "SalaryCycleId", salaryCycleId.ToString() } };

                var memberToSend = new List<Guid>() { memberId };
                memberToSend.AddRange(currentSalaryCycle.Payslips.Select(x => x.MemberId));

                memberToSend = memberToSend.Distinct().ToList();

                memberToSend.ForEach(async mId =>
                {
                    await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                        {"MemberId", mId.ToString()},
                        {"Type", NotificationType.SalaryCycleUpdateSuccess.ToString()},
                        {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                        {"Title", $"Kì lương {currentSalaryCycle.Name} đã được chuyển sang {SalaryCycleStatusTranslate.Translate(currentSalaryCycle.SalaryCycleStatus)}!"},
                        {"Content", $"Kì lương {currentSalaryCycle.Name} đã được chuyển sang {SalaryCycleStatusTranslate.Translate(currentSalaryCycle.SalaryCycleStatus)}!"},
                    });
                });
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}", validationErrors.Entry.Entity.ToString(), validationError.ErrorMessage);
                        //raise a new exception inserting the current one as the InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            catch (Exception ex)
            {
                var targetList = new Dictionary<String, String> { { "SalaryCycleId", salaryCycleId.ToString() } };
                await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                    {"MemberId", memberId.ToString()},
                    {"Type", NotificationType.SalaryCycleUpdateFail.ToString()},
                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                    {"Title", $"Kì lương khộng được cập nhật!"},
                    {"Content", ex.Message.ToString()},
                    {"SaveNotification", "False"}
                });
            }
        }

        private async Task ProcessSalaryCycle_Create(Dictionary<string, string> data)
        {
            var salaryCycleId = Guid.Parse(data["SalaryCycleId"]);

            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository
                    .GetQuery()
                    .Include(x => x.Payslips)
                    .FirstOrDefaultAsync(sc => sc.SalaryCycleId == salaryCycleId);

            var systemWallet = await _unitOfWork.WalletRepository.GetSystemWallets();
            var systemPointWallet = systemWallet.First(w => w.WalletToken == WalletToken.Point);

            var projectThatStarted = await _unitOfWork.ProjectRepository.GetQuery()
            .Where(p => p.ProjectStatus == ProjectStatus.Started)
              .Include(p => p.ProjectWallets.Where(mw => mw.Wallet.WalletStatus == WalletStatus.Available))
                .ThenInclude(p => p.Wallet)
            .ToListAsync();

            projectThatStarted.ForEach(proj =>
            {
                #region Add Base Budget
                var mainPWallet = proj.ProjectWallets.First(pw => pw.Wallet.WalletTag == "main");

                var pointDiff = proj.Budget;

                systemPointWallet.Amount -= pointDiff;
                mainPWallet.Wallet.Amount += pointDiff;

                mainPWallet.Wallet.TransactionsTo.Add(
                  new Transaction()
                  {
                      TransactionType = TransactionType.SystemDepositToProject,
                      FromWalletId = systemPointWallet.WalletId,
                      Note = $"Point từ hệ thống",
                      Token = WalletToken.Point,
                      Amount = pointDiff,

                      FromAmountAfterTransaction = systemPointWallet.Amount,
                      ToAmountAfterTransaction = mainPWallet.Wallet.Amount
                  });

                _unitOfWork.ProjectWalletRepository.Update(mainPWallet);

                #endregion
            });

            currentSalaryCycle.Payslips.ForEach(async payslip =>
            {
                var targetList = new Dictionary<string, string> { { "SalaryCycle", salaryCycleId.ToString() } };

                await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                    {"MemberId", payslip.MemberId.ToString()},
                    {"Type", NotificationType.SalaryCycleStarted.ToString()},
                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                    {"Title", $"Kì lương {currentSalaryCycle.Name} đã bắt đầu!"},
                    {"Content", $"Kì lương {currentSalaryCycle.Name} đã bắt đầu!"},
                });
            });


            if (systemPointWallet.Amount < 0) throw new Exception("Hệ thống không có đủ tiền");
            _unitOfWork.WalletRepository.Update(systemPointWallet);

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new Exception("Xử lý thất bại");
        }

        private async Task ProcessSalaryCycle_Paid(Dictionary<string, string> data)
        {
            _logger.LogError("======================================================================================= TE");
            // Temporary Fix For Project Status
            _unitOfWork.ClearChanges();

            var salaryCycleId = Guid.Parse(data["SalaryCycleId"]);

            //var pendingQueueTasks = new List<QueueTask>();
            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository
                      .GetQuery()
                      .Include(ps => ps.Payslips)
                        .ThenInclude(ps => ps.PayslipItems)

                      .Include(ps => ps.Payslips)
                        .ThenInclude(ps => ps.Member.MemberLevels.Where(x => x.IsActive))
                          .ThenInclude(x => x.Level)

                      .FirstOrDefaultAsync(sc => sc.SalaryCycleId == salaryCycleId);

            if (currentSalaryCycle == null) throw new NotFoundException("Kì lương không tồn tại!", ErrorNameValues.SalaryCycleNotFound);

            var projectReports = await _unitOfWork.ProjectReportRepository
                .GetReportsFullQuery()
                .Where(pr =>
                   pr.SalaryCycleId == currentSalaryCycle!.SalaryCycleId &&
                   pr.Status == ProjectReportStatus.Accepted)
                 .ToListAsync();

            var projectNotDoneReports = await _unitOfWork.ProjectReportRepository.GetReportsFullQuery()
             .Where(pr =>
               pr.SalaryCycleId == currentSalaryCycle!.SalaryCycleId &&
               (pr.Status == ProjectReportStatus.Created || pr.Status == ProjectReportStatus.Drafted))
             .ToListAsync();

            projectNotDoneReports.ForEach(p =>
                {
                    p.Status = ProjectReportStatus.Cancelled;
                    //_unitOfWork.ProjectReportRepository.Update(p);
                }
             );

            // var projectBonusReports = await _unitOfWork.ProjectBonusReportRepository.GetFullReportQuery()
            //   .Where(pr =>
            //     pr.SalaryCycleId == currentSalaryCycle!.SalaryCycleId &&
            //     pr.Status == ProjectBonusReportStatus.Accepted)
            //   .ToListAsync();

            var memberPayslipItems = new Dictionary<Guid, List<PayslipItem>>();

            //// Add Processed Payslips
            //// Parse Payslips From Payslip (P1)
            //currentSalaryCycle.Payslips.ForEach(p =>
            //{
            //    var p1 = p.PayslipItems.FirstOrDefault(x => x.Type == PayslipItemType.P1);

            //    if (!memberPayslipItems.ContainsKey(p.MemberId))
            //    {
            //        memberPayslipItems.Add(p.MemberId, new List<PayslipItem>());
            //    }

            //    if (p1 == null)
            //    {
            //        var psiP1 = new PayslipItem()
            //        {
            //            PayslipId = p.PayslipId,
            //            Type = PayslipItemType.P1,
            //            Token = WalletToken.Point,
            //            Amount = p1.Amount,
            //            Note = $"Point từ hệ thống",
            //        };

            //        _unitOfWork.PayslipItemRepository.Add(psiP1);
            //        memberPayslipItems[p.MemberId].Add(psiP1);
            //    }
            //});

            // Add Default Work Hour To Fix P1
            var memberTotalWorkRealHour = new Dictionary<Guid, double>();
            var projectStartedOrStopped = await _unitOfWork.ProjectRepository.GetQuery()
                .Where(x => x.ProjectStatus == ProjectStatus.Started || x.ProjectStatus == ProjectStatus.Stopped)
                .Include(x => x.ProjectMember).ToListAsync();

            projectStartedOrStopped.ForEach(x =>
            {
                x.ProjectMember.ForEach(m =>
                {
                    if (!memberTotalWorkRealHour.ContainsKey(m.MemberId))
                    {
                        memberTotalWorkRealHour.Add(m.MemberId, 0);
                    }
                });
            });

            // Parse Payslips From Report(P2 and P3)
            projectReports.ForEach(report =>
            {// If project is stopped, end it
                var project = projectStartedOrStopped.FirstOrDefault(x => x.ProjectId == report.ProjectId);
                // Firefighter Incoming
                if (project != null)
                {
                    if (project.ProjectStatus == ProjectStatus.Stopped)
                    {
                        report.Project.ProjectStatus = ProjectStatus.Ended;
                    }
                    else
                    {
                        report.Project.ProjectStatus = ProjectStatus.Started;
                    }
                }
                else
                {
                    if (report.Project.ProjectStatus == ProjectStatus.Stopped)
                    {
                        report.Project.ProjectStatus = ProjectStatus.Ended;
                    }
                    else
                    {
                        report.Project.ProjectStatus = ProjectStatus.Started;
                    }
                }

                report.Status = ProjectReportStatus.Processed;
                var reportResult = BenefitHelper.EstimateRewardsForReport(report);

                _unitOfWork.ProjectReportRepository.Update(report);

                report.ProjectReportMembers.ForEach(x =>
                {
                    var totalWorkRealHour = x.ProjectReportMemberTasks.Sum(x => x.TaskRealHour);
                    memberTotalWorkRealHour[x.ProjectMember.MemberId] += totalWorkRealHour;
                });


                reportResult.MemberRewards.ForEach(result =>
                {
                    var memberId = result.MemberId;
                    var projectMember = report.ProjectReportMembers.First(x => x.ProjectMember.MemberId == memberId).ProjectMember;

                    if (!memberPayslipItems.ContainsKey(memberId))
                    {
                        memberPayslipItems.Add(memberId, new List<PayslipItem>());
                    }

                    var payslipItemP2 = new PayslipItem()
                    {
                        Type = PayslipItemType.P2,
                        Token = WalletToken.Point,
                        Amount = result.P2,
                        ProjectId = report.ProjectId,
                        Note = $"P2 từ <@Project:{report.ProjectId}>",
                        PayslipItemAttributes = new List<PayslipItemAttribute>
                          {
                            new()
                            {
                                Attribute = new Attribute
                                {
                                    AttributeGroupId = AttributeGroupNameValues.SoftSkill,
                                    Value = projectMember.ProjectMemberAttributes.First(x => x.Attribute.AttributeGroupId == AttributeGroupNameValues.SoftSkill).Attribute.Value
                                }
                            },
                            new()
                            {
                                Attribute = new Attribute
                                {
                                    AttributeGroupId = AttributeGroupNameValues.HardSkill,
                                    Value = projectMember.ProjectMemberAttributes.First(x => x.Attribute.AttributeGroupId == AttributeGroupNameValues.HardSkill).Attribute.Value
                                }
                            }
                          }
                    };

                    var payslipItemP3 = new PayslipItem()
                    {
                        Type = PayslipItemType.P3,
                        Token = WalletToken.Point,
                        Amount = result.P3 + result.TaskPoint,
                        ProjectId = report.ProjectId,
                        Note = $"P3 từ <@Project:{report.ProjectId}>",
                    };

                    var payslipItemXP = new PayslipItem()
                    {
                        Type = PayslipItemType.XP,
                        Token = WalletToken.XP,
                        Amount = result.XP,
                        ProjectId = report.ProjectId,
                        Note = $"XP từ <@Project:{report.ProjectId}>",
                    };

                    memberPayslipItems[memberId].AddRange(new List<PayslipItem>()
                    {
                        payslipItemP2,
                        payslipItemP3,
                        payslipItemXP
                    });

                    if (result.Bonus > 0)
                    {
                        var payslipBonus = new PayslipItem()
                        {
                            Type = PayslipItemType.Bonus,
                            Token = WalletToken.Point,
                            Amount = result.Bonus,
                            ProjectId = report.ProjectId,
                            Note = $"Thưởng từ <@Project:{report.ProjectId}>",
                        };

                        memberPayslipItems[memberId].Add(payslipBonus);
                    }
                });
            });

            // Parse P1
            memberTotalWorkRealHour.Keys.ToList().ForEach(mId =>
            {
                var curMember = currentSalaryCycle.Payslips.FirstOrDefault(x => x.MemberId == mId).Member;
                var workRealHour = memberTotalWorkRealHour[mId];
                var p1Result = 0d;

                if (workRealHour != 0)
                {
                    var p1Eq = GlobalVar.SystemConfig.P1Equation;
                    p1Eq = p1Eq
                        .Replace("{basePoint}", curMember.MemberLevels.First().Level.BasePoint.ToString())
                        .Replace("{workRealHour}", workRealHour.ToString());

                    var p1Expression = new Expression(p1Eq);
                    p1Result = p1Expression.calculate();


                    var p1Payslip = new PayslipItem()
                    {
                        Type = PayslipItemType.P1,
                        Token = WalletToken.Point,
                        Amount = p1Result,
                        ProjectId = null,
                        Note = $"P1 từ hệ thống!"
                    };

                    if (!memberPayslipItems.ContainsKey(mId))
                    {
                        memberPayslipItems.Add(mId, new List<PayslipItem>());
                    }
                    memberPayslipItems[mId].Add(p1Payslip);
                }
            });

            // Get Member Wallet, Project Wallet and System Wallet
            var projectWithWallets = await _unitOfWork.ProjectRepository.GetQuery()
              .Where(p => projectReports.Select(pr => pr.ProjectId).Distinct().Contains(p.ProjectId))
              .Include(p => p.ProjectWallets.Where(pw => pw.Wallet.WalletStatus == WalletStatus.Available)).ThenInclude(pw => pw.Wallet)
              .ToListAsync();

            var membersWithWallets = await _unitOfWork.MemberRepository.GetQuery()
                .Where(p => currentSalaryCycle!.Payslips.Select(x => x.MemberId).Contains(p.MemberId))
                  .Include(p => p.MemberWallets.Where(mw => mw.Wallet.WalletStatus == WalletStatus.Available))
                    .ThenInclude(mw => mw.Wallet)
                .ToListAsync();

            var systemWallet = await _unitOfWork.WalletRepository.GetSystemWallets();
            var systemXpWallet = systemWallet.First(sw => sw.WalletToken == WalletToken.XP);
            var systemPointWallet = systemWallet.First(sw => sw.WalletToken == WalletToken.Point);

            // Finalize Payment
            memberPayslipItems.Keys.ToList().ForEach(memberId =>
            {
                var payslipItems = memberPayslipItems[memberId];

                var memberWithWallets = membersWithWallets.FirstOrDefault(x => x.MemberId == memberId);
                if (memberWithWallets == null) throw new BadRequestException($"Thành viên <{memberWithWallets.EmailAddress}> chưa nằm trong kì lương , hãy tra lại kì lương");

                var curPayslip = currentSalaryCycle.Payslips.FirstOrDefault(p => p.MemberId == memberId);
                if (curPayslip == null) throw new BadRequestException($"Thành viên <{memberWithWallets.EmailAddress}> chưa nằm trong kì lương , hãy tra lại kì lương");

                curPayslip.Status = PayslipStatus.Paid;
                payslipItems.AddRange(curPayslip.PayslipItems);

                var memberPointWallet = memberWithWallets.MemberWallets.First(x => x.Wallet.WalletToken == WalletToken.Point);
                var memberXpWallet = memberWithWallets.MemberWallets.First(x => x.Wallet.WalletToken == WalletToken.XP);

                // Paid for P1, P2, P3, Bonus, XP
                payslipItems.ForEach(psi =>
                {
                    // Fix PayslipItem that is created in this function
                    // if (psi.PayslipItemId == null) _unitOfWork.PayslipItemRepository.Add(psi);
                    // if (psi.PayslipId == null
                    psi.PayslipId = curPayslip.PayslipId;

                    switch (psi.Type)
                    {
                        case PayslipItemType.XP:
                            {
                                var xpDiff = psi.Amount;

                                systemXpWallet.Amount -= xpDiff;
                                memberXpWallet.Wallet.Amount += xpDiff;

                                memberXpWallet.Wallet.TransactionsTo.Add(
                              new Transaction()
                              {
                                  FromWalletId = systemXpWallet.WalletId,
                                  ToWalletId = memberXpWallet.WalletId,
                                  PayslipItem = psi,
                                  Note = $"XP từ <@Project:{psi.ProjectId}>",
                                  TransactionType = TransactionType.ProjectSalary,
                                  Token = WalletToken.XP,
                                  Amount = xpDiff,

                                  FromAmountAfterTransaction = systemXpWallet.Amount,
                                  ToAmountAfterTransaction = memberXpWallet.Wallet.Amount
                              }
                          );
                                break;
                            }
                        case PayslipItemType.P1:
                            {
                                var pointDiff = psi.Amount;

                                systemPointWallet.Amount -= pointDiff;
                                memberPointWallet.Wallet.Amount += pointDiff;

                                memberPointWallet.Wallet.TransactionsTo.Add(new Transaction()
                                {
                                    FromWalletId = systemPointWallet.WalletId,
                                    ToWalletId = memberPointWallet.WalletId,
                                    PayslipItem = psi,
                                    Note = $"P1 từ Hệ Thống",
                                    TransactionType = TransactionType.SystemSalary,
                                    Token = WalletToken.Point,
                                    Amount = pointDiff,

                                    FromAmountAfterTransaction = systemPointWallet.Amount,
                                    ToAmountAfterTransaction = memberPointWallet.Wallet.Amount
                                });
                                break;
                            }
                        case PayslipItemType.P2:
                        case PayslipItemType.P3:
                            {
                                var pointDiff = psi.Amount;

                                var projectPointWallet = projectWithWallets.Where(p => p.ProjectId == psi.ProjectId).First()
                              .ProjectWallets.First(pw => pw.Wallet.WalletToken == WalletToken.Point && pw.Wallet.WalletTag == "main");

                                projectPointWallet.Wallet.Amount -= pointDiff;
                                memberPointWallet.Wallet.Amount += pointDiff;

                                memberPointWallet.Wallet.TransactionsTo.Add(
                              new Transaction()
                              {
                                  FromWalletId = projectPointWallet.WalletId,
                                  ToWalletId = memberPointWallet.WalletId,
                                  PayslipItem = psi,
                                  Note = $"Point từ <@Project:{psi.ProjectId}>",
                                  TransactionType = TransactionType.ProjectSalary,
                                  Token = WalletToken.Point,
                                  Amount = pointDiff,

                                  FromAmountAfterTransaction = projectPointWallet.Wallet.Amount,
                                  ToAmountAfterTransaction = memberPointWallet.Wallet.Amount
                              }
                          );

                                break;
                            }
                        case PayslipItemType.Bonus:
                            {
                                var projectBonusWallet = projectWithWallets.Where(p => p.ProjectId == psi.ProjectId).First()
                              .ProjectWallets.First(pw => pw.Wallet.WalletToken == WalletToken.Point && pw.Wallet.WalletTag == "bonus");

                                var pointDiff = psi.Amount;

                                projectBonusWallet.Wallet.Amount -= pointDiff;
                                memberPointWallet.Wallet.Amount += pointDiff;

                                memberPointWallet.Wallet.TransactionsTo.Add(
                              new Transaction()
                              {
                                  FromWalletId = projectBonusWallet.WalletId,
                                  ToWalletId = memberPointWallet.WalletId,
                                  PayslipItem = psi,
                                  Note = $"Thưởng từ <@Project:{psi.ProjectId}>",
                                  TransactionType = TransactionType.ProjectSalary,
                                  Token = WalletToken.Point,
                                  Amount = pointDiff,

                                  FromAmountAfterTransaction = projectBonusWallet.Wallet.Amount,
                                  ToAmountAfterTransaction = memberPointWallet.Wallet.Amount
                              }
                          );


                                break;
                            }
                    }
                });

                _unitOfWork.MemberWalletRepository.Update(memberPointWallet);
                _unitOfWork.MemberWalletRepository.Update(memberXpWallet);
                //_unitOfWork.PayslipRepository.Update(curPayslip);
                // _unitOfWork.MemberRepository.Update(memberWithWallets);
            });

            currentSalaryCycle.SalaryCycleStatus = SalaryCycleStatus.Paid;
            currentSalaryCycle.EndedAt = DateTimeHelper.Now();

            // Postcheck to make sure the system work correctly!
            projectWithWallets.ForEach(p =>
            {
                p.ProjectWallets.ForEach(pw =>
          {
              if (pw.Wallet.Amount < 0) throw new Exception($"Dự án <{p.ProjectName}> không có đủ tiền trong ví {pw.Wallet.WalletTag}!");
          });
            });

            systemWallet.ForEach(w =>
            {
                if (w.Amount < 0) throw new Exception($"Hệ thống không có đủ tiền trong ví {w.WalletTag}!");
            });

            _unitOfWork.ProjectRepository.Update(projectWithWallets);

            _unitOfWork.WalletRepository.Update(systemWallet);
            _unitOfWork.SalaryCycleRepository.Update(currentSalaryCycle);

            var result = await _unitOfWork.SaveAsync();

            if (result)
            {
                await _redisQueueService.AddToQueue(new QueueTask() { TaskName = TaskName.CheckMembersLevel });
                await _redisQueueService.AddToQueue(new QueueTask() { TaskName = TaskName.CheckEndedProjectWallet });
                //await CheckMembersLevel();
                //await CheckEndedProjectWallet();

                //pendingQueueTasks.ForEach(async queueTask =>
                //{
                //    await _redisQueueService.AddToQueue(queueTask);
                //});

                var memberIds = memberPayslipItems.Select(m => m.Key).ToList();

                memberIds.ForEach(async memberId =>
                {
                    var targetList = new Dictionary<String, String>();
                    targetList.Add("SalaryCycle", salaryCycleId.ToString());

                    await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                    {"MemberId", memberId.ToString()},
                    {"Type", NotificationType.SalaryCyclePaid.ToString()},
                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                    {"Title", $"Kì lương hiện tại đã kết thúc"},
                    {"Content", $"Kì lương hiện tại đã kết thúc, hãy kiểm tra lại thông tin!"},
                    {"SendNotification", "True"}
                  });
                });
            }
        }

        private async Task ProcessSalaryCycle_Cancel(Dictionary<string, string> data)
        {
            var salaryCycleId = Guid.Parse(data["SalaryCycleId"]);

            var currentSalaryCycle = await _unitOfWork.SalaryCycleRepository
                    .GetQuery()
                    .Include(p => p.ProjectReports)
                    .Include(p => p.Payslips)
                    .FirstOrDefaultAsync(sc => sc.SalaryCycleId == salaryCycleId);

            currentSalaryCycle!.Payslips.ForEach(ps => ps.Status = PayslipStatus.Cancelled);
            currentSalaryCycle!.ProjectReports.ForEach(pr => pr.Status = ProjectReportStatus.Cancelled);
            currentSalaryCycle!.EndedAt = DateTimeHelper.Now();

            var result = await _unitOfWork.SaveAsync();
            if (!result) throw new Exception("Failed to process!");
        }

        //private async Task ProcessProject_End(Dictionary<string, string> data)
        //{
        //    var curSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetLatestSalaryCycle();
        //    if (curSalaryCycle is { SalaryCycleStatus: >= SalaryCycleStatus.Ongoing and <= SalaryCycleStatus.Locked })
        //    {
        //        return;
        //    }

        //    var now = DateTimeHelper.Now();
        //    var expiredProjects = await _unitOfWork.ProjectRepository
        //        .GetQuery()
        //        .Where(p =>
        //            p.ProjectStatus == ProjectStatus.Started &&
        //            p.EndedAt != null &&
        //            p.EndedAt <= now)
        //        .Include(x => x.ProjectMember
        //            .Where(x =>
        //                x.Role == ProjectMemberRole.Manager &&
        //                x.Status == ProjectMemberStatus.Active))
        //        .Include(x => x.ProjectWallets.Where(pw => pw.Wallet.WalletStatus == WalletStatus.Available))
        //        .ThenInclude(pw => pw.Wallet)
        //        .ToListAsync();

        //    var systemWallet = await _unitOfWork.WalletRepository.GetSystemWallets();
        //    var systemPointWallet = systemWallet.First(x => x.WalletToken == WalletToken.Point);

        //    var queueList = new List<QueueTask>();

        //    expiredProjects.ForEach(async x =>
        //    {
        //        x.ProjectStatus = ProjectStatus.Ended;

        //        var projectMainWallet = x.ProjectWallets.First(pw => pw.Wallet.WalletTag == "main").Wallet;
        //        var projectBonusWallet = x.ProjectWallets.First(pw => pw.Wallet.WalletTag == "bonus").Wallet;

        //        if (projectMainWallet.Amount > 0)
        //        {
        //            var amount = projectMainWallet.Amount;

        //            projectMainWallet.Amount = 0;
        //            systemPointWallet.Amount += amount;

        //            var trx = new Transaction()
        //            {
        //                FromWalletId = projectMainWallet.WalletId,
        //                ToWalletId = systemPointWallet.WalletId,
        //                TransactionType = TransactionType.ProjectReturnToSystem,
        //                Amount = amount,
        //                Note = "Điểm dự án trả về hệ thống",
        //                Token = WalletToken.Point,
        //                FromAmountAfterTransaction = 0,
        //                ToAmountAfterTransaction = systemPointWallet.Amount,
        //            };

        //            _unitOfWork.TransactionRepository.Add(trx);
        //        }

        //        if (projectBonusWallet.Amount > 0)
        //        {
        //            var amount = projectBonusWallet.Amount;

        //            projectBonusWallet.Amount = 0;
        //            systemPointWallet.Amount += amount;

        //            var trx = new Transaction()
        //            {
        //                FromWalletId = projectBonusWallet.WalletId,
        //                ToWalletId = systemPointWallet.WalletId,
        //                TransactionType = TransactionType.ProjectReturnToSystem,
        //                Amount = amount,
        //                Note = "Điểm thưởng dự án trả về hệ thống",
        //                Token = WalletToken.Point,
        //                FromAmountAfterTransaction = 0,
        //                ToAmountAfterTransaction = systemPointWallet.Amount,
        //            };

        //            _unitOfWork.TransactionRepository.Add(trx);
        //        }

        //        var targetList = new Dictionary<string, string> { { "Project", x.ProjectId.ToString() } };
        //        var member = x.ProjectMember.First();

        //        queueList.Add(new QueueTask
        //        {
        //            TaskName = TaskName.SendNotification,
        //            TaskData = new Dictionary<string, string>() {
        //                    {"MemberId", member.MemberId.ToString()},
        //                    {"Type", NotificationType.ProjectEnded.ToString()},
        //                    {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
        //                    {"Title", $"Dự án {x.ProjectName} đã kết thúc!"},
        //                    {"Content", $"Dự án <@Project:{x.ProjectId.ToString()}> đã kết thúc!"},
        //                }
        //        });
        //    }
        //    );

        //    var result = await _unitOfWork.SaveAsync();

        //    if (result)
        //    {
        //        queueList.ForEach(async q =>
        //        {
        //            await _redisQueueService.AddToQueue(q);
        //        });
        //    }
        //}

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _redisQueueService.GetConnection().GetSubscriber().UnsubscribeAll();
        }

        private async Task SendPoint(Dictionary<string, string> data)
        {
            var fromId = Guid.Parse(data["FromId"]);
            var toId = Guid.Parse(data["ToId"]);
            var type = Enum.Parse<TransactionType>(data["TransactionType"]);
            var amount = double.Parse(data["Amount"]);
            var fromTag = data.ContainsKey("FromTag") ? JsonSerializer.Deserialize<List<string>>(data["FromTag"]) : null;
            var toTag = data.ContainsKey("ToTag") ? JsonSerializer.Deserialize<List<string>>(data["ToTag"]) : null;
            var note = data.ContainsKey("Note") ? data["Note"] : null;

            if (type == TransactionType.SponsorDepositToProject) note = "Sponsor Deposit To Project";
            if (type == TransactionType.MemberToMember)
            {
                var limit = await _walletService.GetMonthySendLimitForMember(fromId);
                if (limit.PointLeft - amount < 0)
                {
                    var targetList = new Dictionary<string, string> { { "FromMember", fromId.ToString() }, { "ToMember", toId.ToString() } };

                    await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                        {"MemberId", fromId.ToString()},
                        {"Type", NotificationType.MemberSendPointFailed.ToString()},
                        { "TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                        { "Title", $"Chuyển point thất bại"},
                        { "Content", $"Bạn đã vượt quá mức tối đa chuyển point trên tháng!"},
                    });
                }
            }

            var transactions = await _walletService.SendToken(fromId, toId, amount, WalletToken.Point, type, fromTag, toTag, note);

            if (transactions.Any())
            {
                switch (type)
                {
                    case TransactionType.MemberToMember:
                        {
                            var fromMember = await _unitOfWork.MemberRepository.GetByID(fromId);
                            var toMember = await _unitOfWork.MemberRepository.GetByID(toId);

                            var targetList = new Dictionary<string, string> { { "FromMember", fromId.ToString() }, { "ToMember", toId.ToString() } };

                            await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                                {"MemberId", fromId.ToString()},
                                {"Type", NotificationType.MemberSendPointSuccess.ToString()},
                                {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                                {"Title", $"Chuyển point thành công!"},
                                {"Content", $"Bạn đã chuyển {amount} point cho {toMember.EmailAddress} thành công!"},
                                {"SaveNotification", "True"},
                                {"SendNotification", "True"}
                            });

                            await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                              {"MemberId", toId.ToString()},
                              {"Type", NotificationType.MemberSendPointSuccess.ToString()},
                              {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                              {"Title", $"Bạn đã nhận được point!"},
                              {"Content", $"Bạn đã nhận được {amount} point từ {fromMember.EmailAddress}!"},
                              {"SaveNotification", "True"},
                              {"SendNotification", "True"}
                            });
                            break;
                        }
                    case TransactionType.SponsorDepositToProject:
                        {
                            var projectSponsorId = Guid.Parse(data["ProjectSponsorId"]);

                            _unitOfWork.ProjectSponsorTransactionRepository.Add(new ProjectSponsorTransaction()
                            {
                                ProjectSponsorId = projectSponsorId,
                                Amount = amount,
                                PaidAt = DateTimeHelper.Now(),
                                Type = ProjectSponsorTransactionType.Deposit,
                                Status = ProjectSponsorTransactionStatus.Paid,
                            });

                            await _unitOfWork.SaveAsync();
                            break;
                        }
                    case TransactionType.ProjectSalary:
                        {
                            var payslipItemId = Guid.Parse(data["PayslipItemId"]);
                            var payslip = await _unitOfWork.PayslipItemRepository.GetByID(payslipItemId);
                            if (payslip != null)
                            {
                                payslip.Transactions = transactions;
                            }
                            await _unitOfWork.SaveAsync();
                            break;
                        }
                }
            }
        }

        private async Task SendXP(Dictionary<string, string> data)
        {
            var fromId = Guid.Parse(data["FromId"]);
            var toId = Guid.Parse(data["ToId"]);
            TransactionType type = Enum.Parse<TransactionType>(data["TransactionType"]);
            var fromTag = data.ContainsKey("FromTag") ? JsonSerializer.Deserialize<List<String>>(data["FromTag"]) : null;
            var toTag = data.ContainsKey("ToTag") ? JsonSerializer.Deserialize<List<String>>(data["ToTag"]) : null;
            double amount = double.Parse(data["Amount"]);

            var transactions = await _walletService.SendToken(fromId, toId, amount, WalletToken.XP, TransactionType.ProjectSalary, fromTag, toTag);

            if (transactions != null && transactions.Count() > 0)
            {
                if (type == TransactionType.ProjectSalary)
                {
                    var payslipItemId = Guid.Parse(data["PayslipItemId"]);
                    var payslip = await _unitOfWork.PayslipItemRepository.GetByID(payslipItemId);
                    if (payslip != null)
                    {
                        payslip.Transactions = transactions;
                    }
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        private async Task SendMail(Dictionary<string, string> data)
        {
            if (_webHostEnvironment.EnvironmentName == "Testing")
                return;

            var toEmail = data["ToEmail"];
            var toName = data["ToName"];
            var type = data["Type"];
            var subject = data.ContainsKey("Subject") ? data["Subject"] : "";
            var content = data.ContainsKey("Content") ? data["Subject"] : null;

            if (type == "NEW_USER")
            {
                var password = data["Password"];
                await _mailService.SendMailForNewPassword(toEmail, toName, password);
            }
            if (type == "PROJECT_CREATE_NOTIFIY_PM")
            {
                var projectId = data["ProjectId"];
                var projectName = data["ProjectName"];
                await _mailService.SendMailForNewProjectCreated(projectId, projectName, toEmail, toName);
            }
            if (type == "REQUEST_RESET_PASSWORD")
            {
                var token = data["Token"];
                await _mailService.SendMailForResetPassword(toEmail, toName, token);
            }
            else
            {
                await _mailService.SendMail(toEmail, toName, subject, content);
            }
        }

        // public async Task SetupWallet(Dictionary<string, string> data)
        // {
        //   var targetId = Guid.Parse(data["TargetId"]);
        //   TargetType targetType = (TargetType)Enum.Parse(typeof(TargetType), data["TargetType"]);

        //   List<Wallet> wallets = new List<Wallet>();

        //   if (targetType == TargetType.Member)
        //   {
        //     wallets.Add(new Wallet()
        //     {
        //       WalletToken = WalletToken.XP,
        //       WalletType = WalletType.Cold,
        //       ExpiredDate = DateTimeHelper.Now().AddYears(1000),
        //       Amount = 0,
        //       TargetType = targetType,
        //       MemberWallet = new MemberWallet() { MemberId = targetId }
        //     });

        //     wallets.Add(new Wallet()
        //     {
        //       WalletToken = WalletToken.Point,
        //       WalletType = WalletType.Cold,
        //       ExpiredDate = DateTimeHelper.Now().AddYears(1000),
        //       Amount = 0,
        //       TargetType = targetType,
        //       MemberWallet = new MemberWallet() { MemberId = targetId }
        //     });
        //   }
        //   else if (targetType == TargetType.Project)
        //   {
        //     wallets.Add(new Wallet()
        //     {
        //       WalletToken = WalletToken.Point,
        //       WalletType = WalletType.Cold,
        //       ExpiredDate = DateTimeHelper.Now().AddYears(1000),
        //       Amount = 0,
        //       TargetType = targetType,
        //       ProjectWallet = new ProjectWallet() { ProjectId = targetId }
        //     });
        //   }

        //   _unitOfWork.WalletRepository.Add(wallets);
        //   await _unitOfWork.SaveAsync();
        // }

        public async Task SendNotification(Dictionary<string, string> data)
        {
            if (_webHostEnvironment.EnvironmentName == "Testing")
                return;

            var memberId = Guid.Parse(data["MemberId"]);
            var type = Enum.Parse<NotificationType>(data["Type"]);
            var targetId = data["TargetId"];
            var title = data["Title"];
            var content = data["Content"];

            var sendNoti = !data.ContainsKey("SendNotification") || data["SendNotification"] == "True"; // Default True
            var saveNoti = !data.ContainsKey("SaveNotification") || data["SaveNotification"] == "True"; // Default True

            await _notificationService.SendNotification(memberId, type, targetId, title, content, sendNoti, saveNoti);
        }

        public async Task ProcessBuyVoucher(Dictionary<string, string> data)
        {
            try
            {
                var memberId = Guid.Parse(data["MemberId"]);
                var voucherId = Guid.Parse(data["VoucherId"]);

                var member = await _unitOfWork.MemberRepository
               .GetQuery().Where(m => m.MemberId == memberId)
                 .Include(m => m.MemberWallets.Where(mw =>
                     mw.Wallet.WalletToken == WalletToken.Point &&
                     mw.Wallet.WalletStatus != WalletStatus.Unavailable &&
                     mw.Wallet.ExpiredDate > DateTimeHelper.Now()
                   ))
                   .ThenInclude(mw => mw.Wallet)
                   .FirstOrDefaultAsync();

                if (member == null) throw new NotFoundException();

                var voucher = await _unitOfWork.VoucherRepository.GetQuery().Where(v => v.VoucherId == voucherId).FirstOrDefaultAsync();
                if (voucher == null) throw new NotFoundException("Voucher not found!", ErrorNameValues.VoucherNotFound);

                if (voucher.VoucherAmount <= 0)
                    throw new BadRequestException($"Voucher {voucher.VoucherName} đã hết!");

                var memberPoint = member.MemberWallets.Select(mw => mw.Wallet).ToList().Total(WalletToken.Point);

                if (memberPoint < voucher.VoucherCost)
                    throw new BadRequestException($"Không đủ point để đôỉ voucher {voucher.VoucherName}", ErrorNameValues.InsufficentToken);

                var transactions = await _walletService.SendToken(member.MemberId, Guid.Empty, voucher.VoucherCost, WalletToken.Point, TransactionType.BuyVoucher, note: $"Redeem Voucher");

                if (transactions.Count <= 0) return;

                var code = "";
                while (true)
                {
                    var voucherPrefix = voucher.VoucherId.ToString().Substring(0, 3).ToUpper();
                    var randomSuffix = StringHelper.RandomString(9, false);

                    code = $"UNI-{voucherPrefix}-{randomSuffix}";
                    var codeDb = await _unitOfWork.MemberVoucherRepository.GetQuery().Where(x => x.Code == code).FirstOrDefaultAsync();
                    if (codeDb == null) break;
                }



                var newMemberVoucher = new MemberVoucher()
                {
                    MemberId = member.MemberId,
                    VoucherId = voucherId,
                    Status = MemberVoucherStatus.Created,
                    ExpiredAt = DateTimeHelper.Now().AddMonths(3),
                    Code = code
                };

                voucher.VoucherAmount -= 1;

                _unitOfWork.VoucherRepository.Update(voucher);
                _unitOfWork.MemberVoucherRepository.Add(newMemberVoucher);

                var result = await _unitOfWork.SaveAsync();
                if (result)
                {
                    var targetList = new Dictionary<string, string>();
                    targetList.Add("MemberVoucher", newMemberVoucher.MemberVoucherId.ToString());
                    targetList.Add("Voucher", voucher.VoucherId.ToString());

                    await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
            {"MemberId", memberId.ToString()},
            {"Type", NotificationType.VoucherReedemSuccess.ToString()},
            {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
            {"Title", $"Đã đổi thành công voucher!"},
            {"Content", $"Đã mua thành công voucher {voucher.VoucherName}"},
            {"SendNotification", "True"}
          });
                }
            }
            catch (Exception ex)
            {
                var memberId = Guid.Parse(data["MemberId"]);
                var voucherId = Guid.Parse(data["VoucherId"]);

                var targetList = new Dictionary<String, String>();
                targetList.Add("VoucherId", voucherId.ToString());

                await _redisQueueService.AddToQueue(TaskName.SendNotification, new Dictionary<string, string>() {
                {"MemberId", memberId.ToString()},
                {"Type", NotificationType.VoucherRedeemFailed.ToString()},
                { "TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                { "Title", $"Đổi voucher thất bại!"},
                { "Content", $"{ex.Message}"},
                {"SaveNotification", "False"}
              });
            }

        }

        public async Task CheckMembersLevel()
        {
            _logger.LogInformation("[ Check Members Level ] STARTED");
            var queueList = new List<QueueTask>();

            var members = await _unitOfWork.MemberRepository
              .GetQuery()
              .Where(ml => ml.MemberStatus == MemberStatus.Available)
                .Include(m => m.MemberLevels.Where(ml => ml.IsActive))
                  .ThenInclude(ml => ml.Level)
                .Include(m => m.MemberWallets.Where(mw =>
                    mw.Wallet.WalletStatus == WalletStatus.Available &&
                    mw.Wallet.WalletToken == WalletToken.XP))
                  .ThenInclude(mw => mw.Wallet)
                  .ToListAsync();

            var levels = await _unitOfWork.LevelRepository.GetQuery().OrderBy(l => l.XPNeeded).ToListAsync();

            members.ForEach(m =>
            {
                var totalXp = m.MemberWallets.Select(mw => mw.Wallet).ToList().Total(WalletToken.XP);
                MemberLevel memberCurMLevel = m.MemberLevels.First(ml => ml.IsActive);
                Level memberNewLevel = null!;

                foreach (var level in levels)
                {
                    if (level.XPNeeded > totalXp) break;
                    memberNewLevel = level;
                }

                if (memberNewLevel.LevelId != memberCurMLevel.LevelId)
                {
                    memberCurMLevel.IsActive = false;
                    m.MemberLevels.Add(new()
                    {
                        LevelId = memberNewLevel.LevelId,
                    });

                    _logger.LogInformation($"({m.EmailAddress}) : {memberCurMLevel.LevelId} [{memberCurMLevel.Level.XPNeeded}] -> {memberNewLevel.LevelId} [{memberNewLevel.XPNeeded}]");

                    var targetList = new Dictionary<string, string>();
                    targetList.Add("Member", m.MemberId.ToString());
                    targetList.Add("LevelOld", memberCurMLevel.LevelId.ToString());
                    targetList.Add("LevelNew", memberNewLevel.LevelId.ToString());

                    queueList.Add(new()
                    {
                        TaskName = TaskName.SendNotification,
                        TaskData = new Dictionary<string, string>() {
                      {"MemberId", m.MemberId.ToString()},
                      {"Type", NotificationType.VoucherReedemSuccess.ToString()},
                      {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                      {"Title", $"Bạn đã được thăng cấp"},
                      {"Content", $"Bạn đã được thăng cấp lên {memberNewLevel.LevelName}"},
                      {"SendNotification", "True"},
                      {"SaveNotification", "True"}
                        }
                    });
                }
            });

            var result = await _unitOfWork.SaveAsync();

            queueList.ForEach(async t =>
            {
                await _redisQueueService.AddToQueue(t);
            });

            _logger.LogInformation("[ Check Members Level ] DONE");
        }

        public async Task CheckDisabledMemberWallet()
        {
            _logger.LogInformation("[ Check Members Wallet Of Disabled Member ] STARTED");
            var queueList = new List<QueueTask>();

            var members = await _unitOfWork.MemberRepository
              .GetQuery()
              .Where(ml => ml.MemberStatus == MemberStatus.Disabled)
                .Include(m => m.MemberWallets.Where(mw =>
                    mw.Wallet.WalletStatus == WalletStatus.Available && mw.Wallet.Amount > 0))
                  .ThenInclude(mw => mw.Wallet)
                  .ToListAsync();

            var systemWallets = await _unitOfWork.WalletRepository.GetSystemWallets();
            var systemPointWallet = systemWallets.First(x => x.WalletToken == WalletToken.Point);
            var systemXPWallet = systemWallets.First(x => x.WalletToken == WalletToken.XP);

            members.ForEach(m =>
            {
                var wallets = m.MemberWallets.Select(x => x.Wallet).ToList();
                if (wallets.Count > 0)
                {
                    wallets.ForEach(w =>
                    {
                        if (w.WalletToken == WalletToken.Point)
                        {
                            var amount = w.Amount;
                            w.Amount = 0;
                            systemPointWallet.Amount += amount;

                            w.TransactionsFrom.Add(new Transaction()
                            {
                                FromWalletId = w.WalletId,
                                ToWalletId = systemPointWallet.WalletId,
                                TransactionType = TransactionType.MemberDisableReturnToSystem,
                                Amount = amount,
                                Note = "Tài khoản bị vô hiệu hoá trả điểm về cho hệ thống!",
                                FromAmountAfterTransaction = 0,
                                ToAmountAfterTransaction = systemPointWallet.Amount,
                                Token = WalletToken.Point
                            });
                        }
                        else if (w.WalletToken == WalletToken.XP)
                        {
                            var amount = w.Amount;
                            w.Amount = 0;
                            systemXPWallet.Amount += amount;

                            w.TransactionsFrom.Add(new Transaction()
                            {
                                FromWalletId = w.WalletId,
                                ToWalletId = systemXPWallet.WalletId,
                                TransactionType = TransactionType.MemberDisableReturnToSystem,
                                Amount = amount,
                                Note = "Tài khoản bị vô hiệu hoá trả xp về cho hệ thống!",
                                FromAmountAfterTransaction = 0,
                                ToAmountAfterTransaction = systemXPWallet.Amount,
                                Token = WalletToken.XP,
                            });
                        }
                    });
                }
            });

            var result = await _unitOfWork.SaveAsync();

            _logger.LogInformation("[ Check Wallet Of Disalbed Member ] DONE");
        }

        public async Task CheckEndedProjectWallet()
        {
            _logger.LogInformation("[ Check Wallet Of Ended Project ] STARTED");
            var queueList = new List<QueueTask>();

            var projects = await _unitOfWork.ProjectRepository
              .GetQuery()
                .Include(m => m.ProjectWallets.Where(mw =>
                    mw.Wallet.WalletStatus == WalletStatus.Available && mw.Wallet.Amount > 0))
                  .ThenInclude(mw => mw.Wallet)

              .Where(ml => ml.ProjectStatus == ProjectStatus.Ended &&
              ml.ProjectWallets.Any(mw => mw.Wallet.WalletStatus == WalletStatus.Available && mw.Wallet.Amount > 0))
                  .ToListAsync();

            var systemWallets = await _unitOfWork.WalletRepository.GetSystemWallets();
            var systemPointWallet = systemWallets.First(x => x.WalletToken == WalletToken.Point);
            var systemXPWallet = systemWallets.First(x => x.WalletToken == WalletToken.XP);

            projects.ForEach(project =>
            {
                var projectMainWallet = project.ProjectWallets.FirstOrDefault(pw => pw.Wallet.WalletTag == "main")?.Wallet ?? null;
                var projectBonusWallet = project.ProjectWallets.FirstOrDefault(pw => pw.Wallet.WalletTag == "bonus")?.Wallet ?? null;

                if (projectMainWallet != null && projectMainWallet.Amount > 0)
                {
                    var amount = projectMainWallet.Amount;

                    projectMainWallet.Amount = 0;
                    systemPointWallet.Amount += amount;

                    var trx = new Transaction()
                    {
                        FromWalletId = projectMainWallet.WalletId,
                        ToWalletId = systemPointWallet.WalletId,
                        TransactionType = TransactionType.ProjectReturnToSystem,
                        Amount = amount,
                        Note = "Điểm dự án trả về hệ thống",
                        Token = WalletToken.Point,
                        FromAmountAfterTransaction = 0,
                        ToAmountAfterTransaction = systemPointWallet.Amount,
                    };

                    projectMainWallet.WalletStatus = WalletStatus.Unavailable;
                    _unitOfWork.TransactionRepository.Add(trx);
                }

                if (projectBonusWallet != null && projectBonusWallet.Amount > 0)
                {
                    var amount = projectBonusWallet.Amount;

                    projectBonusWallet.Amount = 0;
                    systemPointWallet.Amount += amount;

                    var trx = new Transaction()
                    {
                        FromWalletId = projectBonusWallet.WalletId,
                        ToWalletId = systemPointWallet.WalletId,
                        TransactionType = TransactionType.ProjectReturnToSystem,
                        Amount = amount,
                        Note = "Điểm thưởng dự án trả về hệ thống",
                        Token = WalletToken.Point,
                        FromAmountAfterTransaction = 0,
                        ToAmountAfterTransaction = systemPointWallet.Amount,
                    };

                    projectBonusWallet.WalletStatus = WalletStatus.Unavailable;
                    _unitOfWork.TransactionRepository.Add(trx);
                }
            });

            var result = await _unitOfWork.SaveAsync();

            _logger.LogInformation("[ Check Wallet Of Ended Project  ] DONE");
        }

        public async Task CheckExpiredWallets()
        {
            var expiredWallet = await _unitOfWork.WalletRepository
            .GetQuery()
            .Where(w =>
              w.ExpiredDate < DateTimeHelper.Now() &&
              w.WalletStatus == WalletStatus.Available)
            .ToListAsync();

            var systemWallet = await _unitOfWork.WalletRepository
              .GetQuery()
              .Where(w => w.IsSystem == true && w.WalletToken == WalletToken.Point).FirstOrDefaultAsync();

            expiredWallet.ForEach(wallet =>
            {
                var diff = wallet!.Amount;

                systemWallet!.Amount += wallet.Amount;

                var trx = new Transaction()
                {
                    FromWallet = wallet!,
                    ToWallet = systemWallet!,
                    TransactionType = TransactionType.WalletExpire,
                    Note = "Wallet Expired",
                    Amount = wallet!.Amount,
                    Token = WalletToken.Point,

                    FromAmountAfterTransaction = diff,
                    ToAmountAfterTransaction = systemWallet.Amount
                };

                wallet!.WalletStatus = WalletStatus.Unavailable;
                wallet!.Amount = 0;

                wallet!.TransactionsTo.Add(trx);

                _unitOfWork.WalletRepository.Update(wallet);
            });

            await _unitOfWork.SaveAsync();
        }

        public async Task CheckExpiredVouchers()
        {
            var now = DateTimeHelper.Now();
            var expiredVouchers = await _unitOfWork.MemberVoucherRepository
                .GetQuery()
                .Where(p => p.ExpiredAt < now && p.Status == MemberVoucherStatus.Created)
                .Include(x => x.Member)
                .ToListAsync();

            var queueList = new List<QueueTask>();

            expiredVouchers.ForEach(x =>
                {
                    x.Status = MemberVoucherStatus.Expired;

                    var targetList = new Dictionary<string, string>();
                    targetList.Add("Voucher", x.VoucherId.ToString());
                    targetList.Add("MemberVoucher", x.MemberVoucherId.ToString());


                    queueList.Add(new()
                    {
                        TaskName = TaskName.SendNotification,
                        TaskData = new Dictionary<string, string>() {
                            {"MemberId", x.MemberId.ToString()},
                            {"Type", NotificationType.ProjectEnded.ToString()},
                            {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                            {"Title", $"Voucher của bạn đã hết hạn sử dụng!"},
                            {"Content", $"Voucher {x.Voucher.VoucherName} của bạn đã hết hạn!"},
                        }
                    });
                }
            );

            queueList.ForEach(async q =>
            {
                await _redisQueueService.AddToQueue(q);
            });

            await _unitOfWork.SaveAsync();
        }

        public async Task CheckExpiredProjects()
        {
            var curSalaryCycle = await _unitOfWork.SalaryCycleRepository.GetLatestSalaryCycle();
            if (curSalaryCycle is { SalaryCycleStatus: >= SalaryCycleStatus.Ongoing and <= SalaryCycleStatus.Locked })
            {
                return;
            }

            var now = DateTimeHelper.Now();
            var expiredProjects = await _unitOfWork.ProjectRepository
                .GetQuery()
                .Where(p =>
                    p.ProjectStatus == ProjectStatus.Started &&
                    p.EndedAt != null &&
                    p.EndedAt <= now)
                .Include(x => x.ProjectMember
                    .Where(x =>
                        x.Role == ProjectMemberRole.Manager &&
                        x.Status == ProjectMemberStatus.Active))
                .Include(x => x.ProjectWallets.Where(pw => pw.Wallet.WalletStatus == WalletStatus.Available))
                .ThenInclude(pw => pw.Wallet)
                .Include(x => x.ProjectEndRequests.Where(x => x.Status == ProjectEndRequestStatus.Created))
                .ToListAsync();

            var systemWallet = await _unitOfWork.WalletRepository.GetSystemWallets();
            var systemPointWallet = systemWallet.First(x => x.WalletToken == WalletToken.Point);

            var queueList = new List<QueueTask>();

            expiredProjects.ForEach(async x =>
                {
                    x.ProjectEndRequests.ForEach(re => re.Status = ProjectEndRequestStatus.Cancelled);
                    x.ProjectStatus = ProjectStatus.Ended;

                    var projectMainWallet = x.ProjectWallets.FirstOrDefault(pw => pw.Wallet.WalletTag == "main")?.Wallet ?? null;
                    var projectBonusWallet = x.ProjectWallets.FirstOrDefault(pw => pw.Wallet.WalletTag == "bonus")?.Wallet ?? null;

                    if (projectMainWallet != null && projectMainWallet.Amount > 0)
                    {
                        var amount = projectMainWallet.Amount;

                        projectMainWallet.Amount = 0;
                        systemPointWallet.Amount += amount;

                        var trx = new Transaction()
                        {
                            FromWalletId = projectMainWallet.WalletId,
                            ToWalletId = systemPointWallet.WalletId,
                            TransactionType = TransactionType.ProjectReturnToSystem,
                            Amount = amount,
                            Note = "Điểm dự án trả về hệ thống",
                            Token = WalletToken.Point,
                            FromAmountAfterTransaction = 0,
                            ToAmountAfterTransaction = systemPointWallet.Amount,
                        };

                        _unitOfWork.TransactionRepository.Add(trx);
                    }

                    if (projectBonusWallet != null && projectBonusWallet.Amount > 0)
                    {
                        var amount = projectBonusWallet.Amount;

                        projectBonusWallet.Amount = 0;
                        systemPointWallet.Amount += amount;

                        var trx = new Transaction()
                        {
                            FromWalletId = projectBonusWallet.WalletId,
                            ToWalletId = systemPointWallet.WalletId,
                            TransactionType = TransactionType.ProjectReturnToSystem,
                            Amount = amount,
                            Note = "Điểm thưởng dự án trả về hệ thống",
                            Token = WalletToken.Point,
                            FromAmountAfterTransaction = 0,
                            ToAmountAfterTransaction = systemPointWallet.Amount,
                        };

                        _unitOfWork.TransactionRepository.Add(trx);
                    }

                    var targetList = new Dictionary<string, string> { { "Project", x.ProjectId.ToString() } };
                    var member = x.ProjectMember.First();

                    queueList.Add(new QueueTask
                    {
                        TaskName = TaskName.SendNotification,
                        TaskData = new Dictionary<string, string>() {
                            {"MemberId", member.MemberId.ToString()},
                            {"Type", NotificationType.ProjectEnded.ToString()},
                            {"TargetId", $"{JsonSerializer.Serialize(targetList)}"},
                            {"Title", $"Dự án {x.ProjectName} đã kết thúc!"},
                            {"Content", $"Dự án <@Project:{x.ProjectId.ToString()}> đã kết thúc!"},
                        }
                    });
                }
            );

            var result = await _unitOfWork.SaveAsync();

            if (result)
            {
                queueList.ForEach(async q =>
                {
                    await _redisQueueService.AddToQueue(q);
                });
            }
        }
    }
}