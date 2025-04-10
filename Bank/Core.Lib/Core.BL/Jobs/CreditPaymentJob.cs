using Bank.DAL.Enums;
using Core.DAL;
using Core.DAL.Models.History;
using Core.DAL.Options;
using MassTransit.Configuration;
using MassTransit.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Core.BL.Jobs
{
    [DisallowConcurrentExecution]
    public class CreditPaymentJob : IJob
    {
        private CoreDbContext _coreDbContext;
        private CoreJobsConfig _config;
        private ILogger<CreditPaymentJob> _logger;

        public CreditPaymentJob(CoreDbContext coreDbContext, IOptionsMonitor<CoreJobsConfig> config, ILogger<CreditPaymentJob> logger)
        {
            _coreDbContext = coreDbContext;
            _config = config.CurrentValue;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.UtcNow;

            var masterAccount = await _coreDbContext.BankAccounts
                .FirstOrDefaultAsync(a => a.BankAccountType == BankAccountType.Master);
            if (masterAccount == null)
            {
                _logger.LogError("Мастер-счет не найден. Операция прервана.");
                return;
            }

            var creditBankAccount = _coreDbContext.CreditBankAccounts
                                    .Include(x => x.Tariff)
                                    .Where(x => !x.CloseDateTime.HasValue)
                                    .Where(x => !x.IsFrozen)
                                    .Where(x => x.Debt > 0)
                                    .Where(x => x.CreateDateTime <= now)
                                    .OrderByDescending(x => x.CreateDateTime)
                                    .AsSplitQuery()
                                    .AsQueryable();

            int currentPage = 0;
            var batchSize = _config.BatchSize != 0 ? _config.BatchSize : 200;
            int creditsCount = await creditBankAccount.CountAsync();
            int numberOfPages = (int)Math.Ceiling(creditsCount / (batchSize * 1.0));

            if (numberOfPages == 0)
            {
                _logger.LogInformation("No banks to take money from");
                return;
            }

            while (currentPage < numberOfPages)
            {
                using var transaction = await _coreDbContext.Database.BeginTransactionAsync();
                try
                {
                    var creditBankAccountsList = await creditBankAccount.Skip(currentPage * batchSize)
                                            .Take(batchSize)
                                            .ToListAsync();

                    List<Guid> bankIds = creditBankAccountsList.Select(x => x.Id).ToList();

                    //await _coreDbContext.Database.ExecuteSqlAsync($@"SELECT * FROM ""CreditBankAccounts"" WHERE ""Id"" IN ({string.Join(",", bankIds.Select(id => $"'{id}'"))}) FOR SHARE;");

                    var bankAccountsHistory = await _coreDbContext.BankAccountOperationsHistory
                                             .Where(x => bankIds.Contains(x.BankAccountId))
                                             .Where(x => x.BankAccountOperationType == BankAccountOperationType.LoanRepayment)
                                             .OrderByDescending(x => x.OperationDateTime)
                                             .ToListAsync();

                    bankAccountsHistory = bankAccountsHistory
                                            .DistinctBy(x => x.BankAccountId)
                                            .ToList();
                    foreach (var credit in creditBankAccountsList)
                    {
                        if (credit.IsFrozen == true || credit.CloseDateTime != null || credit.Debt == 0)
                        {
                            continue;
                        }

                        var lastOperation = bankAccountsHistory
                                        .FirstOrDefault(x => x.BankAccountId == credit.Id && x.BankAccountOperationType == BankAccountOperationType.LoanRepayment);

                        if (lastOperation != null)
                        {
                            //TimeSpan operationTimeDeltaTemp = now - lastOperation.OperationDateTime;
                            //bool needToPayTemp = credit.Tariff.PaymentType switch
                            //{
                            //    PaymentType.Daily => operationTimeDeltaTemp.TotalDays >= 1,
                            //    PaymentType.Weekly => operationTimeDeltaTemp.TotalDays >= 7,
                            //    PaymentType.Monthly => operationTimeDeltaTemp.TotalDays >= 30,
                            //    _ => throw new Exception("Wrong payment type")
                            //};

                            bool needToPayTemp = (now - lastOperation.OperationDateTime).TotalSeconds > 20;
                            if (!needToPayTemp)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //TimeSpan operationTimeDeltaTemp = now - credit.CreateDateTime;
                            //bool needToPayTemp = credit.Tariff.PaymentType switch
                            //{
                            //    PaymentType.Daily => operationTimeDeltaTemp.TotalDays >= 1,
                            //    PaymentType.Weekly => operationTimeDeltaTemp.TotalDays >= 7,
                            //    PaymentType.Monthly => operationTimeDeltaTemp.TotalDays >= 30,
                            //    _ => throw new Exception("Wrong payment type")
                            //};
                            bool needToPayTemp = (now - credit.CreateDateTime).TotalSeconds > 20;
                            if (!needToPayTemp)
                            {
                                continue;
                            }
                        }

                        decimal previousDebt = credit.Debt;
                        decimal paymentAmount = credit.Tariff.MinimumPayment;

                        if (paymentAmount > credit.Debt)
                        {
                            paymentAmount = credit.Debt;
                        }

                        if (credit.Balance < paymentAmount)
                        {
                            var overDuePaymentHistoryRecord = new BankAccountOperationsHistory
                            {
                                BankAccountOperationType = BankAccountOperationType.LoanRepayment,
                                OperatingMoney = paymentAmount,
                                CurrentBalance = credit.Debt,
                                PreviousBalance = credit.Debt,
                                OperationDateTime = DateTime.UtcNow.AddMilliseconds(1),
                                BankAccountOperationInitiator = BankAccountOperationInitiator.System,
                                BankAccountOperationStatus = BankAccountOperationStatus.Reject,

                                BankAccountId = credit.Id,
                                UserId = credit.OwnerId
                            };
                            await _coreDbContext.BankAccountOperationsHistory.AddAsync(overDuePaymentHistoryRecord);

                            _logger.LogWarning($"Недостаточно средств на счете {credit.Id} для выплаты минимального платежа. Требуется: {paymentAmount}, доступно: {credit.Balance}");
                            continue;
                        }

                        credit.Debt = Math.Max(0, credit.Debt - paymentAmount);
                        credit.Balance -= paymentAmount;

                        if (credit.Balance < 0)
                        {
                            _logger.LogError($"Баланс кредитного счета {credit.Id} не может быть отрицательным. Операция отменена.");
                            throw new InvalidOperationException("Баланс кредитного счета не может быть отрицательным");
                        }

                        masterAccount.Balance += paymentAmount;

                        var historyRecord = new BankAccountOperationsHistory
                        {
                            BankAccountOperationType = BankAccountOperationType.LoanRepayment,
                            OperatingMoney = paymentAmount,
                            CurrentBalance = credit.Debt,
                            PreviousBalance = previousDebt,
                            OperationDateTime = DateTime.UtcNow.AddMilliseconds(1),
                            BankAccountOperationInitiator = BankAccountOperationInitiator.System,
                            BankAccountOperationStatus = BankAccountOperationStatus.Success,

                            BankAccountId = credit.Id,
                            UserId = credit.OwnerId
                        };

                        await _coreDbContext.BankAccountOperationsHistory.AddAsync(historyRecord);
                    }

                    await _coreDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"page: {currentPage}, size: {creditsCount}, batch: {batchSize}");
                    currentPage++;
                }
                catch(Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e.Message);
                }
            }
        }
    }
}
