using ResourceManager.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace ResourceManager.Core.Services
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

        public List<ResourceEntry> FindMissingTranslations(string basePath, string targethPath)
        {
            var polishResources = ReadResources(basePath);
            var englishResources = ReadResources(targethPath);

            foreach (var resource in polishResources)
            {
                var targetResource = englishResources
                    .FirstOrDefault(x => x.Name == resource.Name);

                resource.TargetKeyExists = targetResource != null;
                resource.TargetValue = targetResource?.Value;
            }

            return polishResources
                .Where(resource =>
                    !resource.TargetKeyExists ||
                    string.IsNullOrWhiteSpace(resource.TargetValue))
                .ToList();
        }

        public string[] GetResxFiles(string folderPath)
        {
            return Directory.GetFiles(
                folderPath,
                "*.resx",
                SearchOption.TopDirectoryOnly);
        }
    }
}
