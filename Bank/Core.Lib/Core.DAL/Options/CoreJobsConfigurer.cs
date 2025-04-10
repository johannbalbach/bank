using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Core.DAL.Options
{
    public class CoreJobsConfigurer : IConfigureOptions<CoreJobsConfig>
    {
        private readonly IConfiguration _configuration;
        private const string section = "CoreJobs";

        public CoreJobsConfigurer(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        public void Configure(CoreJobsConfig options)
        {
            _configuration.GetSection(section).Bind(options);
        }
    }
}
