using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ResourceManager.Services
{
    public class LanguageService
    {
        public List<CultureInfo> GetAvailableLanguages()
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
