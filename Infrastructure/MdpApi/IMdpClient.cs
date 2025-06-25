using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.MdpApi;

public interface IMdpClient
{
    Task<ExpandoObject> GetData(IEnumerable<Uri> uris, (string AccessToken, string Env, string Bgroup) auth);
}