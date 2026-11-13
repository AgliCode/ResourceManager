namespace ResourceManager.Core.Models
{
    public class NewLanguageEntry
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Comment { get; set; }
        public bool IsSelected { get; set; } = true;
        public string? TranslatedValue { get; set; }
    }
}
