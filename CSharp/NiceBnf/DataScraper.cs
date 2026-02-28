using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Runtime.CompilerServices;

namespace NiceBnf;

// ─── Data Models ─────────────────────────────────────────────────────────────

/// <summary>A dose entry for a specific patient group under a route of administration.</summary>
public record PatientGroupDose(
    /// <summary>e.g. "Adult", "Child 1–11 months", "Neonate"</summary>
    string PatientGroup,
    /// <summary>e.g. "500 mg 3 times a day for 5 days"</summary>
    string DoseStatement,
    /// <summary>"adult", "child", or "neonate"</summary>
    string PatientType
);

/// <summary>A route of administration (e.g. "By mouth") with its patient-group doses.</summary>
public record RouteOfAdministration(
    string Route,
    IReadOnlyList<PatientGroupDose> Doses
);

/// <summary>A clinical indication with all routes and doses beneath it.</summary>
public record Indication(
    /// <summary>e.g. "Susceptible infections (e.g. sinusitis, salmonellosis, oral infections)"</summary>
    string Text,
    IReadOnlyList<RouteOfAdministration> Routes
);

/// <summary>A BNF drug entry with its full name and all indications/doses.</summary>
public record Drug(
    /// <summary>e.g. "Amoxicillin"</summary>
    string Name,
    /// <summary>URL slug, e.g. "amoxicillin"</summary>
    string Slug,
    /// <summary>Full page URL</summary>
    string Url,
    IReadOnlyList<Indication> Indications
);

// ─── Scraper ─────────────────────────────────────────────────────────────────

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

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns every drug page slug listed on the BNF A–Z index
    /// (e.g. "/drugs/amoxicillin/").
    /// </summary>
    public async Task<IReadOnlyList<string>> GetDrugSlugsAsync(CancellationToken ct = default)
    {
        var html = await _httpClient.GetStringAsync($"{BaseUrl}/drugs/", ct);
        using var document = await _parser.ParseDocumentAsync(html, ct);

        // Drug links follow the pattern /drugs/<slug>/ with exactly 3 slashes.
        var slugs = document
            .QuerySelectorAll("a[href^='/drugs/']")
            .Select(a => a.GetAttribute("href")!)
            .Where(href => href != "/drugs/" && href.Count(c => c == '/') == 3)
            .Distinct()
            .ToList();

        return slugs;
    }

    /// <summary>
    /// Fetches and parses a single drug page, returning a structured <see cref="Drug"/>
    /// object. Returns <c>null</c> if the page cannot be fetched.
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

        // If the page has no "Indications and dose" section, return the drug with no doses.
        if (document.QuerySelector("h2[id='indications-and-dose']") is null)
            return new Drug(name, drugSlug, url, []);

        return new Drug(name, drugSlug, url, ParseIndications(document));
    }

    /// <summary>
    /// Lazily scrapes every drug from the BNF A–Z list, yielding each
    /// <see cref="Drug"/> as soon as it is parsed.
    /// </summary>
    public async IAsyncEnumerable<Drug> ScrapeAllDrugsAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
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
                                  $"({drug.Indications.Count} indication(s))");
                yield return drug;
            }

            // Avoid hammering the server between requests.
            if (i < slugs.Count - 1)
                await Task.Delay(RequestDelayMs, ct);
        }
    }

    // ── Parsing helpers ───────────────────────────────────────────────────────

    // CSS module class names use hashed suffixes (e.g. "indicationWrapper--edb2c").
    // Attribute-contains selectors [class*="…"] are used for resilience against hash changes.

    private static IReadOnlyList<Indication> ParseIndications(IDocument document)
    {
        var indications = new List<Indication>();

        foreach (var el in document.QuerySelectorAll("section[class*='indicationWrapper']"))
        {
            var text = el.QuerySelector("[class*='indicationText']")
                         ?.TextContent.Trim() ?? string.Empty;

            indications.Add(new Indication(text, ParseRoutes(el)));
        }

        return indications;
    }

    private static IReadOnlyList<RouteOfAdministration> ParseRoutes(IElement indicationEl)
    {
        var routes = new List<RouteOfAdministration>();

        foreach (var routeEl in indicationEl.QuerySelectorAll("section[class*='routeOfAdministration']"))
        {
            var routeName = routeEl
                .QuerySelector("[class*='routeOfAdministrationHeading']")
                ?.TextContent.Trim() ?? string.Empty;

            routes.Add(new RouteOfAdministration(routeName, ParseDoses(routeEl)));
        }

        return routes;
    }

    private static IReadOnlyList<PatientGroupDose> ParseDoses(IElement routeEl)
    {
        var doses = new List<PatientGroupDose>();

        // Each patient group is a <div class="…patientGroupDose… …adult/child/neonate…">
        //   <dt class="…patientGroup…">Adult</dt>
        //   <dd class="…doseStatement…">500 mg every 8 hours</dd>
        foreach (var div in routeEl.QuerySelectorAll("div[class*='patientGroupDose']"))
        {
            var group = div.QuerySelector("dt")?.TextContent.Trim() ?? string.Empty;
            var dose  = div.QuerySelector("dd")?.TextContent.Trim() ?? string.Empty;

            var classes = div.ClassName ?? string.Empty;
            var patientType = classes.Contains("adult")   ? "adult"
                            : classes.Contains("child")   ? "child"
                            : classes.Contains("neonate") ? "neonate"
                            : "unknown";

            doses.Add(new PatientGroupDose(group, dose, patientType));
        }

        return doses;
    }

    public void Dispose() => _httpClient.Dispose();
}
