using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.TokenService;

public record TokenServiceClientConfiguration(string GrantType, string ClientId, string ClientSecret, ICollection<string> Scopes);