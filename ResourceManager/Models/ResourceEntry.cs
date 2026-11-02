using ResourceManager.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;


namespace ResourceManager.Models
{
    public class ResourceEntry
    {
        public string Name { get; set; } = string.Empty;

        public string? Value { get; set; }

        public string? Comment { get; set; }

        public string? EnglishValue { get; set; }

        public bool EnglishKeyExists { get; set; }

        public TranslationStatus Status { get; set; } = TranslationStatus.NotStarted;

        public Brush StatusEmoji => Status switch
        {
            TranslationStatus.NotStarted => Brushes.White,
            TranslationStatus.Current => Brushes.Orange,
            TranslationStatus.Approved => Brushes.Green,
            _ => Brushes.White
        };
    }
}
