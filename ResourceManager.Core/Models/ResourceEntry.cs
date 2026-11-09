using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ResourceManager.Core.Models
{
    public class ResourceEntry
    {
        public string Name { get; set; } = string.Empty;

        public string? Value { get; set; }

        public string? Comment { get; set; }

        public string? EnglishValue { get; set; }

        public bool EnglishKeyExists { get; set; }

    }
}
