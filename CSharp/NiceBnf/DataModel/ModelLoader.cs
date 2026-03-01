namespace DataModel;

public static class ModelLoader
{
    public static async Task<IReadOnlyList<Drug>> LoadDrugsAsync()
    {
        var filePaths = new DirectoryInfo(FileLoader.GetDefinitionsPath()).GetFiles("*.json");
        var drugs = new List<Drug>();
        foreach (var filePath in filePaths)
        {
            drugs.Add(await JsonHelper.DeserializeAsync<Drug>(File.OpenRead(filePath.FullName)));
        }
        return drugs;
    }
}