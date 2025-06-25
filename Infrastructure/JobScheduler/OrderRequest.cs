using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.JobScheduler;

public class OrderRequest
{
    public OrderRequest()
    {
        Orders = new List<Order>();
    }
    
    [JsonPropertyName("jobschedulerId")]
    public string JobSchedulerId { get; set; }
    
    [JsonPropertyName("jobChain")]
    public string JobChain { get; set; }
    
    [JsonPropertyName("compact")]
    public bool Compact { get; set; }

    [JsonPropertyName("orders")]
    public List<Order> Orders { get; set; }

    public static OrderRequest CreateOrderRequest(string referenceNumber, string businessGroup, string calculationType, string jobChainEnv, int seqNo)
    {
        return new OrderRequest
        {
            JobSchedulerId = "rkedev",
            Orders = new List<Order>
            {
                new Order
                {
                    JobChain = $"/{jobChainEnv}/ifa_outbound/IFAOutbound",
                    OrderId = $"MDPOrder{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}",
                    At = "now",
                    Params = new List<Param>
                    {
                        new Param("BGROUP", businessGroup),
                        new Param("REFNO", referenceNumber),
                        new Param("CSEQNO", seqNo.ToString()),
                        new Param("CALCTYPE", calculationType),
                        new Param("IFANAME", "LV"),
                    }
                }
            }
        };
    }

    public static OrderRequest OrderStatusRequest(string orderId, string jobChainEnv)
    {
        return new OrderRequest
        {
            JobSchedulerId = "rkedev",
            JobChain = $"/{jobChainEnv}/ifa_outbound/IFAOutbound",
            Compact = false,
            Orders = new List<Order> { 
                new Order
                {
                    OrderId = orderId,
                    JobChain = $"/{jobChainEnv}/ifa_outbound/IFAOutbound"
                }
            }
        };
    }
}

public record Order
{
    [JsonPropertyName("jobChain")]
    public string JobChain { get; set; }

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("audit_log")]
    public AuditLog AuditLog { get; set; }

    [JsonPropertyName("at")]
    public string At { get; set; }

    [JsonPropertyName("params")]
    public List<Param> Params { get; set; }
}

public record AuditLog(
    [property: JsonPropertyName("comment")] string Comment
);

public record Param(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value
);

