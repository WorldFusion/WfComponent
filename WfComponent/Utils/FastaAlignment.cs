using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WfComponent.Utils
{
    public static class FastaAlignment
    {
        public static readonly char delimiter = '\t';

        // コンセンサス　アライメントが正常に作成されたらtrue
        // Consensus.fasta は multi-fasta
        // return : Alginment file path
        public static string ConsensusAlign(string consensusPath, string oneSeqReferencePath, ref string message)
        {
            // コンセンサス配列が作成できなかった分節もある
            if (string.IsNullOrEmpty(consensusPath) ||
                !FileUtils.IsFileNumberOfLinesAbove(consensusPath, 1))
            {
                message += consensusPath + " is not valid (non-path, or empty) ";
                return string.Empty;
            }
            var fastaDic = Fasta.FastaFile2Dic(consensusPath);

            var writeAligns = new List<string>();
            foreach (var name2nuc in fastaDic)  // Consensusには Multi-Fasta形式。　Top1 - Top3 まであり得る
            {
                if (string.IsNullOrEmpty(name2nuc.Key) ||
                    string.IsNullOrEmpty(name2nuc.Value)) continue;

                var referName = GetReferenceName(name2nuc.Key);
                if (string.IsNullOrEmpty(referName))
                {
                    message += "Unable to retrieve Consensus reference gid : " + name2nuc.Key + Environment.NewLine;
                    continue;
                }

                // コンセンサスの基になったリファレンス塩基配列
                var referNucs = Fasta.FastaSelectSequence(oneSeqReferencePath, referName);
                if (!referNucs.Any()) continue;

                (string referSeq, string consSeq, string alignSeq) =
                    Fasta.GetNucAlignment(referNucs.First().Value, name2nuc.Value);
                if (string.IsNullOrEmpty(referSeq) || string.IsNullOrEmpty(consSeq) || string.IsNullOrEmpty(alignSeq))
                {
                    message += "Unable to alignment sequences  : " + name2nuc.Key + Environment.NewLine;
                    continue;
                }

                var alignName = string.Empty;
                var consName = name2nuc.Key;
                Fasta.GetPairLength(referName, name2nuc.Key, ref referName);
                Fasta.GetPairLength(referName, name2nuc.Key, ref alignName);
                Fasta.GetPairLength(referName, name2nuc.Key, ref consName);

                writeAligns.Add(referName + delimiter + referSeq);
                writeAligns.Add(consName + delimiter + consSeq);
                writeAligns.Add(alignName + delimiter + alignSeq);
                writeAligns.Add(string.Empty);
            }

            // 書き込むものがあれば。
            if (writeAligns.Any())
            {
                var alignFilePath = Path.ChangeExtension(consensusPath, "align");
                var fwMessage = string.Empty;
                FileUtils.WriteFile(alignFilePath, writeAligns, ref fwMessage);
                if (string.IsNullOrEmpty(fwMessage))
                    return alignFilePath;
                else
                    message += "write alignment error, " + fwMessage + Environment.NewLine;
            }
            return string.Empty;
        }

        public static string GetReferenceName(string fastaName)
        {
            if (string.IsNullOrEmpty(fastaName)) return string.Empty;
            // >rep-barcode07-all|InfluenzaAMP|consensus1 gi|295445910|gb|HM142743|Influenza cover=1 depth=5054.48
            var referNames = fastaName.Split(' ');
            if (referNames.Length < 2) return string.Empty;

            return referNames[1];
        }

        // Consensus に書いてあるリファレンス　// 基本はNCBI Influenza 
        public static string GetConsensusReference(string fastaName)
        {
            if (string.IsNullOrEmpty(fastaName)) return string.Empty;


            // >rep-barcode07-all|InfluenzaAMP|consensus1 gi|295445910|gb|HM142743|Influenza cover=1 depth=5054.48
            var referNames = fastaName.Split(' ');
            if (referNames.Length < 2) return string.Empty;

            var referName = referNames[1]; // gi|295445910|gb|HM142743|Influenza 
            if (string.IsNullOrEmpty(referName) ||
                 !referName.StartsWith("gi")) return string.Empty;  // FluGAS独自IDは gn で始まる

            var ncbiIds = referName.Split('|');
            if (referName.Length < 2) return string.Empty;

            return ncbiIds[1];
        }

        // Consensus nuc から AAにする
        public static string GetConsesusAA(string consensusNucs, NucProperties gene, AminoProperties cds)
        {

            var consensusAA = string.Empty;

            if (cds.SubLocations == null)
            {
                consensusAA = Fasta.Nucs2AAseq(
                                                    consensusNucs,
                                                    gene.startPos,
                                                    gene.endPos);
            }
            else
            {
                // CDS に join がある場合。
                foreach (var loc in cds.SubLocations)
                {
                    consensusAA += Fasta.Nucs2AAseq(
                                                    consensusNucs,
                                                    loc.Key,
                                                    loc.Value);
                }
            }
            return consensusAA;
        }


        public static void AlignmentReferenceTreeImple(string referencePath, string consensusFilePath, ref string message, string basename = "")
        {
            // File check 
            if (FileUtils.FileSize(consensusFilePath, ref message) < 100)
            {  // file read error のメッセージに付け加える。
                message += Environment.NewLine + "not a valid file " + consensusFilePath;
                return;
            }

            var refreFastaLines = FileUtils.ReadFile(referencePath, ref message);
            if (!string.IsNullOrEmpty(message))
            {
                message += Environment.NewLine + "not a valid file " + referencePath;
                return;
            }

            // consTop -> nuc
            var consensusNameNuc =
                                Fasta.ConsensusFasta(consensusFilePath);

            var consensusCnt = 0;
            foreach (var consNuc in consensusNameNuc)
            {
                consensusCnt++;
                var consensusCoronaRefMargeFile =  // merge temp file
                    Path.Combine(
                        Path.GetDirectoryName(consensusFilePath),
                        Path.GetFileNameWithoutExtension(consensusFilePath) + "-Top" + consensusCnt + basename + ".fna");  // Top1-corona.fna

                var mergeLine = new List<string>()
                {
                    ">" + consNuc.Key,
                    consNuc.Value,
                };
                mergeLine.AddRange(refreFastaLines);

                FileUtils.WriteFile(consensusCoronaRefMargeFile,
                                                                         mergeLine,
                                                                         ref message);
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine + "Consensus, Corona reference create error, ";
                    continue;
                }

                var clustalOProcess =
                    new External.ClustalOmega(
                        new External.Properties.ClustalOmegaOptions()
                        {
                            targetFile = consensusCoronaRefMargeFile,

                        });

                var clustalO = clustalOProcess.StartProcess();

                if (ConstantValues.NormalEndMessage.Equals(clustalO))
                {
                    message += Environment.NewLine + clustalOProcess.GetExecLog();
                    message += Environment.NewLine + clustalOProcess.GetMessage();
                }
            }

        }

    }
}
