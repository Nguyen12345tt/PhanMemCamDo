using System.Text.RegularExpressions;

namespace PhanMemCamDo.Services
{
    public partial class NotificationRegex
    {
        [GeneratedRegex(@"HD\d+", RegexOptions.IgnoreCase)]
        private static partial Regex ContractCodeRegex();

        public static Match MatchTitle(string input) => ContractCodeRegex().Match(input);
    }
}
