namespace ResourceManager.Core.Models
{
    public class ResourceEntry
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Comment { get; set; }
        public string? TargetValue { get; set; }
        public bool TargetKeyExists { get; set; }

    }
}
