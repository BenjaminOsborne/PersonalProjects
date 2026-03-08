using System.ComponentModel.DataAnnotations;
using DataModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace NiceBnf.Web.Components;

public partial class DrugDetail
{
    [Required] [Parameter] public Drug Drug { get; set; } = null!; //Always set on construction

    private MudDataGrid<Indication>? _indicationsGrid;
    private MudDataGrid<MedicinalForm>? _medicinalFormsGrid;

    private static string BuildMedicineHeader(MedicinalForm form)
    {
        var labelsSuffix = form.CautionaryLabels.Any()
            ? string.Join(", ", form.CautionaryLabels.Select(l => l.Number))
            : null;
        return labelsSuffix != null
            ? $"{form.FormType} (Label {labelsSuffix})"
            : form.FormType;
    }

    private Task _OnIndicationRowClicked(DataGridRowClickEventArgs<Indication> args) =>
        _indicationsGrid!.ToggleHierarchyVisibilityAsync(args.Item);

    private Task _OnMedicineClicked(DataGridRowClickEventArgs<MedicinalForm> args) =>
        _medicinalFormsGrid!.ToggleHierarchyVisibilityAsync(args.Item);
}