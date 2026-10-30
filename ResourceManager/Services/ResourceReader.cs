using ResourceManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace ResourceManager.Services
{
    public class ResourceReader
    {
        public List<ResourceEntry> ReadResources(string filePath)
        {
            return XDocument.Load(filePath)
            .Descendants()
            .Where(element => element.Name.LocalName == "data")
            .Where(element => element.Attribute("name") != null)
            .Select(element => new ResourceEntry
            {
                Name = element.Attribute("name")!.Value,

                Value = element.Elements()
                    .FirstOrDefault(child => child.Name.LocalName == "value")
                    ?.Value,

                Comment = element.Elements()
                    .FirstOrDefault(child => child.Name.LocalName == "comment")
                    ?.Value
            })
            .ToList();
        }

        public List<ResourceEntry> FindMissingTranslations(
            string polishPath,
            string englishPath)
        {
            var polishResources = ReadResources(polishPath);
            var englishResources = ReadResources(englishPath);

            foreach (var resource in polishResources)
            {
                var englishResource = englishResources
                    .FirstOrDefault(x => x.Name == resource.Name);

                resource.EnglishKeyExists = englishResource != null;
                resource.EnglishValue = englishResource?.Value;
            }

            return polishResources
                .Where(resource =>
                    !resource.EnglishKeyExists ||
                    string.IsNullOrWhiteSpace(resource.EnglishValue))
                .ToList();
        }
    }
}
