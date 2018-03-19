using System.Runtime.Serialization;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public enum OrderType
    {
        [EnumMember(Value = "buy")]
        Buy,
        [EnumMember(Value = "sell")]
        Sell
    }
}