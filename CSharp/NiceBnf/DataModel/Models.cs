namespace DataModel;

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