using System.Globalization;

namespace ResourceManager.Core.Services
{
    public class LanguageService
    {
        public static List<CultureInfo> GetAvailableLanguages()
        {
            return CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Where(culture => !string.IsNullOrWhiteSpace(culture.Name))
                .DistinctBy(culture => culture.Name)
                .OrderBy(culture => culture.EnglishName)
                .ToList();
        }
    }
}
