using System.Collections.Immutable;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NiceBnf;

// ─── Data Models ─────────────────────────────────────────────────────────────

/// <summary>A dose entry for a specific patient group under a route of administration.</summary>
/// <param name="PatientGroup">e.g. "Adult", "Child 1–11 months", "Neonate"</param>
/// <param name="DoseStatement">e.g. "500 mg 3 times a day for 5 days"</param>
/// <param name="PatientType">"adult", "child", or "neonate"</param>
public record PatientGroupDose(
    string PatientGroup,
    string DoseStatement,
    string PatientType
);

/// <summary>A route of administration (e.g. "By mouth") with its patient-group doses.</summary>
public record RouteOfAdministration(
    string Route,
    IReadOnlyList<PatientGroupDose> Doses
);

/// <summary>A clinical indication with all routes and doses beneath it.</summary>
/// <param name="Text">e.g. "Susceptible infections (e.g. sinusitis, salmonellosis, oral infections)"</param>
public record Indication(
    string Text,
    IReadOnlyList<RouteOfAdministration> Routes
);

/// <summary>A BNF cautionary and advisory label.</summary>
/// <param name="Number">e.g. 9</param>
/// <param name="Description">e.g. "Space the doses evenly throughout the day…"</param>
public record CautionaryLabel(
    int Number,
    string Description);

/// <summary>A single pack size/price entry for a preparation.</summary>
/// <param name="Size">e.g. "21"</param>
/// <param name="Unit">e.g. "capsule"</param>
/// <param name="NhsIndicativePrice">e.g. "£0.94"</param>
/// <param name="DrugTariff">e.g. "Part VIIIA Category M"</param>
/// <param name="DrugTariffPrice">e.g. "£0.94"</param>
/// <param name="LegalCategory">e.g. "POM (Prescription-only medicine)"</param>
public record MedicinalFormPack(
    string? Size,
    string? Unit,
    string? NhsIndicativePrice,
    string? DrugTariff,
    string? DrugTariffPrice,
    string? LegalCategory
);

/// <summary>A specific brand/manufacturer preparation within a medicinal form.</summary>
/// <param name="Name">e.g. "Amoxicillin 250mg capsules"</param>
/// <param name="Manufacturer">e.g. "A A H Pharmaceuticals Ltd"</param>
/// <param name="ActiveIngredients">e.g. "Amoxicillin (as Amoxicillin trihydrate) 250 mg"</param>
/// <param name="CautionaryLabels">BNF cautionary labels, e.g. label 9 with its description</param>
public record Preparation(
    string Name,
    string Manufacturer,
    bool SugarFree,
    string ActiveIngredients,
    IReadOnlyList<CautionaryLabel> CautionaryLabels,
    IReadOnlyList<MedicinalFormPack> Packs
);

/// <summary>A physical dosage form (e.g. "Oral capsule") with all its preparations.</summary>
/// <param name="FormType">e.g. "Oral capsule", "Oral suspension"</param>
/// <param name="Excipients">e.g. "May contain sucrose." – null if not stated</param>
/// <param name="Electrolytes">e.g. "May contain sodium." – null if not stated</param>
/// <param name="CautionaryLabels">BNF cautionary labels that apply to all preparations in this form</param>
public record MedicinalForm(
    string FormType,
    string? Excipients,
    string? Electrolytes,
    IReadOnlyList<CautionaryLabel> CautionaryLabels,
    IReadOnlyList<Preparation> Preparations
);

/// <summary>A BNF drug entry with its full name, all indications/doses, and medicinal forms.</summary>
/// <param name="Name">e.g. "Amoxicillin"</param>
/// <param name="Slug">URL slug, e.g. "amoxicillin"</param>
/// <param name="Url">Full page URL</param>
public record Drug(
    string Name,
    string Slug,
    string Url,
    IReadOnlyList<Indication> Indications,
    IReadOnlyList<MedicinalForm> MedicinalForms
);

/// <summary>
/// Scrapes drugs and their recommended doses from https://bnf.nice.org.uk/drugs/
/// using AngleSharp for HTML parsing.
/// </summary>
public sealed class DataScraper : IDisposable
{
    private const string BaseUrl = "https://bnf.nice.org.uk";
    private const int RequestDelayMs = 500; // polite crawl delay

    private readonly HttpClient _httpClient;
    private readonly HtmlParser _parser;

