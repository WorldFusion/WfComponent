using Bio;
using Bio.IO;
using Bio.IO.GenBank;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WfComponent.Utils
{
    // use packege, see detail.
    // https://github.com/dotnetbio/dotnetbio.github.io
    public static class Genbank
    {

        // NCBI Program-API URL sample
        // ## ans1  like xml
        // https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=nucleotide&id=1821698063&retmode=text
        // ## genbank
        // https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=nucleotide&rettype=gbwithparts&retmode=text&id=1821698063


        public static readonly string genbankFooter = "gb";

        /// <summary>
        /// NCBI API URL Template
        /// </summary>
        public static string NcbiGenbankDownloadUrlTemplate = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=nucleotide&rettype=gbwithparts&retmode=text&id={0}";

        // 1つの NCBI Gid からSequence情報をパースして返します。
        public static SequenceProperties GetSequenceProperties(string gid, ref string message )
        {
            var url = string.Format(NcbiGenbankDownloadUrlTemplate, gid);

            // SequenceProperties prop = new SequenceProperties();
            WebClient downloader = new WebClient();
            try
            {
                Stream stream = downloader.OpenRead(url);
                var props = GetGenbankPropertoes(stream);
                if (props.Any())
                    return props.First(); // gid なので一個だけのハズ

            }
            catch (Exception e)
            {
                message = "Genbank download or parse Exception. gid = " + 
                                    gid + "  " +
                                    e.Message;
            }
            return null;

        }




        // uncompleted 未完成メソッド
        // Genbank file parser.　
        public static IEnumerable<SequenceProperties> GetInfluenzaProperties(string filePath, ref string message)
        {
            IEnumerable<SequenceProperties> sequences = null;

            FileStream fs = File.OpenRead(filePath);
            try
            {
                sequences = GetGenbankPropertoes(fs);
            } 
            catch (Exception e)
            {
                message = e.Message;
            }
            return sequences;
        }

        private static IEnumerable<SequenceProperties> GetGenbankPropertoes(Stream stream)
        {
            var props  = new List<SequenceProperties>();
            using (stream)
            {
                ISequenceParser parser = new GenBankParser();
                IEnumerable<ISequence> sequences = parser.Parse(stream);
                foreach (var seq in sequences)
                {
                    SequenceProperties prop = new SequenceProperties();
                    System.Diagnostics.Debug.WriteLine(seq.ID);

                    // GenBankMetadata dat = (GenBankMetadata) sq.Metadata.Values.First();
                    if (seq.Metadata.ContainsKey("GenBank"))
                    {
                        GenBankMetadata dat = (GenBankMetadata)seq.Metadata["GenBank"];
                        System.Diagnostics.Debug.WriteLine(dat.Definition);

                        prop.accession = seq.ID;
                        prop.name = dat.Definition;
                        prop.sequence = Encoding.Default.GetString(seq.ToArray());

                        if (dat.Features.CodingSequences.Any())
                        {
                            var aaprops = new List<AminoProperties>();
                            foreach (var aaseq in dat.Features.CodingSequences)
                            {
                                var subLocation = new Dictionary<int, int>(); ;
                                if (aaseq.Location.SubLocations.Any())
                                {
                                    foreach (var loc in aaseq.Location.SubLocations)
                                    {
                                        var subStartPos = loc.LocationStart;
                                        var subEndPos = loc.LocationEnd;
                                        subLocation.Add(subStartPos, subEndPos);
                                    }
                                }
                                else
                                {
                                    subLocation.Add(
                                        aaseq.Location.LocationStart,
                                        aaseq.Location.LocationEnd);
                                }

                                var cdsGene = string.IsNullOrEmpty(aaseq.GeneSymbol) ?
                                                    string.Join(" ", aaseq.LocusTag) :
                                                    aaseq.GeneSymbol;

                                aaprops.Add(
                                    new AminoProperties()
                                    {
                                        Name = cdsGene,
                                        AAseq = aaseq.Translation,
                                        ProteinIds = aaseq.ProteinId,
                                        SubLocations = subLocation
                                    });
                            }
                            prop.aminoProps = aaprops;

                        }

                        if (dat.Features.Genes.Any())
                        {
                            var nucprop = new List<NucProperties>();
                            foreach (var gene in dat.Features.Genes)
                            {
                                nucprop.Add(
                                    new NucProperties()
                                    {
                                        Name = gene.GeneSymbol,
                                        startPos = gene.Location.LocationStart,
                                        endPos = gene.Location.LocationEnd
                                    });
                            }
                            prop.nucProps = nucprop;
                        }
                    }
                    props.Add(prop);
                }
            }
            return props;
        }


    }


    // NCBI use Properties 
    public class SequenceProperties
    {
        public string accession;
        public string name;     // Influenza A virus (A/swine/England/453/2006(H1N1))
        public string type;
        public string symbol;  // GeneSymbol	"\"HA\""
        public string sequence;
        public IEnumerable<AminoProperties> aminoProps;
        public IEnumerable<NucProperties> nucProps;

    }

    public class AminoProperties
    {
        public string AAseq;
        public string Name;
        public IEnumerable<string> ProteinIds;
        public IDictionary<int, int> SubLocations;
    }

    public class NucProperties
    {
        // public string NucSeq;
        public string Name;

        public int startPos;
        public int endPos;
        public string Comment;

    }

}
