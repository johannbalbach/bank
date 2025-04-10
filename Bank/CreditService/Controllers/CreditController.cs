using Bank.DAL.Enums;
using CreditService.Dtos;
using CreditService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CreditService.Controllers
{
    [ApiController]
    [Route("credit/[controller]")]
    public class CreditController : ControllerBase
    {
        private readonly ICreditService _service;

        public CreditController(ICreditService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("RequestCredit")]
        [Authorize]
        public async Task<CreditAccountDto> RequestCredit(CreditRequestDto request)
        {
            return await _service.RequestCredit(request, GetUserId());
        }

        [HttpPost]
        [Route("{CreditId}/deposit")]
        [Authorize]
        public async Task<CreditBalanceLeftDto> PayCredit([FromRoute] Guid CreditId, MoneyOperationRequestDTO req)
        {
            return await _service.DepositCredit(CreditId, req, GetUserId());

        }

        [HttpPost]
        [Route("{CreditId}/withdraw")]
        [Authorize]
        public async Task<CreditBalanceLeftDto> WithdrawCredit([FromRoute] Guid CreditId, MoneyOperationRequestDTO req)
        {
            return await _service.WithdrawCredit(CreditId, req, GetUserId());
        }

        [HttpGet]
        [Route("{CreditId}/details")]
        [Authorize]
        public async Task<CreditAccountDetailsDto> GetCreditDetails([FromRoute] Guid CreditId)
        {
            var userRoleClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRoleClaim == null)
                throw new Exception("Token not found");
            if (userRoleClaim == UserRole.Client.ToString())
                return await _service.GetCreditDetails(CreditId, GetUserId(), UserRole.Client);
            else if (userRoleClaim == UserRole.Employee.ToString())
                return await _service.GetCreditDetails(CreditId, GetUserId(), UserRole.Employee);
            else
                throw new Exception("Token not found");
        }

        [HttpPost]
        [Route("{CreditId}/close")]
        [Authorize]
        public async Task<CreditAccountCloseDto> CloseCredit([FromRoute] Guid CreditId)
        {
            return await _service.CloseCredit(CreditId, GetUserId());
        }

        [HttpPost]
        [Route("CreateTariff")]
        [Authorize(Roles = "Employee")]
        public async Task<TariffDto> CreateCreditTariff([FromBody] TariffCreateDto tariffCreateDto)
        {
            return await _service.CreateCreditTariff(tariffCreateDto, GetUserId());
        }

        [HttpGet]
        [Route("GetAllTariffs")]
        public async Task<List<TariffDto>> GetAllTariffs()
        {
            return await _service.GetAllTariffs(GetUserId());
        }

        [HttpGet]
        [Route("GetOverduePayments")]
        [Authorize]
        public async Task<List<CreditOperationHistoryDto>> GetOverduePayments(Guid CreditId, bool addSuccessPayments)
        {
            return await _service.GetOverduePayments(CreditId, GetUserId(), addSuccessPayments);
        }

        [HttpGet]
        [Route("GetCreditRaiting/{UserId}")]
        [Authorize]
        public async Task<CreditRatingDto> GetCreditRaiting([FromRoute ]Guid UserId)
        {

            var userRoleClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRoleClaim == null)
                throw new Exception("Token not found");
            if (userRoleClaim == UserRole.Client.ToString())
                return await _service.GetCreditRating(UserId, GetUserId(), UserRole.Client);
            else if (userRoleClaim == UserRole.Employee.ToString())
                return await _service.GetCreditRating(UserId, GetUserId(), UserRole.Employee);
            else
                throw new Exception("Token not found");
           
        }
        [HttpGet]
        [Route("GetCreditRaiting/My")]
        [Authorize]
        public async Task<CreditRatingDto> GetCreditRaiting()
        {

            var userRoleClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (userRoleClaim == null)
                throw new Exception("Token not found");
            if (userRoleClaim == UserRole.Client.ToString())
                return await _service.GetCreditRating(GetUserId(), GetUserId(), UserRole.Client);
            else if (userRoleClaim == UserRole.Employee.ToString())
                return await _service.GetCreditRating(GetUserId(), GetUserId(), UserRole.Employee);
            else
                throw new Exception("Token not found");

        }


        [HttpGet]
        [Route("GetMyCredits")]
        [Authorize]
        public async Task<List<CreditAccountDto>> GetMyCredits()
        {
            return await _service.GetUserCredits(GetUserId(), GetUserId());
        }

        [HttpGet]
        [Route("{UserId}/GetAllCredits")]
        [Authorize(Roles = "Employee")]
        public async Task<List<CreditAccountDto>> GetUserCredits(Guid UserId)
        {
            return await _service.GetUserCredits(UserId, GetUserId());
        }

        private Guid GetUserId()
        {
            var userEmailClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (userEmailClaim == null)
                throw new Exception("Token not found");

            return new Guid(userEmailClaim);
        }
    }
}