    public DataScraper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _parser = new HtmlParser();
    }

    public void Dispose() => _httpClient.Dispose();

    /// <summary>
    /// Returns every drug page slug listed on the BNF A–Z index
    /// (e.g. "/drugs/amoxicillin/").
    /// </summary>
    public async Task<IReadOnlyList<string>> GetDrugSlugsAsync(CancellationToken ct = default)
    {
        var html = await _httpClient.GetStringAsync($"{BaseUrl}/drugs/", ct);
        using var document = await _parser.ParseDocumentAsync(html, ct);

        // Drug links follow the pattern /drugs/<slug>/ with exactly 3 slashes.
        var hrefs = document
            .QuerySelectorAll("a[href^='/drugs/']")
            .Select(a => a.GetAttribute("href")!)
            .ToList();
        var slugs = hrefs
            .Where(href => href != "/drugs/" && href.StartsWith("/drugs/"))
            .Distinct()
            .ToList();
        return slugs;
    }

    /// <summary>
    /// Fetches and parses a single drug page, returning a structured <see cref="Drug"/>
    /// object that includes indications and medicinal forms. Returns <c>null</c> if the
    /// page cannot be fetched.
    /// </summary>
    public async Task<Drug?> ScrapeDrugAsync(string slug, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}{slug}";
        string html;
        try
        {
            html = await _httpClient.GetStringAsync(url, ct);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Failed to fetch {url}: {ex.Message}");
            return null;
        }

        using var document = await _parser.ParseDocumentAsync(html, ct);

        // Drug name is inside <h1 class="page-header__heading"><span>Name</span></h1>
        var name = document.QuerySelector("h1 span")?.TextContent.Trim()
                   ?? slug.Trim('/').Split('/').Last().Replace('-', ' ');

        var drugSlug = slug.Trim('/').Split('/').Last();

        // Polite delay before the second request for this drug.
        await Task.Delay(RequestDelayMs, ct);
        var medicinalForms = await ScrapeMedicinalFormsAsync(slug, ct);

        return new Drug(
            Name: name,
            Slug: drugSlug,
            Url: url,
            Indications: document.QuerySelector("h2[id='indications-and-dose']") != null
                ? _ParseIndications(document)
                : [],
            MedicinalForms: medicinalForms);
    }

    /// <summary>
    /// Fetches and parses the medicinal-forms page for a drug slug.
    /// Returns an empty list if the page cannot be fetched.
    /// </summary>
    public async Task<IReadOnlyList<MedicinalForm>> ScrapeMedicinalFormsAsync(string slug, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}{slug}medicinal-forms/";
        string html;
        try
        {
            html = await _httpClient.GetStringAsync(url, ct);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Failed to fetch {url}: {ex.Message}");
            return [];
        }

        using var document = await _parser.ParseDocumentAsync(html, ct);
        return _ParseMedicinalForms(document);
    }

    /// <summary>
    /// Lazily scrapes every drug from the BNF A–Z list, yielding each
    /// <see cref="Drug"/> as soon as it is parsed.
    /// </summary>
    public async IAsyncEnumerable<Drug> ScrapeAllDrugsAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        var slugs = await GetDrugSlugsAsync(ct);
        Console.WriteLine($"Found {slugs.Count} drugs. Starting scrape…");

        for (int i = 0; i < slugs.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var drug = await ScrapeDrugAsync(slugs[i], ct);
            if (drug is not null)
            {
                Console.WriteLine($"[{i + 1}/{slugs.Count}] {drug.Name} " +
                                  $"({drug.Indications.Count} indication(s), " +
                                  $"{drug.MedicinalForms.Count} form(s))");
                yield return drug;
            }

            // Avoid hammering the server between drugs.
            if (i < slugs.Count - 1)
            {
                await Task.Delay(RequestDelayMs, ct);
            }
        }
    }

    // CSS module class names use hashed suffixes (e.g. "indicationWrapper--edb2c").
    // Attribute-contains selectors [class*="…"] are used for resilience against hash changes.

    private static IReadOnlyList<Indication> _ParseIndications(IDocument document) =>
        document
            .QuerySelectorAll("section[class*='indicationWrapper']")
            .Select(el => new Indication(
                Text: el.QuerySelectorGetContent("[class*='indicationText']"),
                Routes: _ParseRoutes(el)))
            .ToList();

    private static IReadOnlyList<RouteOfAdministration> _ParseRoutes(IElement indicationEl) =>
        indicationEl
            .QuerySelectorAll("section[class*='routeOfAdministration']")
            .Select(routeEl => new RouteOfAdministration(
                Route: routeEl.QuerySelectorGetContent("[class*='routeOfAdministrationHeading']"),
                Doses: _ParseDoses(routeEl)))
            .ToList();

    // Each patient group is a <div class="…patientGroupDose… …adult/child/neonate…">
    //   <dt class="…patientGroup…">Adult</dt>
    //   <dd class="…doseStatement…">500 mg every 8 hours</dd>
    private static IReadOnlyList<PatientGroupDose> _ParseDoses(IElement routeEl) =>
        routeEl
            .QuerySelectorAll("div[class*='patientGroupDose']")
            .Select(div => new PatientGroupDose(
                PatientGroup: div.QuerySelectorGetContent("dt"),
                DoseStatement: div.QuerySelectorGetContent("dd"),
                PatientType: _ParsePatientType(className: div.ClassName)))
            .ToList();

    private static string _ParsePatientType(string? className) =>
        className != null
            ? className.Contains("adult")
                ? "Adult"
                : className.Contains("child") ? "Child"
                    : className.Contains("neonate") ? "Neonate"
                        : "Unknown"
            : "Unknown";

    /* Page structure:
    <section class="medicinal-forms-module--form--*">
      <h2 id="oral-capsule">Oral capsule</h2>
      <details>   ← form-level cautionary labels (h3[class*='--labelAccordionHeading--'])
      <h3>Excipients</h3><p>May contain sucrose.</p>     ← optional, no class
      <h3>Electrolytes</h3><p>May contain sodium.</p>    ← optional, no class
      <ol class="medicinal-forms-module--prepList--*">
        <li>
          <details>
            <summary>
              <h3 class="Prep-module--prepHeading--*">
                <span class="Prep-module--sugarFree--*">Sugar free</span>   ← optional
                <span class="Prep-module--headingText--*">
                  Amoxicillin 250mg capsules
                  <span class="Prep-module--manufacturer--*">A A H Pharmaceuticals Ltd</span>
                </span>
              </h3>
            </summary>
            <details>  ← prep-level cautionary labels (h4[class*='nestedLabelAccordionHeading'])
            <dl>       ← active ingredients
              <div class="Prep-module--packDefinitionListItem--*">
                <dt>Active ingredients</dt><dd>Amoxicillin ... 250 mg</dd>
              </div>
            </dl>
            <ol class="Prep-module--packList--*">
              <li class="Prep-module--packItem--*">
                <dl>Size / Unit / NHS indicative price / Drug tariff / Legal category</dl>
              </li>
            </ol>
          </details>
        </li>
      </ol>
    </section>
    */
    private static IReadOnlyList<MedicinalForm> _ParseMedicinalForms(IDocument document) =>
        document
            .QuerySelectorAll("section[class*='medicinal-forms-module--form']")
            .Select(_ParseMedicinalForm)
            .ToList();

    private static MedicinalForm _ParseMedicinalForm(IElement section)
    {
        var formType = section.QuerySelector("h2")?.TextContent.Trim() ?? string.Empty;

        // Excipients and electrolytes are plain <h3> elements (no class) that are
        // direct children of the section, followed by a <p> with the detail.
        string? excipients = null;
        string? electrolytes = null;
        foreach (var child in section.Children)
        {
            if (child.TagName != "H3" || child.ClassName?.Length > 0)
            {
                continue;
            }
            var text = child.TextContent.Trim();
            if (text == "Excipients")
            {
                excipients = excipients == null
                    ? child.NextElementSibling?.TextContent.Trim()
                    : throw new Exception("Excipients already set");
            }
            else if (text == "Electrolytes")
            {
                electrolytes = electrolytes == null
                    ? child.NextElementSibling?.TextContent.Trim()
                    : throw new Exception("Electrolytes already set");
            }
        }

        // Form-level cautionary labels live in the top-level <details> (<h3> not <h4>).
        var formLabelDetails = section
            .QuerySelector("h3[class*='medicinal-forms-module--labelAccordionHeading']")
            ?.Closest("details");
        var formLabels = _ParseCautionaryLabels(formLabelDetails);

        var preparations = section
            .QuerySelectorAll("ol[class*='medicinal-forms-module--prepList'] > li")
            .Select(_ParsePreparation)
            .ToList();

        return new MedicinalForm(
            FormType: formType,
            Excipients: excipients,
            Electrolytes: electrolytes,
            CautionaryLabels: formLabels,
            Preparations: preparations);
    }

    private static Preparation _ParsePreparation(IElement li)
    {
        var prepDetails = li.QuerySelector("details");

        // Name and manufacturer from the <summary> heading.
        var summary = prepDetails?.QuerySelector("summary");
        var manufacturer = summary?.QuerySelectorGetContent("[class*='Prep-module--manufacturer']") ?? "";
        var fullHeading = summary?.QuerySelectorGetContent("[class*='Prep-module--headingText']") ?? "";
        // Strip the manufacturer from the end of the heading text to get the bare name.
        var name = manufacturer.Length > 0 && fullHeading.EndsWith(manufacturer)
            ? fullHeading[..^manufacturer.Length].Trim()
            : fullHeading;

        var sugarFree = summary?.QuerySelector("[class*='Prep-module--sugarFree']") != null;

        // Prep-level cautionary labels live in a nested <details> (<h4> not <h3>).
        var prepLabelDetails = prepDetails
            ?.QuerySelector("h4[class*='medicinal-forms-module--nestedLabelAccordionHeading']")
            ?.Closest("details");
        var cautionaryLabels = _ParseCautionaryLabels(prepLabelDetails);

        // Active ingredients: find a <dt> with that exact text that is NOT inside a pack item.
        var activeDt = prepDetails
            ?.QuerySelectorAll("dt")
            .FirstOrDefault(dt =>
                dt.TextContent.Trim() == "Active ingredients" &&
                dt.Closest("li[class*='Prep-module--packItem']") == null);
        var activeIngredients = activeDt?.NextElementSibling?.TextContent.Trim() ?? "";

        var packs = _ParsePacks(prepDetails?.QuerySelector("ol[class*='Prep-module--packList']"));

        return new Preparation(
            Name: name,
            Manufacturer: manufacturer,
            SugarFree: sugarFree,
            ActiveIngredients: activeIngredients,
            CautionaryLabels: cautionaryLabels,
            Packs: packs);
    }

    private static IReadOnlyList<MedicinalFormPack> _ParsePacks(IElement? packList) =>
        packList?
            .QuerySelectorAll("li[class*='Prep-module--packItem']")
            .Select(ParsePack)
            .ToList()
        ?? [];

    private static MedicinalFormPack ParsePack(IElement li)
    {
        // Build a dt→dd dictionary from each definition list item in this pack.
        var fields = li
            .QuerySelectorAll("div[class*='Prep-module--packDefinitionListItem']")
            .ToImmutableDictionary(
                div => div.QuerySelectorGetContent("dt"),
                div => div.QuerySelectorGetContent("dd"));

        return new MedicinalFormPack(
            Size: fields.GetValueOrDefault("Size"),
            Unit: fields.GetValueOrDefault("Unit"),
            NhsIndicativePrice: fields.GetValueOrDefault("NHS indicative price"),
            DrugTariff: fields.GetValueOrDefault("Drug tariff"),
            DrugTariffPrice: fields.GetValueOrDefault("Drug tariff price"),
            LegalCategory: fields.GetValueOrDefault("Legal category"));
    }

    /// <summary>
    /// Extracts BNF cautionary labels from a &lt;details&gt; accordion element.
    /// Each label item is a &lt;li&gt; containing an &lt;h4 class="…labelHeading…"&gt;Label 9&lt;/h4&gt;
    /// followed by a &lt;p&gt; with the English description.
    /// </summary>
    private static IReadOnlyList<CautionaryLabel> _ParseCautionaryLabels(IElement? detailsEl)
    {
        if (detailsEl == null)
        {
            return [];
        }
        var labels = new List<CautionaryLabel>();
        foreach (var heading in detailsEl.QuerySelectorAll("[class*='medicinal-forms-module--labelHeading']"))
        {
            var match = Regex.Match(heading.TextContent, @"\d+");
            if (!match.Success)
            {
                continue;
            }
            var number = int.Parse(match.Value);
            var description = heading.NextElementSibling?.TextContent.Trim() ?? "";
            labels.Add(new CautionaryLabel(number, description));
        }
        return labels;
    }
}

public static class AgileSharpExtensions
{
    public static string QuerySelectorGetContent(this IElement el, string selector) =>
        el.QuerySelector(selector)?.TextContent.Trim() ?? string.Empty;
}