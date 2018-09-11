using System;
using System.Linq;
using System.Text.RegularExpressions;
using Lykke.Service.CexIoAdapter.Services.Settings;

namespace Lykke.Service.CexIoAdapter.Services.CexIo
{
    public static class CexIoInstrument
    {
        public static string FromPair(string symbol1, string symbol2)
        {
            AssertIsSymbol(symbol1);
            AssertIsSymbol(symbol2);
            return $"{symbol1}:{symbol2}";
        }

        public static (string, string) FromInstrument(string instrument)
        {
            var pair = instrument.Split(':', 2);

            if (pair.Length != 2)
                throw new ArgumentException(nameof(instrument), $"{instrument} is not valid cex.io instrument " +
                                                                $"(BTC:USD is correct one)");

            AssertIsSymbol(pair[0]);
            AssertIsSymbol(pair[1]);

            return (pair[0], pair[1]);
        }

        private static readonly Regex IsSymbol = new Regex(@"^[a-z]{3,4}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static void AssertIsSymbol(string symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));

            if (!IsSymbol.IsMatch(symbol))
            {
                throw new ArgumentException($"{symbol} doesn't look like a correct symbol");
            }
        }

        public static string ToLykkeInstrument(string instrument, CurrencyMapping mapping)
        {
            var (symbol1, symbol2) = FromInstrument(instrument);

            if (mapping.Rename.TryGetValue(symbol1, out var s1))
            {
                symbol1 = s1;
            }

            if (mapping.Rename.TryGetValue(symbol2, out var s2))
            {
                symbol2 = s2;
            }

            return symbol1 + symbol2;
        }

        public static string FromLykkeInstrument(string instrument, CurrencyMapping mapping)
        {
            if (instrument.Length == 6)
            {
                var l1 = instrument.Substring(0, 3);
                var l2 = instrument.Substring(3, 3);

                return FromLykkePair(l1, l2, mapping);
            }

            if (instrument.Length == 7)
            {
                var candidate = instrument.Substring(0, 4);

                if (mapping.FourCharsCurrencies.Contains(candidate))
                {
                    return FromLykkePair(candidate, instrument.Substring(4, 3), mapping);
                }
                else
                {
                    return FromLykkePair(instrument.Substring(0, 3), instrument.Substring(3, 4), mapping);
                }
            }

            if (instrument.Length == 8)
            {
                return FromLykkePair(instrument.Substring(0, 4), instrument.Substring(4, 4), mapping);
            }

            throw new ArgumentException("Instrument should be of length 6 or 7", nameof(instrument));
        }

        private static string FromLykkePair(string l1, string l2, CurrencyMapping mapping)
        {
            if (mapping.Inverted.TryGetValue(l1, out var s1))
            {
                l1 = s1;
            }

            if (mapping.Inverted.TryGetValue(l2, out var s2))
            {
                l2 = s2;
            }

            return FromPair(l1, l2);
        }
    }
}
