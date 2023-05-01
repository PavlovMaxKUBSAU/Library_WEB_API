using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CourseLibrary.API.Auth
{
    public class Authentication : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public Authentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
            :base(options, logger, encoder, clock)
        {
            HandleAuthenticateAsync();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Miss Auth Header"));
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(' ');
                var login = credentials[0];
                var password = credentials[1];

                if (login == "l" && password == "p")
                {
                    Claim[] claims = { new Claim(ClaimTypes.NameIdentifier, login) };
                    var id = new ClaimsIdentity(claims, Scheme.Name);
                    var principial = new ClaimsPrincipal(id);
                    var ticket = new AuthenticationTicket(principial, Scheme.Name);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                
                return Task.FromResult(AuthenticateResult.Fail("Invalid Login or Pwd"));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Auth Header (caught)!"));
            }
        }
    }
}
