using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IndentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("bankapi", "Bank API") 
            };

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("bankapi", "Bank API")
            };
        }


        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // machine to machine client (from quickstart 1)
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "api1" }
                },
                // interactive ASP.NET Core Web App
                new Client
                {
                    ClientId = "web",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
            
                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "bankapi"
                    }
                },
                new Client
                {
                    ClientId = "ebankclient",
                    ClientName = "E-bank client",
                    ClientUri = "http://158.160.18.15",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireClientSecret = false,
                    RedirectUris = {
                        "http://158.160.18.15:5174/signin-oidc",
                        "http://158.160.18.15:5175/signin-oidc" 
                    },
                    PostLogoutRedirectUris = {
                        "http://158.160.18.15:5175/signout-oidc",
                        "http://158.160.18.15:5174/signout-oidc"
                    },
                    AllowedCorsOrigins = {
                        "http://158.160.18.15:5175",
                        "http://158.160.18.15:5174"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "bankapi"
                    },
                    AllowAccessTokensViaBrowser = true,
                    AllowOfflineAccess = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AbsoluteRefreshTokenLifetime = 86400
                }
            };
    }
}