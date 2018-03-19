using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public sealed class CurrencyMapping
    {
        public IReadOnlyDictionary<string, string> Rename { get; set; }

        public IReadOnlyDictionary<string, string> Inverted => Rename.ToDictionary(x => x.Value, x => x.Key);

        public IReadOnlyCollection<string> FourCharsCurrencies { get; set; } = new [] { "DASH" };
    }
}
