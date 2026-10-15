using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LolAccountSwitcher
{
    public class SummonerStatsResult
    {
        public string GameName { get; set; } = "";
        public string TagLine { get; set; } = "";
        public int Level { get; set; }
        public string Rank { get; set; } = "Unranked";
        public string Tier { get; set; } = "UNRANKED";
        public string? ProfileImageUrl { get; set; }
        public string? TierImageUrl { get; set; }
        public string ProfileUrl { get; set; } = "";
        public string LiveUrl { get; set; } = "";
    }

    public static class LolStatsService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        static LolStatsService()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            HttpClient.Timeout = TimeSpan.FromSeconds(8);
        }

        public static async Task<SummonerStatsResult?> FetchStatsAsync(string summonerInput, string server)
        {
            if (string.IsNullOrWhiteSpace(summonerInput))
                return null;

            string region = NormalizeServer(server);
            string riotId = summonerInput.Trim();

            // Eğer # yoksa ama - varsa # formatına çevirelim
            if (!riotId.Contains('#') && riotId.Contains('-'))
            {
                int dashIdx = riotId.LastIndexOf('-');
                riotId = riotId.Substring(0, dashIdx) + "#" + riotId.Substring(dashIdx + 1);
            }

            string requestUrl = $"https://lol-api-summoner.op.gg/api/v3/{region}/summoners?riot_id={Uri.EscapeDataString(riotId)}";

            try
            {
                using var response = await HttpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("data", out var dataArray) || dataArray.GetArrayLength() == 0)
                    return null;

                var first = dataArray[0];

                string gameName = first.TryGetProperty("game_name", out var gn) ? (gn.GetString() ?? "") : "";
                string tagLine = first.TryGetProperty("tagline", out var tl) ? (tl.GetString() ?? "") : "";
                int level = first.TryGetProperty("level", out var lv) && lv.TryGetInt32(out int parsedLv) ? parsedLv : 0;
                string? profileImageUrl = first.TryGetProperty("profile_image_url", out var pi) ? pi.GetString() : null;

                string rank = "Unranked";
                string tier = "UNRANKED";
                string? tierImageUrl = null;

                if (first.TryGetProperty("solo_tier_info", out var soloTier) && soloTier.ValueKind == JsonValueKind.Object)
                {
                    tier = soloTier.TryGetProperty("tier", out var tProp) ? (tProp.GetString() ?? "UNRANKED") : "UNRANKED";
                    int division = soloTier.TryGetProperty("division", out var dProp) && dProp.TryGetInt32(out int div) ? div : 0;
                    int lp = soloTier.TryGetProperty("lp", out var lpProp) && lpProp.TryGetInt32(out int parsedLp) ? parsedLp : 0;
                    tierImageUrl = soloTier.TryGetProperty("tier_image_url", out var tiProp) ? tiProp.GetString() : null;

                    rank = FormatRank(tier, division, lp);
                }

                string encodedName = Uri.EscapeDataString(gameName);
                string encodedTag = Uri.EscapeDataString(tagLine);

                // League of Graphs ve Porofessor profil bağlantıları
                string logUrl = $"https://www.leagueofgraphs.com/tr/summoner/{region}/{encodedName}-{encodedTag}";
                string poroUrl = $"https://porofessor.gg/tr/live/{region}/{encodedName}-{encodedTag}";

                return new SummonerStatsResult
                {
                    GameName = gameName,
                    TagLine = tagLine,
                    Level = level,
                    Rank = rank,
                    Tier = tier,
                    ProfileImageUrl = profileImageUrl,
                    TierImageUrl = tierImageUrl,
                    ProfileUrl = logUrl,
                    LiveUrl = poroUrl
                };
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeServer(string server)
        {
            return server.Trim().ToUpperInvariant() switch
            {
                "TR" => "tr",
                "EUW" => "euw",
                "EUNE" => "eune",
                "NA" => "na",
                "KR" => "kr",
                "BR" => "br",
                "LAN" => "lan",
                "LAS" => "las",
                "OCE" => "oce",
                "RU" => "ru",
                "JP" => "jp",
                _ => "euw"
            };
        }

        private static string FormatRank(string tier, int division, int lp)
        {
            if (string.IsNullOrWhiteSpace(tier) || tier.Equals("UNRANKED", StringComparison.OrdinalIgnoreCase))
                return "Unranked";

            // Baş harfi büyük yap: GOLD -> Gold, DIAMOND -> Diamond
            string tierFormatted = char.ToUpperInvariant(tier[0]) + tier.Substring(1).ToLowerInvariant();

            // Apex ranklar (Master, Grandmaster, Challenger) division içermez
            if (tier.Equals("MASTER", StringComparison.OrdinalIgnoreCase) ||
                tier.Equals("GRANDMASTER", StringComparison.OrdinalIgnoreCase) ||
                tier.Equals("CHALLENGER", StringComparison.OrdinalIgnoreCase))
            {
                return $"{tierFormatted} • {lp} LP";
            }

            string roman = division switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                _ => ""
            };

            return string.IsNullOrEmpty(roman)
                ? $"{tierFormatted} • {lp} LP"
                : $"{tierFormatted} {roman} • {lp} LP";
        }
    }
}
