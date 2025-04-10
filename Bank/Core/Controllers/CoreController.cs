using Bank.DAL.Enums;
using Bank.DTO.DTOs.Other;
using Core.BL.CQRS.Commands.ChangeBankAccountNameCommand;
using Core.BL.CQRS.Commands.CloseCardBankAccount;
using Core.BL.CQRS.Commands.CreateNewCardBankAccount;
using Core.BL.CQRS.Commands.PutMoneyOnBankAccount;
using Core.BL.CQRS.Commands.PutMoneyOnCreditBankAccount;
using Core.BL.CQRS.Commands.TransferMoneyBetweenBankAccounts;
using Core.BL.CQRS.Commands.WithdrawMoneyFromCardBankAccount;
using Core.BL.CQRS.Commands.WithdrawMoneyFromCreditBankAccount;
using Core.BL.CQRS.Queries.GetCardBankAccountDetails;
using Core.BL.CQRS.Queries.GetCreditBankAccountDetails;
using Core.BL.CQRS.Queries.GetMyBankAccounts;
using Core.BL.CQRS.Queries.GetUserBankAccounts;
using Core.Controllers.BasedController;
using Core.DAL;
using Core.DTO.DTOs.Requests.BankAccount;
using Core.DTO.DTOs.Requests.History;
using Core.DTO.DTOs.Responses.Aggregates;
using Core.DTO.DTOs.Responses.BankAccounts;
using Core.DTO.DTOs.Responses.Cards;
using Core.DTO.DTOs.Responses.History;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Serialization;

