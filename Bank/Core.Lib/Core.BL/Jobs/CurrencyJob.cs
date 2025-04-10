using Bank.DTO.DTOs.Other;
using Core.DAL;
using Core.DAL.Models.Currency;
using Core.DAL.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Text;
using System.Xml.Serialization;

namespace Core.BL.Jobs
{
    [DisallowConcurrentExecution]
    public class CurrencyJob : IJob
    {
        private readonly CoreDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CurrencyOptions _options;
        private readonly ILogger<CurrencyJob> _logger;

        public CurrencyJob(
            CoreDbContext context,
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<CurrencyOptions> options,
            ILogger<CurrencyJob> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var currenciesFromDb = await _context.Currencies.ToListAsync();

            Dictionary<int, CurrencyType> numberToCurrencyDict = currenciesFromDb.ToDictionary(x => x.Vcode);

            var stringContent = GetRequestStringContent();
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(_options.RequestUrl, stringContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error on response from bank, statusCode : {statusCode}, message : {message}", response.StatusCode, await response.Content.ReadAsStringAsync());
                return;
            }

            var responseStream = await response.Content.ReadAsStreamAsync();
            var currencyDtoFromResponse = (new XmlSerializer(typeof(CurrencyDtoFromBankRf)).Deserialize(responseStream) as CurrencyDtoFromBankRf);

            if (currencyDtoFromResponse == null)
            {
                _logger.LogError("Xml from response has incorrect type, type : {type}", await response.Content.ReadAsStringAsync());
                return;
            }

            var currenciesListFromResponse = currencyDtoFromResponse
                                                .Body?
                                                .GetCursOnDateXMLResponse?
                                                .GetCursOnDateXMLResult?
                                                .ValuteData?
                                                .ValuteCursOnDate ?? [];

            DateTime now = DateTime.UtcNow;
            foreach (var currency in currenciesListFromResponse)
            {
                currency.Vname = currency.Vname.Trim();
                if (!numberToCurrencyDict.TryGetValue(currency.Vcode, out CurrencyType? currencyFromDb))
                {
                    var newCurrencyType = new CurrencyType
                    {
                        Id = Guid.NewGuid(),

                        Vname = currency.Vname,
                        Vnom = currency.Vnom,
                        Vcurs = currency.Vcurs,
                        Vcode = currency.Vcode,
                        VchCode = currency.VchCode,
                        VunitRate = currency.VunitRate,

                        CreateDateTime = now
                    };

                    await _context.Currencies.AddAsync(newCurrencyType);
                    continue;
                }
                else
                {
                    currencyFromDb.Vcurs = currency.Vcurs;
                    currencyFromDb.VunitRate = currency.VunitRate;
                    currencyFromDb.ModifyDateTime = now;
                    _context.Entry(currencyFromDb).State = EntityState.Modified;
                }
            }

            _logger.LogInformation("Number of elements in list of currencies : {number}", currenciesListFromResponse.Count);
            await _context.SaveChangesAsync();
        }

        private static StringContent GetRequestStringContent()
        {
            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            string requestBody = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
  <soap12:Body>
    <GetCursOnDateXML xmlns=""http://web.cbr.ru/"">
      <On_date>{currentTime}</On_date>
    </GetCursOnDateXML>
  </soap12:Body>
</soap12:Envelope>";

            return new StringContent(requestBody, Encoding.UTF8, "application/soap+xml");
        }
    }
}
