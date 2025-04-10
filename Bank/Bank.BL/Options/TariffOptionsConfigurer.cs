using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bank.BL.Options
{
    public class TariffOptionsConfigurer : IConfigureOptions<TariffOptionsModel>
    {
        private const string section = "TariffOptions";

        private readonly IConfiguration _configuration;

        public TariffOptionsConfigurer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(TariffOptionsModel options)
        {
            _configuration.GetSection(section).Bind(options);
        }
    }
}
