using Bank.DAL.Enums;
using CreditService.Db.Entities;
using CreditService.Db;
using CreditService.Dtos;
using CreditService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Bank.DTO.DTOs.ServiceBusDto;
using MassTransit;
using System.Security.Principal;
using MassTransit.Transports;
using Bank.DAL.Exceptions;

namespace CreditService.Services
{
    public class CreditService : ICreditService
    {
        private readonly CreditDbContext _context;
        private readonly IRequestClient<IsUserBlockedRequest> _IsUserBlockedRequestClient;
        private readonly IRequestClient<IsUserExistRequest> _IsUserExistRequestClient;
        private readonly IRequestClient<GetUserRoleRequest> _GetUserRoleClient;
        private readonly IRequestClient<GetBankAccountBalanceRequest> _GetBankAccountBalanceClient;
        private readonly IRequestClient<GetBankAccountHistoryRequest> _GetBankAccountHistoryClient;
        private readonly IRequestClient<ConvertMoneyRequest> _ConvertMoneyClient;

        private readonly IPublishEndpoint _publishEndpoint;
        public CreditService(CreditDbContext context, IBus bus, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _IsUserBlockedRequestClient = bus.CreateRequestClient<IsUserBlockedRequest>();
            _IsUserExistRequestClient = bus.CreateRequestClient<IsUserExistRequest>();
            _GetUserRoleClient = bus.CreateRequestClient<GetUserRoleRequest>();
            _GetBankAccountBalanceClient = bus.CreateRequestClient<GetBankAccountBalanceRequest>();
            _GetBankAccountHistoryClient = bus.CreateRequestClient<GetBankAccountHistoryRequest>();
            _ConvertMoneyClient = bus.CreateRequestClient<ConvertMoneyRequest>();

            _publishEndpoint = publishEndpoint;

            EnsureMasterAccountExists().GetAwaiter().GetResult();
        }

