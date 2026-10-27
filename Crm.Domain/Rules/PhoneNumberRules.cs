using System.Text.RegularExpressions;

namespace Crm.Domain.Rules
{
    public sealed record ParsedPhone(string Stored, string MatchKey, bool IsGeorgian);

    public static class PhoneNumberRules
    {
        private static readonly Regex ValidFormat = new(@"^\+?[0-9]+\z", RegexOptions.Compiled);
        private const string GeorgiaCode = "995";
        private const int GeorgiaNationalLength = 9;

        public static ParsedPhone? Parse(string input)
        {
            // Rule 1 + 6: remove spaces only; the result is what we store
            var stored = input.Replace(" ", "");
            if (!ValidFormat.IsMatch(stored))
                return null;

            // Rule 3: '+' or '00' marks an international number
            var afterPrefix = stored.StartsWith('+') ? stored[1..]
                            : stored.StartsWith("00") ? stored[2..]
                            : null;

            if (afterPrefix is not null)
            {
                // Rule 4: +995 / 00995 must be exactly 995 + 9 digits
                if (afterPrefix.StartsWith(GeorgiaCode))
                    return afterPrefix.Length == GeorgiaCode.Length + GeorgiaNationalLength
                        ? new(stored, afterPrefix, IsGeorgian: true)
                        : null;

                return afterPrefix.Length is >= 7 and <= 15
                    ? new(stored, afterPrefix, IsGeorgian: false)
                    : null;
            }

            if (afterPrefix is not null && afterPrefix.StartsWith('0'))
                return null;

            // Rule 2: Georgian shapes without a prefix
            var key = stored.Length switch
            {
                9 => GeorgiaCode + stored,
                10 when stored[0] == '0' => GeorgiaCode + stored[1..],
                12 when stored.StartsWith(GeorgiaCode) => stored,
                _ => null   // Rule 5: everything else
            };

            return key is null ? null : new(stored, key, IsGeorgian: true);
        }

        public static string? GetMatchKey(string input) => Parse(input)?.MatchKey;
    }
}