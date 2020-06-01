using Bio;
using Bio.Algorithms.Alignment;
using Bio.SimilarityMatrices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WfComponent.Utils
{
    public static class Fasta
    {

        //　指定したfastaReadName の　Name->Nucs を返します。
        public static IDictionary<string, string> FastaSelectSequence(string fastaFilePath, string fastaReadName)
        {
            string errorMessage = null;
            var fastaLines = FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
                return new Dictionary<string, string>();            // return null; // ? 

            return FastaSequence(fastaLines, fastaReadName);
        }

        public static IDictionary<string, string> FastaSequence(string[] fastaLines, string fastaReadName)
        {
            var resRefName2RefNucs = new Dictionary<string, string>();

            var readNameLine = string.Empty;
            var nucLines = new List<string>();
            var isThisReference = false;

            foreach (var line in fastaLines)
            {
                if (line.Replace(">", "").StartsWith(fastaReadName))
                {
                    isThisReference = true;
                    readNameLine = line;
                    continue;
                }

                if (line.StartsWith(">")) isThisReference = false;  // 次のリード
                if (isThisReference) nucLines.Add(line);

            }

            resRefName2RefNucs.Add(readNameLine, string.Join(string.Empty, nucLines));
            return resRefName2RefNucs;
        }

        public static IEnumerable<KeyValuePair<string, string>> ConsensusFasta(string fastaFilePath)
        {
            var resRefName2RefNucs = new SortedDictionary<string, string>();  // Cons1 ~ Cons3 なので Sortすると順番になる（）

            string errorMessage = null;
            string[] fastaLines = fastaLines = FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage)) return resRefName2RefNucs;  // 読み込みできなければ空のDicを返す。

            var readNameLine = string.Empty;
            var nucLines = new List<string>();
            foreach (var line in fastaLines.Reverse())
            {
                if (line.StartsWith(">"))
                {
                    readNameLine = line.Replace(">", "");
                    nucLines.Reverse();
                    resRefName2RefNucs.Add(readNameLine, string.Join(string.Empty, nucLines));
                    nucLines = new List<string>();
                    continue;
                }
                nucLines.Add(line);
            }
            return resRefName2RefNucs;
        }

        public static IEnumerable<KeyValuePair<string, string>> FastaFile2Dic(string fastaFilePath, bool isDelSpaceName = false)
        {
            var readName2nucs = new Dictionary<string, string>();
            string errorMessage = null;

            string[] fastaLines = null;
            if (System.IO.Path.GetExtension(fastaFilePath).EndsWith("gz"))
                fastaLines = FileUtils.ReadGzFile(fastaFilePath, ref errorMessage);
            else
                fastaLines = FileUtils.ReadFile(fastaFilePath, ref errorMessage);

            if (!string.IsNullOrEmpty(errorMessage)) return readName2nucs;  // 読み込みできなければ空のDicを返す。

            var readNameLine = string.Empty;
            var readCount = 0;
            var nucLines = new List<string>();
            foreach (var line in fastaLines.Reverse())
            {
                if (line.StartsWith(">"))
                {
                    readNameLine = line.Replace(">", "");
                    if (isDelSpaceName)  // ユーザ指定のFasta　同じ名前とかあるものを救済
                    {
                        readCount++;
                        readNameLine = readNameLine.Replace(' ', '_').Replace('?', '_').ToUpper() + "_UserSeq-" + readCount.ToString("000000");
                        // if (readName2nucs.ContainsKey(readNameLine)) continue;
                    }
                    nucLines.Reverse();
                    readName2nucs.Add(readNameLine, string.Join(string.Empty, nucLines).Trim());
                    nucLines = new List<string>();
                    continue;
                }
                nucLines.Add(line);
            }
            return readName2nucs;
        }

        public static IEnumerable<string> FastaNames(string fastaFilePath)
        {
            string errorMessage = null;
            var fastaLines = FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage)) return new string[] { };  // 空配列返す？ null 返す？

            var names = new List<string>();
            foreach (var line in fastaLines)
            {
                if (line.StartsWith(">"))
                    names.Add(line.Split(' ').First().Replace(">", ""));
            }
            // 読み込んだFASTAの名前（順番大事なやつ）
            return names;
        }

        public static IEnumerable<string> FastaFullNames(string fastaFilePath)
        {
            string errorMessage = null;
            var fastaLines = FileUtils.ReadFile(fastaFilePath, ref errorMessage);
            if (!string.IsNullOrEmpty(errorMessage)) return new string[] { };  // 空配列返す？ null 返す？

            var names = new List<string>();
            foreach (var line in fastaLines)
            {
                if (line.StartsWith(">"))
                    names.Add(line.Replace(">", ""));
            }
            // 読み込んだFASTAの名前（順番大事なやつ）
            return names;
        }

        // Fastaに含まれる’N'の個数が、閾値を超えるとtrue
        public static bool IsApplyConsensus(string fastaNuc, int cutoffRatio)
        {
            double nCnt = fastaNuc.ToUpper().Where(s => s == 'N').Count();
            double fastaCnt = fastaNuc.Count();
            double ratio = nCnt / fastaCnt * 100;

            return ratio < (double)cutoffRatio;
        }

        //  Nucreotide(string) から AmnoAcid配列(string)にして返します。
        // startPos/endPos は 1-origin 
        public static string Nucs2AAseq(string nucs, int startPos, int endPos)
        {
            startPos -= 1;
            var nuclen = nucs.Length;
            if(nucs.Length <= endPos)
            {
                var nucAA = nucs.Substring(startPos);
                System.Diagnostics.Debug.WriteLine(nucAA);
            } else
            {
                var nucAA = nucs.Substring(startPos, endPos - startPos);
                System.Diagnostics.Debug.WriteLine(nucAA);
            }

            // 終了位置が endPos が大きい場合最後までを指定
            var toAAnuc = nucs.Length <= endPos ?
                nucs.Substring(startPos):
                nucs.Substring(startPos, endPos - startPos);

            var resAAList = new List<char>();
            foreach (string s in Regex.Split(toAAnuc, @"(?<=\G.{3})(?!$)")) 
                resAAList.Add(Nuc2AA(s));  

            return string.Join(string.Empty, resAAList);
        }

        public static char NonAAchr = '-'; // AA なし 
        public static char Nuc2AA(string codon)
        {
            var nux = codon.ToUpper();
            switch (nux)
            {
                case "AAA": return 'K';
                case "AAC": return 'N';
                case "AAG": return 'K';
                case "AAT": return 'N';
                case "ACA": return 'T';
                case "ACC": return 'T';
                case "ACG": return 'T';
                case "ACT": return 'T';
                case "AGA": return 'R';
                case "AGC": return 'S';
                case "AGG": return 'R';
                case "AGT": return 'S';
                case "ATA": return 'I';
                case "ATC": return 'I';
                case "ATG": return 'M';
                case "ATT": return 'I';
                case "CAA": return 'Q';
                case "CAC": return 'H';
                case "CAG": return 'Q';
                case "CAT": return 'H';
                case "CCA": return 'P';
                case "CCC": return 'P';
                case "CCG": return 'P';
                case "CCT": return 'P';
                case "CGA": return 'R';
                case "CGC": return 'R';
                case "CGG": return 'R';
                case "CGT": return 'R';
                case "CTA": return 'L';
                case "CTC": return 'L';
                case "CTG": return 'L';
                case "CTT": return 'L';
                case "GAA": return 'E';
                case "GAC": return 'D';
                case "GAG": return 'E';
                case "GAT": return 'D';
                case "GCA": return 'A';
                case "GCC": return 'A';
                case "GCG": return 'A';
                case "GCT": return 'A';
                case "GGA": return 'G';
                case "GGC": return 'G';
                case "GGG": return 'G';
                case "GGT": return 'G';
                case "GTA": return 'V';
                case "GTC": return 'V';
                case "GTG": return 'V';
                case "GTT": return 'V';
                case "TAA": return '*';
                case "TAC": return 'Y';
                case "TAG": return '*';
                case "TAT": return 'Y';
                case "TCA": return 'S';
                case "TCC": return 'S';
                case "TCG": return 'S';
                case "TCT": return 'S';
                case "TGA": return '*';
                case "TGC": return 'C';
                case "TGG": return 'W';
                case "TGT": return 'C';
                case "TTA": return 'L';
                case "TTC": return 'F';
                case "TTG": return 'L';
                case "TTT": return 'F';

                default: return NonAAchr;   // null 無いはず
            }
        }

        public static (string seq1, string seq2, string cons) GetNucAlignment(string nucSeq1, string nucSeq2)
        {
            nucSeq1 = nucSeq1.Replace("\"", string.Empty);
            nucSeq2 = nucSeq2.Replace("\"", string.Empty);

            // Create two sequences; normally you'd have this already.
            ISequence nuc1 = new Sequence(Alphabets.AmbiguousDNA, nucSeq1);
            ISequence nuc2 = new Sequence(Alphabets.AmbiguousDNA, nucSeq2);

            // Pick our aligner; there are several to choose from; all in the 
            // Bio.Algorithms.Alignment namespace.
            var algo = new Bio.Algorithms.Alignment.NeedlemanWunschAligner();

            // Setup the aligner with appropriate parameters
            algo.SimilarityMatrix = new SimilarityMatrix(
                                  SimilarityMatrix.StandardSimilarityMatrix.DiagonalScoreMatrix);
            algo.GapOpenCost = -6;
            algo.GapExtensionCost = -1;

            // Execute the alignment.
            var results = algo.AlignSimple(nuc1, nuc2);

            // Display / Process the results.
            System.Diagnostics.Debug.WriteLine("Pairwise Alignment: " + results.Count + " result entries");
            if (results.Count < 1) return (string.Empty, string.Empty, string.Empty);

            IPairwiseSequenceAlignment sequences = results.First();

            var res = sequences.First();
            var align1 = Encoding.Default.GetString(res.FirstSequence.ToArray());
            var align2 = Encoding.Default.GetString(res.SecondSequence.ToArray());
            var cons = Encoding.Default.GetString(res.Consensus.ToArray());

            return (align1, align2, cons);

        }


        public static (string seq1, string seq2, string cons) GetAaAlignment(string aaSeq1, string aaSeq2)
        {
            aaSeq1 = aaSeq1.Replace("\"", string.Empty);
            aaSeq2 = aaSeq2.Replace("\"", string.Empty);

            // Create two sequences; normally you'd have this already.
            ISequence aa1 = new Sequence(Alphabets.AmbiguousProtein, aaSeq1);
            ISequence aa2 = new Sequence(Alphabets.AmbiguousProtein, aaSeq2);

            // Pick our aligner; there are several to choose from; all in the 
            // Bio.Algorithms.Alignment namespace.
            var algo = new Bio.Algorithms.Alignment.NeedlemanWunschAligner();

            // Setup the aligner with appropriate parameters
            // algo.SimilarityMatrix = new SimilarityMatrix(
            //                      SimilarityMatrix.StandardSimilarityMatrix.DiagonalScoreMatrix);
            algo.GapOpenCost = -6;
            algo.GapExtensionCost = -1;

            // Execute the alignment.
            var results = algo.AlignSimple(aa1, aa2);

            // Display / Process the results.
            System.Diagnostics.Debug.WriteLine("Pairwise Alignment: " + results.Count + " result entries");
            if (results.Count < 1) return (string.Empty , string.Empty , string.Empty);

            IPairwiseSequenceAlignment sequences = results.First();

            var res = sequences.First();
            var align1 = Encoding.Default.GetString(res.FirstSequence.ToArray());
            var align2 = Encoding.Default.GetString(res.SecondSequence.ToArray());
            var cons = Encoding.Default.GetString(res.Consensus.ToArray());

            return (align1, align2, cons);

        }

        public static void GetPairLength(string s1, string s2, ref string res)
        {
            var strLen = s1.Length > s2.Length ?
                                s1.Length : s2.Length;

            var lessLength = strLen - res.Length;
            res =  new String(' ' ,  lessLength) + res;

        }



    }
}
