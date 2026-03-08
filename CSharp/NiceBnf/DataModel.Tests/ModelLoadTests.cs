namespace DataModel.Tests;

public class ModelLoadTests
{
    [Test]
    public async Task Check_load()
    {
        var all = await new ModelLoader().LoadDrugsAsync();
        var found = all.Single(x => x.Name == "Abacavir with lamivudine");
        Assert.That(found.Indications.Count, Is.EqualTo(1));
    }
}