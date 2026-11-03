using ResourceManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ResourceManager.Services
{
    internal class ResxWriter
    {
        public void SaveTranslationToResx(Dictionary<string, string> translations, string? englishFilePath)
        {
            if (string.IsNullOrEmpty(englishFilePath))
                throw new InvalidOperationException("No path to the RESX file was provided.");

            var document = System.Xml.Linq.XDocument.Load(englishFilePath);

            foreach (var translation in translations)
            {
                var dataElement = document.Root?
                    .Elements("data")
                    .FirstOrDefault(element =>
                        (string?)element.Attribute("name") == translation.Key);

                if (dataElement != null) dataElement.Element("value")!.Value = translation.Value;
                else
                {
                    // Add translation
                    document.Root?.Add(
                    new System.Xml.Linq.XElement("data",
                        new System.Xml.Linq.XAttribute("name", translation.Key),
                        new System.Xml.Linq.XAttribute(System.Xml.Linq.XNamespace.Xml + "space", "preserve"),
                        new System.Xml.Linq.XElement("value", translation.Value)
                        )
                    );
                }

            }


            document.Save(englishFilePath);
        }
        public void CreateTranslatedResx(string sourceFilePath,string outputFilePath,IEnumerable<NewLanguageEntry> entries)
        {
            var sourceDocument = System.Xml.Linq.XDocument.Load(sourceFilePath);

            var sourceRoot = sourceDocument.Root
                ?? throw new InvalidOperationException("Invalid RESX file.");

            var newRoot = new System.Xml.Linq.XElement(
                sourceRoot.Name,
                sourceRoot.Attributes(),
                sourceRoot.Elements()
                    .Where(element => element.Name.LocalName != "data")
                    .Select(element => new System.Xml.Linq.XElement(element))
            );

            foreach (var entry in entries)
            {
  

                var originalData = sourceRoot.Elements()
                    .FirstOrDefault(element =>
                        element.Name.LocalName == "data" &&
                        (string?)element.Attribute("name") == entry.Name);

                if (originalData == null)
                    throw new InvalidOperationException(
                        $"Resource '{entry.Name}' was not found.");

                var translatedData = new System.Xml.Linq.XElement(originalData);
                translatedData.Element("value")!.Value = entry.TranslatedValue ?? string.Empty;

                newRoot.Add(translatedData);
            }

            new System.Xml.Linq.XDocument(
                sourceDocument.Declaration,
                newRoot
            ).Save(outputFilePath);
        }
    }
}
