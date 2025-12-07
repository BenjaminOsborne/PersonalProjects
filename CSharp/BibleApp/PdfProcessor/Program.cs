using PdfProcessor;


switch (int.Parse("1")) //String parse keeps all branches referenced!
{
    case 0:
        await SpurgeonSermonDownloader.DownloadSpuregonSermonsAsync();
        break;
    case 1:
        await PdfTextExtractor.ExtractSpuregonSermonAsync();
        break;
    default:
        throw new ArgumentException("Unhandled default...");
}