namespace Core.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    public class CoreController : CoreControllerExtended
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CoreDbContext _context;

        public CoreController(IMediator mediator, ILogger<CoreControllerExtended> logger, IHttpClientFactory httpClientFactory, CoreDbContext context) : base(mediator, logger)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        [Route("user/accounts/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserBankAccountsResponseDTO))]
        [Produces("application/json")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetUserBankAccounts([FromRoute] Guid userId)
        {
            try
            {
                var request = new GetUserBankAccountsRequest 
                {
                    UserId = userId
                };
                return Ok(await Send<GetUserBankAccountsRequest, UserBankAccountsResponseDTO>(request));
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("user/accounts/my")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserBankAccountsResponseDTO))]
        [Produces("application/json")]
        public async Task<IActionResult> GetMyBankAccounts([FromQuery] List<Guid>? accountsIds)
        {
            try
            {
                return Ok(await SendWithMedata<GetMyBankAccountsRequest, UserBankAccountsResponseDTO>(new GetMyBankAccountsRequest() { AccountsIds = accountsIds ?? []}));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [Route("accounts/all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankAccountsListDTO))]
        [Produces("application/json")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetAllBankAccounts([FromQuery] AllBankAccountsQueryDTO query)
        {
            try
            {
                return Ok(await Send<AllBankAccountsQueryDTO, BankAccountsListDTO>(query));
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("account/card/{accountId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>))]
        [Produces("application/json")]
        public async Task<IActionResult> GetCardBankAccountDetails([FromRoute] Guid accountId)
        {
            try
            {
                var request = new GetCardBankAccountDetailsRequest
                {
                    AccountId = accountId
                };
                return Ok(await SendWithMedata<GetCardBankAccountDetailsRequest, BankAccountCardDTO<BankAccountResponseDTO, CardResponseDTO>>(request));
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("account/credit/{accountId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>))]
        [Produces("application/json")]
        public async Task<IActionResult> GetCreditBankAccountDetails([FromRoute] Guid accountId)
        {
            try
            {
                var request = new GetCreditBankAccountDetailsRequest
                {
                    AccountId = accountId
                };
                return Ok(await SendWithMedata<GetCreditBankAccountDetailsRequest, BankAccountCardDTO<CreditBankAccountFullResponseDTO, CreditCardResponseDTO>>(request));
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("accounts/history")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankAccountOperationsHistoryListDTO))]
        [Produces("application/json")]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetBankAccountsOperationsHistory([FromQuery] BankAccountOperationsHistoryQueryDTO query)
        {
            try
            {
                return Ok(await Send<BankAccountOperationsHistoryQueryDTO, BankAccountOperationsHistoryListDTO> (query));
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("accounts/history/my")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankAccountOperationsHistoryListDTO))]
        [Produces("application/json")]
        public async Task<IActionResult> GetBankAccountOperationsHistoryPersonal([FromQuery] BankAccountOperationsHistoryQueryPersonalDTO query)
        {
            try
            {
                return Ok(await SendWithMedata<BankAccountOperationsHistoryQueryPersonalDTO, BankAccountOperationsHistoryListDTO>(query));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/card/deposit/{accountId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> PutMoneyOnCardBankAccount([FromBody] BankAccountMoneyOperationRequestDTO request, [FromRoute] Guid accountId)
        {
            try
            {
                var newRequest = new PutMoneyOnCardBankAccountRequest
                {
                    AccountId = accountId,
                    RequestDTO = request
                };
                return Ok(await SendWithMedata<PutMoneyOnCardBankAccountRequest, PutMoneyOnCardBankAccountResponse>(newRequest));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/card/withdraw/{accountId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> WithdrawMoneyFromCardBankAccount([FromBody] BankAccountMoneyOperationRequestDTO request, [FromRoute] Guid accountId)
        {
            try
            {
                var newRequest = new WithdrawMoneyFromCardBankAccountRequest
                {
                    AccountId = accountId,
                    RequestDTO = request
                };
                return Ok(await SendWithMedata<WithdrawMoneyFromCardBankAccountRequest, WithdrawMoneyFromCardBankAccountResponse>(newRequest));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/credit/deposit/{accountId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> PutMoneyOnCreditBankAccount([FromBody] BankAccountMoneyOperationRequestDTO request, [FromRoute] Guid accountId)
        {
            try
            {
                var newRequest = new PutMoneyOnCreditBankAccountRequest
                {
                    AccountId = accountId,
                    RequestDTO = request
                };
                return Ok(await SendWithMedata<PutMoneyOnCreditBankAccountRequest, PutMoneyOnCreditBankAccountResponse>(newRequest));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/credit/withdraw/{accountId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> WithdrawMoneyFromCreditBankAccount([FromBody] BankAccountMoneyOperationRequestDTO request, [FromRoute] Guid accountId)
        {
            try
            {
                var newRequest = new WithdrawMoneyFromCreditBankAccountRequest
                {
                    AccountId = accountId,
                    RequestDTO = request
                };
                return Ok(await SendWithMedata<WithdrawMoneyFromCreditBankAccountRequest, WithdrawMoneyFromCreditBankAccountResponse>(newRequest));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/card/create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateNewCardBankAccountResponse))]
        [Produces("application/json")]
        public async Task<IActionResult> CreateNewCardBankAccount([FromBody] DebitCardBankAccountCreateRequestDTO request)
        {
            try
            {
                return Ok(await SendWithMedata<DebitCardBankAccountCreateRequestDTO, CreateNewCardBankAccountResponse>(request));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("account/{accountId:guid}/change/name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> ChangeBankAccountName([FromBody] ChangeBankAccountNameRequestDTO request, [FromRoute] Guid accountId)
        {
            try
            {
                var newRequest = new ChangeBankAccountNameRequest
                {
                    AccountName = request.AccountName,
                    AccountId = accountId
                };
                return Ok(await SendWithMedata<ChangeBankAccountNameRequest, ChangeBankAccountNameResponse>(newRequest));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/card/{accountId:guid}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> CloseCardBankAccount([FromRoute] Guid accountId)
        {
            try
            {
                var newRequest = new CloseCardBankAccountRequest
                {
                    AccountId = accountId
                };
                return Ok(await SendWithMedata<CloseCardBankAccountRequest, CloseCardBankAccountResponse>(newRequest));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("account/transfer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> TransferMoneyBankAccount([FromBody] TransferMoneyBetweenBankAccountsRequest request)
        {
            try
            {
                return Ok(await SendWithMedata<TransferMoneyBetweenBankAccountsRequest, TransferMoneyBetweenBankAccountsResponse>(request));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }


        [HttpGet]
        [Route("account/curs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurs()
        {
            var bankAccounts = await _context.BankAccounts.ToListAsync();
            var currencies = await _context.Currencies.ToListAsync();

            foreach(var bankAccount in bankAccounts)
            {
                var currency = currencies.FirstOrDefault(x => x.VchCode == bankAccount.CurrencyType);
                if(currency != null)
                {
                    bankAccount.Currency = currency;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
            /*try
            {
                var client = _httpClientFactory.CreateClient();

                var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

                string requestSoapContent = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <GetCursOnDateXML xmlns=""http://web.cbr.ru/"">
      <On_date>{currentTime}</On_date>
    </GetCursOnDateXML>
  </soap12:Body>
</soap12:Envelope>";

                var stringContent = new StringContent(requestSoapContent, Encoding.UTF8, "application/soap+xml");

                var response = await client.PostAsync("https://www.cbr.ru/DailyInfoWebServ/DailyInfo.asmx", stringContent);

                var stream = await response.Content.ReadAsStreamAsync();

                var xml = new XmlSerializer(typeof(CurrencyDtoFromBankRf)).Deserialize(stream);

                return Ok(xml);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }*/
        }
    }
}