        public async Task<CreditAccountDto> RequestCredit(CreditRequestDto request, Guid UserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (await IsUserBlockedRequest(UserId))
                    throw new InvalidOperationException("Вы заблокированы");

                var tariff = await _context.CreditTariffs.FirstOrDefaultAsync(t => t.Id == request.TariffId);
                if (tariff == null)
                {
                    throw new NotFoundException("Тариф не найден");
                }
                if (request.Amount > tariff.CreditLimit)
                {
                    throw new InvalidOperationException("Сумма кредита больше, чем позволяет тариф");
                }

                var masterAccount = await _context.CreditBankAccounts
                    .FirstOrDefaultAsync(a => a.AccountType == BankAccountType.Master);

                if (masterAccount == null)
                {
                    throw new InvalidOperationException("Мастер-счет не найден");
                }

                var masterAccountBalance = await GetBankAccountBalance(masterAccount.Id);
                if (masterAccountBalance.Balance < request.Amount)
                {
                    throw new InvalidOperationException("Недостаточно средств на мастер-счете для выдачи кредита");
                }

                var creditAccount = new CreditBankAccount
                {
                    Id = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CurrencyType = request.CurrencyType,
                    AccountNumber = request.AccountName,
                    Balance = request.Amount,
                    Debt = request.Amount,
                    AccountType = BankAccountType.Credit,
                    OwnerId = UserId,
                    AccountName = request.AccountName,
                    IsFrozen = false,
                    TariffId = request.TariffId,
                    PayingCardId = Guid.NewGuid(),
                    CreditCardId = Guid.NewGuid()
                };

                masterAccount.Balance -= request.Amount;
                if (masterAccount.Balance < 0)
                {
                    throw new InvalidOperationException("Баланс мастер-счета не может быть отрицательным");
                }

                await UpdateBankAccount(masterAccount);

                await CreateBankAccount(creditAccount);

                _context.CreditBankAccounts.Add(creditAccount);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return MapToCreditAccountDto(creditAccount);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CreditBalanceLeftDto> DepositCredit(Guid CreditId, MoneyOperationRequestDTO amount, Guid UserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (await IsUserBlockedRequest(UserId))
                    throw new InvalidOperationException("Вы заблокированы");

                var creditAccount = await _context.CreditBankAccounts.FirstOrDefaultAsync(c => c.Id == CreditId);
                if (creditAccount == null || creditAccount.IsFrozen)
                    throw new NotFoundException("Кредитный аккаунт не найден или он заморожен");
                if (creditAccount.OwnerId != UserId)
                    throw new ForbiddenException("Вы не владелец этого счёта");

                var tariff = await _context.CreditTariffs.FirstOrDefaultAsync(t => t.Id == creditAccount.TariffId);
                if (tariff == null)
                    throw new NotFoundException("Тариф не найден");

                var updatedAccount = await GetBankAccountBalance(creditAccount.Id);

                creditAccount.Debt = updatedAccount.Debt;
                creditAccount.Balance = updatedAccount.Balance;

                decimal depositAmount = (decimal)amount.Money;

                if (creditAccount.CurrencyType != amount.CurrencyType)
                {
                    var conversionResult = await ConvertMoney(
                        amount.CurrencyType,           
                        creditAccount.CurrencyType,    
                        depositAmount                  
                    );
                    depositAmount = conversionResult.Amount; 
                }

                if (depositAmount > creditAccount.Debt)
                    throw new InvalidOperationException("Сумма депозита больше, чем сумма долга");
                if (tariff.MinimumPayment > depositAmount)
                    throw new InvalidOperationException("сумма депозита меньше, чем минимальная сумма выплаты");

                var masterAccount = await _context.CreditBankAccounts.FirstOrDefaultAsync(a => a.AccountType == BankAccountType.Master);
                if (masterAccount == null)
                    throw new InvalidOperationException("Мастер-счет не найден");

                var masterAccountBalance = await GetBankAccountBalance(masterAccount.Id);
                masterAccount.Balance = masterAccountBalance.Balance;

                decimal depositAmountInMasterCurrency = depositAmount;
                if (masterAccount.CurrencyType != creditAccount.CurrencyType)
                {
                    var masterConversionResult = await ConvertMoney(
                        creditAccount.CurrencyType,   
                        masterAccount.CurrencyType,   
                        depositAmount                
                    );
                    depositAmountInMasterCurrency = masterConversionResult.Amount;
                }

                creditAccount.Balance -= depositAmount;
                if (creditAccount.Balance < 0)
                    throw new InvalidOperationException("Баланс кредитного счета не может быть отрицательным");

                masterAccount.Balance += depositAmountInMasterCurrency;
                creditAccount.Debt = Math.Max(0, creditAccount.Debt - depositAmount);

                var operation = new BankAccountOperationsHistory
                {
                    BankAccountId = CreditId,
                    OperationType = BankAccountOperationType.Replenishment,
                    OperatingMoney = depositAmount,
                    OperationDateTime = DateTime.UtcNow
                };

                await UpdateBankAccount(creditAccount);
                await UpdateBankAccount(masterAccount);

                _context.OperationsHistory.Add(operation);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new CreditBalanceLeftDto
                {
                    Id = CreditId,
                    CurrencyType = creditAccount.CurrencyType,
                    Balance = creditAccount.Balance,
                    DebitedAmount = depositAmount
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CreditBalanceLeftDto> WithdrawCredit(Guid CreditId, MoneyOperationRequestDTO amount, Guid UserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (await IsUserBlockedRequest(UserId))
                    throw new InvalidOperationException("Вы заблокированы");

                var creditAccount = await _context.CreditBankAccounts.FirstOrDefaultAsync(c => c.Id == CreditId);

                if (creditAccount == null || creditAccount.IsFrozen)
                    throw new NotFoundException("Кредитный аккаунт не найден или он заморожен");
                if (creditAccount.OwnerId != UserId)
                    throw new ForbiddenException("вы не владелец этого счёта");

                var tariff = await _context.CreditTariffs.FirstOrDefaultAsync(t => t.Id == creditAccount.TariffId);
                if (tariff == null)
                    throw new NotFoundException("Тариф не найден");

                var updatedAccount = await GetBankAccountBalance(creditAccount.Id);
                creditAccount.Debt = updatedAccount.Debt;
                creditAccount.Balance = updatedAccount.Balance;

                decimal withdrawAmount = (decimal)amount.Money;

                if (creditAccount.CurrencyType != amount.CurrencyType)
                {
                    var conversionResult = await ConvertMoney(
                        amount.CurrencyType,
                        creditAccount.CurrencyType,
                        withdrawAmount 
                    );
                    withdrawAmount = conversionResult.Amount;
                }

                if (creditAccount.Debt + withdrawAmount > tariff.CreditLimit)
                {
                    throw new InvalidOperationException("Достигнут предел кредита");
                }

                var masterAccount = await _context.CreditBankAccounts.FirstOrDefaultAsync(a => a.AccountType == BankAccountType.Master);
                if (masterAccount == null)
                    throw new InvalidOperationException("Мастер-счет не найден");

                var masterAccountBalance = await GetBankAccountBalance(masterAccount.Id);
                masterAccount.Balance = masterAccountBalance.Balance;

                decimal withdrawAmountInMasterCurrency = withdrawAmount;
                if (masterAccount.CurrencyType != creditAccount.CurrencyType)
                {
                    var masterConversionResult = await ConvertMoney(
                        creditAccount.CurrencyType,    // Валюта кредитного счета
                        masterAccount.CurrencyType,    // Валюта мастер-счета
                        withdrawAmount                 // Сумма в валюте кредитного счета
                    );
                    withdrawAmountInMasterCurrency = masterConversionResult.Amount;
                }

                if (masterAccount.Balance < withdrawAmountInMasterCurrency)
                {
                    throw new InvalidOperationException("Недостаточно средств на мастер-счете для выдачи кредита");
                }

                masterAccount.Balance -= withdrawAmountInMasterCurrency;
                if (masterAccount.Balance < 0)
                    throw new InvalidOperationException("Баланс мастер-счета не может быть отрицательным");

                creditAccount.Balance += withdrawAmount;
                creditAccount.Debt += withdrawAmount;

                var operation = new BankAccountOperationsHistory
                {
                    BankAccountId = CreditId,
                    OperationType = BankAccountOperationType.Withdrawal,
                    OperatingMoney = withdrawAmount,
                    OperationDateTime = DateTime.UtcNow
                };

                await UpdateBankAccount(creditAccount);
                await UpdateBankAccount(masterAccount);

                _context.OperationsHistory.Add(operation);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new CreditBalanceLeftDto
                {
                    Id = CreditId,
                    CurrencyType = creditAccount.CurrencyType,
                    Balance = creditAccount.Balance,
                    DebitedAmount = withdrawAmount
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CreditAccountDetailsDto> GetCreditDetails(Guid CreditId, Guid UserId, UserRole role)
        {
            if (await IsUserBlockedRequest(UserId))
                throw new InvalidOperationException("Вы заблокированы");

            var creditAccount = await _context.CreditBankAccounts
                .Include(c => c.Tariff)
                .FirstOrDefaultAsync(c => c.Id == CreditId);
            if (creditAccount == null)
            {
                throw new NotFoundException("Кредитный аккаунт не найден");
            }

            if (role != UserRole.Employee && creditAccount.OwnerId != UserId)
                throw new ForbiddenException("У вас недостаточно прав для просмотра");

            var updatedAccount = await GetBankAccountBalance(creditAccount.Id);

            creditAccount.Debt = updatedAccount.Debt;
            creditAccount.Balance = updatedAccount.Balance;

            return MapToCreditAccountDetailsDto(creditAccount);
        }

        public async Task<CreditAccountCloseDto> CloseCredit(Guid CreditId, Guid UserId)
        {
            if (await IsUserBlockedRequest(UserId))
                throw new InvalidOperationException("Вы заблокированы");

            var creditAccount = await _context.CreditBankAccounts.FirstOrDefaultAsync(c => c.Id == CreditId);
            if (creditAccount == null || creditAccount.IsFrozen || creditAccount.CloseDateTime.HasValue)
            {
                throw new InvalidOperationException("Кредитный аккаунт не найден или он заморожен или он уже закрыт");
            }
            if (creditAccount.OwnerId != UserId)
                throw new InvalidOperationException("Вы не владелец счёта");

            var updatedAccount = await GetBankAccountBalance(creditAccount.Id);

            creditAccount.Debt = updatedAccount.Debt;
            creditAccount.Balance = updatedAccount.Balance;

            if (creditAccount.Debt > 0)
            {
                throw new InvalidOperationException("Кредит не может быть закрыт, так как долг всё ещё не оплачен");
            }

            creditAccount.IsFrozen = true;
            creditAccount.CloseDateTime = DateTime.UtcNow;
            await CloseBankAccount(creditAccount);
            await _context.SaveChangesAsync();

            return new CreditAccountCloseDto
            {
                Id = CreditId,
                CreateDate = creditAccount.CreateDateTime,
                CloseDate = (DateTime)creditAccount.CloseDateTime,
                IsFrozen = creditAccount.IsFrozen,
                OwnerId = creditAccount.OwnerId.ToString()
            };
        }

        public async Task<List<CreditOperationHistoryDto>> GetOverduePayments(Guid CreditId, Guid UserId, bool addSuccessPayments)
        {
            if (await IsUserBlockedRequest(UserId))
                throw new InvalidOperationException("Вы заблокированы");

            await UpdateBankAccountHistory(CreditId);

            if (addSuccessPayments)
            {
                var allOperations = await _context.OperationsHistory
                .Where(o => o.BankAccountId == CreditId && o.OperationType == BankAccountOperationType.LoanRepayment)
                .Select(o => new CreditOperationHistoryDto
                {
                    BankAccountId = o.BankAccountId,
                    BankAccountOperationType = o.OperationType,
                    BankAccountOperationStatus = o.OperationStatus,
                    BankAccountOperationInitiator = o.OperationInitiator,
                    CurrentBalance = o.CurrentBalance,
                    PreviousBalance = o.PreviousBalance,
                    OperatingMoney = o.OperatingMoney,
                    OperationDateTime = o.OperationDateTime,
                    UserId = o.UserId
                })
                .OrderBy(o => o.OperationDateTime)
                .ToListAsync();

                return allOperations;
            }

            var operations = await _context.OperationsHistory
                .Where(o => o.BankAccountId == CreditId && o.OperationStatus == BankAccountOperationStatus.Reject && o.OperationType == BankAccountOperationType.LoanRepayment)
                .Select(o => new CreditOperationHistoryDto
                {
                    BankAccountId = o.BankAccountId,
                    BankAccountOperationType = o.OperationType,
                    BankAccountOperationStatus = o.OperationStatus,
                    BankAccountOperationInitiator = o.OperationInitiator,
                    CurrentBalance = o.CurrentBalance,
                    PreviousBalance = o.PreviousBalance,
                    OperatingMoney = o.OperatingMoney,
                    OperationDateTime = o.OperationDateTime,
                    UserId = o.UserId
                })
                .OrderBy(o => o.OperationDateTime)  
                .ToListAsync();

            return operations;
        }

        public async Task<TariffDto> CreateCreditTariff(TariffCreateDto tariffCreateDto, Guid UserId)
        {
            if (await IsUserBlockedRequest(UserId))
                throw new InvalidOperationException("Вы заблокированы");

            var tariff = new CreditTariff
            {
                Name = tariffCreateDto.Name,
                InterestRate = (double)tariffCreateDto.InterestRate,
                CreditLimit = tariffCreateDto.CreditLimit,
                MinimumPayment = tariffCreateDto.MinimumPayment,
                PaymentType = tariffCreateDto.PaymentType
            };

            _context.CreditTariffs.Add(tariff);
            await CreateTariff(tariff);
            await _context.SaveChangesAsync();

            return MapToTariffDto(tariff);
        }

        public async Task<List<TariffDto>> GetAllTariffs(Guid UserId)
        {
            if (await IsUserBlockedRequest(UserId))
                throw new InvalidOperationException("Вы заблокированы");

            var tariffs = await _context.CreditTariffs.ToListAsync();

            List<TariffDto> tariffDtos = new List<TariffDto>();
            foreach (var tariff in tariffs)
            {
                tariffDtos.Add(MapToTariffDto(tariff));
            }

            return tariffDtos;
        }

        public async Task<List<CreditAccountDto>> GetUserCredits(Guid UserId, Guid ThisUserId)
        {
            if (await IsUserBlockedRequest(ThisUserId))
                throw new InvalidOperationException("Вы заблокированы");

            var credits = await _context.CreditBankAccounts
                .Where(c => c.OwnerId == UserId)
                .ToListAsync();

            foreach (var credit in credits)
            {
                var updatedAccount = await GetBankAccountBalance(credit.Id);

                credit.Debt = updatedAccount.Debt;
                credit.Balance = updatedAccount.Balance;
            }

            List<CreditAccountDto> creditDtos = new List<CreditAccountDto>();
            foreach (var credit in credits)
            {
                creditDtos.Add(MapToCreditAccountDto(credit));
            }


            return creditDtos;
        }

        public async Task<CreditRatingDto> GetCreditRating(Guid UserId, Guid ThisUserId, UserRole ThisUserRole)
        {
            if (await IsUserBlockedRequest(ThisUserId))
                throw new InvalidOperationException("Вы заблокированы");

            if (!await IsUserExistRequest(UserId))
                throw new NotFoundException("Пользователь не найден");

            if (!(ThisUserRole == UserRole.Employee || (UserId == ThisUserId)))
                throw new ForbiddenException("У вас недостаточно прав для просмотра кредитного рейтинга");

            var credits = await _context.CreditBankAccounts
                .Where(c => c.OwnerId == UserId)
                .ToListAsync();

            foreach (var credit in credits)
            {
                var updatedAccount = await GetBankAccountBalance(credit.Id);
                credit.Debt = updatedAccount.Debt;
                credit.Balance = updatedAccount.Balance;
            }

            if (credits.Count == 0)
            {
                return new CreditRatingDto
                {
                    UserId = UserId,
                    CreditScore = 50,
                    TotalCredits = 0,
                    OverduePayments = 0,
                    RatingDescription = "У пользователя нет кредитной истории."
                };
            }

            // Получаем все просроченные платежи по всем кредитам пользователя
            int overduePayments = 0;
            foreach (var credit in credits)
            {
                await UpdateBankAccountHistory(credit.Id);
                var overdue = await _context.OperationsHistory
                    .Where(o => o.BankAccountId == credit.Id
                             && o.OperationStatus == BankAccountOperationStatus.Reject
                             && o.OperationType == BankAccountOperationType.LoanRepayment)
                    .CountAsync();
                overduePayments += overdue;
            }

            int creditScore = 100;

            // Уменьшаем рейтинг за каждый просроченный платеж
            creditScore -= overduePayments * 10;

            // Уменьшаем рейтинг за незакрытые кредиты с долгом
            int activeCreditsWithDebt = credits.Count(c => !c.CloseDateTime.HasValue && c.Debt > 0);
            creditScore -= activeCreditsWithDebt * 5;

            creditScore = Math.Max(0, Math.Min(100, creditScore));

            string ratingDescription = creditScore switch
            {
                100 => "Идеальный заемщик: все кредиты закрыты, просрочек нет.",
                >= 80 => "Хороший заемщик: минимальное количество просрочек.",
                >= 50 => "Средний заемщик: есть просрочки или незакрытые кредиты.",
                >= 20 => "Рискованный заемщик: значительное количество просрочек.",
                _ => "Ненадежный заемщик: большое количество просрочек и незакрытых кредитов."
            };

            return new CreditRatingDto
            {
                UserId = UserId,
                CreditScore = creditScore,
                TotalCredits = credits.Count,
                OverduePayments = overduePayments,
                RatingDescription = ratingDescription
            };
        }

        private async Task UpdateBankAccountHistory(Guid BankAccountId)
        {
            var response = await GetAccountCreditHistory(BankAccountId);

            foreach (var log in response.getBankAccountHistoryCommandDtos)
            {
                var exist = await _context.OperationsHistory.AnyAsync(o => o.OperationDateTime == log.OperationDateTime && o.BankAccountId == log.BankAccountId);

                if (!exist)
                {
                    await _context.OperationsHistory.AddAsync(MapToBankAccountOperationsHistory(log));
                }
                await _context.SaveChangesAsync();
            }
        }
        private async Task EnsureMasterAccountExists()
        {
            var masterAccount = await _context.CreditBankAccounts
                .FirstOrDefaultAsync(a => a.AccountType == BankAccountType.Master);

            var masterTariff = await _context.CreditTariffs
                .FirstOrDefaultAsync(a => a.Name == "MasterTariff");

            if (masterTariff == null)
            {
                masterTariff = new CreditTariff
                {
                    Id = Guid.NewGuid(),
                    Name = "MasterTariff",
                    InterestRate = 0,
                    CreditLimit = decimal.MaxValue,
                    MinimumPayment = 0,
                    PaymentType = PaymentType.Monthly,
                };

                _context.CreditTariffs.Add(masterTariff);
                await _context.SaveChangesAsync();

            }
            await CreateTariff(masterTariff);

            if (masterAccount == null)
            {
                masterAccount = new CreditBankAccount
                {
                    Id = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    AccountNumber = "MasterAccount",
                    CurrencyType = "USD",
                    Balance = 1_000_000m,
                    Debt = 0,
                    AccountType = BankAccountType.Master,
                    OwnerId = Guid.NewGuid(),
                    AccountName = "MasterAccount",
                    IsFrozen = false,
                    TariffId = masterTariff.Id,
                    PayingCardId = Guid.NewGuid(),
                    CreditCardId = Guid.NewGuid()
                };

                _context.CreditBankAccounts.Add(masterAccount);
                await _context.SaveChangesAsync();

            }

            await CreateBankAccount(masterAccount);
        }

        #region requests
        private async Task CreateTariff(CreditTariff tariff)
        {
            var @event = new CreateTariffEvent
            {
                Id = tariff.Id,
                Name = tariff.Name,
                InterestRate = tariff.InterestRate,
                CreditLimit = tariff.CreditLimit,
                MinimumPayment = tariff.MinimumPayment,
                PaymentType = tariff.PaymentType
            };
            try
            {
                await _publishEndpoint.Publish(@event);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось уведомить о создании тарифа");
            }
        }
        private async Task CloseBankAccount(CreditBankAccount account)
        {
            var @event = new CloseBankAccountEvent
            {
                Id = account.Id,
                IsFrozen = account.IsFrozen,
                CloseDateTime = (DateTime)account.CloseDateTime,
            };
            try
            {
                await _publishEndpoint.Publish(@event);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось уведомить о закрытии счета");
            }
        }
        private async Task UpdateBankAccount(CreditBankAccount account)
        {
            var @event = new UpdateBankAccountEvent
            {
                Id = account.Id,
                Balance = account.Balance,
                Debt = account.Debt,
            };
            try
            {
                await _publishEndpoint.Publish(@event);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось уведомить об обновлении счета");
            }
        }
        private async Task CreateBankAccount(CreditBankAccount account)
        {
            var @event = new CreateBankAccountEvent 
            {
                Id = account.Id,
                AccountName = account.AccountName,
                AccountNumber = account.AccountNumber,
                TariffId = account.TariffId,
                IsFrozen = account.IsFrozen,
                CurrencyType = account.CurrencyType,
                Balance = account.Balance,
                Debt = account.Debt,
                AccountType = account.AccountType,
                OwnerId = account.OwnerId,
            };
            try
            {
                await _publishEndpoint.Publish(@event);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось уведомить о создании счета");
            }
        }
        private async Task<GetBankAccountHistoryCommand> GetAccountCreditHistory(Guid BankAccountId)
        {
            var response = await _GetBankAccountHistoryClient.GetResponse<GetBankAccountHistoryCommand>(new GetBankAccountHistoryRequest { BankAccountId = BankAccountId });

            return response.Message;
        }
        private async Task<GetBankAccountBalanceCommand> GetBankAccountBalance(Guid BankAccountId)
        {
            var response = await _GetBankAccountBalanceClient.GetResponse<GetBankAccountBalanceCommand>(new GetBankAccountBalanceRequest { BankAccountId = BankAccountId});

            return response.Message;
        }

        private async Task<ConvertMoneyCommand> ConvertMoney(string RequestCurrencyName, string AccountCurrencyName, decimal RequestCurrencyAmount)
        {
            var response = await _ConvertMoneyClient.GetResponse<ConvertMoneyCommand>(new ConvertMoneyRequest { RequestCurrencyAmount = RequestCurrencyAmount, AccountCurrencyName = AccountCurrencyName, RequestCurrencyName = RequestCurrencyName });

            return response.Message;
        }

        private async Task<bool> IsUserBlockedRequest(Guid UserId)
        {
            var response = await _IsUserBlockedRequestClient.GetResponse<IsUserBlockedCommand>(new IsUserBlockedRequest {UserId = UserId });

            return response.Message.IsBlocked;
        }

        private async Task<bool> IsUserExistRequest(Guid UserId)
        {
            var response = await _IsUserExistRequestClient.GetResponse<IsUserExistCommand>(new IsUserExistRequest { UserId = UserId });

            return response.Message.IsExist;
        }

        private async Task<UserRole> GetUserRole(Guid UserId)
        {
            var response = await _GetUserRoleClient.GetResponse<GetUserRoleCommand>(new GetUserRoleRequest { UserId = UserId });

            return response.Message.Role;
        }
        #endregion

        #region mappers
        private CreditAccountDto MapToCreditAccountDto(CreditBankAccount account)
        {
            var tariff = _context.CreditTariffs.FirstOrDefault(t => t.Id == account.TariffId);
            return new CreditAccountDto
            {
                Id = account.Id,
                IsFrozen = account.IsFrozen,
                CurrencyType = account.CurrencyType,
                Balance = account.Balance,
                Debt = account.Debt,
                BankAccountType = account.AccountType.ToString(),
                OwnerId = account.OwnerId.ToString(),
                Tariff = new TariffBriefDto
                {
                    Id = account.TariffId,
                    Name = tariff.Name,
                    InterestRate =tariff.InterestRate,
                    PaymentType = tariff.PaymentType.ToString(),
                }
            };
        }

        private CreditAccountDetailsDto MapToCreditAccountDetailsDto(CreditBankAccount account)
        {
            return new CreditAccountDetailsDto
            {
                Id = account.Id,
                CreateDateTime = account.CreateDateTime,
                IsFrozen = account.IsFrozen,
                CurrencyType = account.CurrencyType,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Debt = account.Debt,
                BankAccountType = account.AccountType.ToString(),
                OwnerId = account.OwnerId.ToString(),
                Tariff = MapToTariffDto(account.Tariff),
                PayingCardId = account.PayingCardId,
                CreditCardId = account.CreditCardId
            };
        }

        private TariffDto MapToTariffDto(CreditTariff tariff)
        {
            return new TariffDto
            {
                Id = tariff.Id,
                Name = tariff.Name,
                InterestRate = tariff.InterestRate,
                CreditLimit = tariff.CreditLimit,
                MinimumPayment = tariff.MinimumPayment,
                PaymentType = tariff.PaymentType.ToString()
            };
        }

        private BankAccountOperationsHistory MapToBankAccountOperationsHistory(GetBankAccountHistoryCommandDto dto)
        {
            return new BankAccountOperationsHistory
            {
                Id = Guid.NewGuid(),
                OperationType = dto.OperationType,
                OperatingMoney = dto.OperatingMoney,
                CurrentBalance = dto.CurrentBalance,
                PreviousBalance = dto.PreviousBalance,
                OperationDateTime = dto.OperationDateTime,
                OperationInitiator = dto.OperationInitiator,
                OperationStatus = dto.OperationStatus,
                UserId = dto.UserId,
                BankAccountId = dto.BankAccountId
            };
        }
        private List<BankAccountOperationsHistory> MapRangeToBankAccountOperationsHistory(List<GetBankAccountHistoryCommandDto> dto)
        {
            List<BankAccountOperationsHistory> result = new List<BankAccountOperationsHistory>();
            foreach (var log in dto)
            {
                result.Add(MapToBankAccountOperationsHistory(log));
            }
            return result;
        }
        #endregion
    }
}
