namespace WfComponent.Utils
{
    public static class NcbiUtils
    {

        public static string NcbiNucreotideUrlTemplate = "https://www.ncbi.nlm.nih.gov/nuccore/{0}";

        public static void NcbiNcreotidePage(string ncbiAccession)
        {

            var url = string.Format(NcbiNucreotideUrlTemplate, ncbiAccession);
            System.Diagnostics.Debug.WriteLine(url);
            System.Diagnostics.Process.Start(
                                new System.Diagnostics.ProcessStartInfo(url)
                                { UseShellExecute = true });
        }

    }
}
