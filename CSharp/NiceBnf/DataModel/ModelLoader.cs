namespace DataModel;

public interface IModelLoader
{
    Task<IReadOnlyList<Drug>> LoadDrugsAsync();
}

public class ModelLoader : IModelLoader
{
    public async Task<IReadOnlyList<Drug>> LoadDrugsAsync()
    {
        var filePaths = new DirectoryInfo(FileLoader.GetDefinitionsPath()).GetFiles("*.json");
        var drugs = new List<Drug>();
        foreach (var filePath in filePaths)
        {
            drugs.Add(await JsonHelper.DeserializeAsync<Drug>(File.OpenRead(filePath.FullName)));
        }
        return drugs
            .OrderBy(d => d.Name)
            .ToList();
    }
}