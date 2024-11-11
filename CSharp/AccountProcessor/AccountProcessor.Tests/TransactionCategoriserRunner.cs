using NUnit.Framework;
using System.Text.Json;

namespace AccountProcessor.Tests
{
    public class TransactionCategoriserRunner
    {
        [Test]
        public void BootstrapJson()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while(dir.Name != "CSharp")
            {
                dir = dir.Parent!;
            }

            var outputPath = Path.Combine(dir.FullName, "AccountProcessor", "AccountProcessor", "Components", "Services", "MatchModel.json");
            var content = JsonSerializer.Serialize(new
                {
                    SomeData = new
                    {
                        Collection = new[] { "1", "2" }
                    }
                },
                new JsonSerializerOptions { IncludeFields = true, WriteIndented = true });
            File.WriteAllText(outputPath, content);
        }
    }
}
