using System;
using System.Collections.Generic;
using System.Linq;

namespace WfComponent.External
{
    public static class GuppyCommand
    {
        public static string Guppy1DBinName = "guppy_basecaller_1d2.exe";
        public static string Guppy2DBinName = "guppy_basecaller.exe";
        public static string GuppyBarcoderBinName = "guppy_barcoder.exe";
        public static string GuppyAlignerBinName = "guppy_aligner.exe";

        public static string GuppyBasecallOutDir = "basecall";
        public static string GuppyBarcodeOutDir = "barcode";

        public static (string commandArgs, string outFastqDir, string flowcell, string kit)
            GuppyBasecallCommandArgs(string inDir, string outDir, string otherParameter, ref string message)
        {

            var di = new System.IO.DirectoryInfo(inDir);
            var fs = di.EnumerateFiles("*.fast5", System.IO.SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (!fs.Any())
            {
                message = "not found fast5 files.";
                return (string.Empty, string.Empty, string.Empty, string.Empty);
            }

            // var (sequenceKitId, flowcellType, errorMessage) = HDF5api.GetSequenceKitFlowcellType(fs.First().FullName);
            var errorMessage = string.Empty;
            var sequenceKitId = string.Empty;
            var flowcellType = string.Empty;
            try
            {
                var fast5attributs = new Fast5(fs.First().FullName);
                if (string.IsNullOrEmpty(fast5attributs.errorMessage))
                {

                    sequenceKitId = fast5attributs.attributes.Where(s => s.Key.StartsWith(Fast5.sequence))
                                                               .ToArray()
                                                               .First()
                                                               .Value.ToString();
                    flowcellType = fast5attributs.attributes.Where(s => s.Key.StartsWith(Fast5.flowcell))
                                                               .ToArray()
                                                               .First()
                                                               .Value.ToString();

                    // fast5 log out.
                    Utils.FileUtils.WriteUniqDateLog("fast5 read is normal end." + Environment.NewLine +
                                                                   string.Join(Environment.NewLine, fast5attributs.logMesage));
                }
                else
                {
                    errorMessage = fast5attributs.errorMessage;
                }
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Fast5 read error.\n" + e.Message);
                errorMessage = e.Message;
            }

            if (!string.IsNullOrEmpty(errorMessage) || string.IsNullOrEmpty(sequenceKitId) || string.IsNullOrEmpty(flowcellType)) 
            {
                message = errorMessage + Environment.NewLine +
                                "no-read fast5 fllowcell-type or kit-name.";
                return (string.Empty, string.Empty, string.Empty, string.Empty);
            }



            var workDir = System.IO.Path.Combine(
                    outDir,
                    GuppyBasecallOutDir
                    );

            var args = new List<string>();
            args.Add("Fast");
            args.Add("--flowcell");
            args.Add(flowcellType.ToUpper());
            args.Add("--kit");
            args.Add(sequenceKitId.ToUpper());
            args.Add("--input_path");
            args.Add("\"" + inDir + "\"");
            args.Add("--save_path");
            args.Add("\"" + workDir + "\"");
            args.Add("--recursive  -q 0"); // inputを再起的に探す
            if (!string.IsNullOrEmpty(otherParameter)) args.Add(otherParameter);

            var commandArgs = string.Join(" ", args);
            return (commandArgs, workDir, flowcellType, sequenceKitId);
        }

        public static (string commandArgs, string outFastqDir)
            GuppyBarcodeCommandArgs(string inDir, string outDir, string kitName, string otherParameter, ref string message)
        {
            message = string.Empty;
            var workDir = System.IO.Path.Combine(
                                             outDir,
                                             GuppyBarcodeOutDir
                                             );

            var args = new List<string>();
            args.Add("--trim_barcodes");
            args.Add("--input_path");
            args.Add("\"" + inDir + "\"");
            args.Add("--save_path");
            args.Add("\"" + workDir + "\"");
            args.Add("--barcode_kits");
            args.Add(kitName.ToUpper());
            args.Add("--recursive"); // inputを再起的に探す 
            if (!string.IsNullOrEmpty(otherParameter)) args.Add(otherParameter);

            var command = string.Join(" ", args);
            return (command, workDir);
        }

        public static string GuppyFastBasecallCommandArgs(string inDir, string outDir, string confName, ref string message)
        {
            message = string.Empty;
            var di = new System.IO.DirectoryInfo(inDir);
            var fs = di.EnumerateFiles("*.fast5", System.IO.SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (!fs.Any())
            {
                message = "not found fast5 files.  search directory is " + inDir + " !";
                return string.Empty;
            }

            var args = new List<string>();
            args.Add("--config ");
            args.Add(confName);   // guppy v3+ から Fastモード 使う為に cfg に変更
            args.Add("--input_path ");
            args.Add("\"" + inDir + "\"");
            args.Add("--save_path ");
            args.Add("\"" + outDir + "\" ");
            args.Add("--recursive  -q 0"); // inputを再起的に探す

            var commandArgs = string.Join(" ", args);
            return commandArgs;
        }

        public static string GuppyFastBarcodeCommandArgs(string inDir, string outDir, ref string message, string barcode_kits = null, string kit = null)
        {
            message = string.Empty;
            var di = new System.IO.DirectoryInfo(inDir);
            var fs = di.EnumerateFiles("*.fastq", System.IO.SearchOption.AllDirectories);  // サブフォルダに分かれている場合がある
            if (!fs.Any())
            {
                message = "not found fastq files.";
                return string.Empty;
            }

            var args = new List<string>();
            if (!string.IsNullOrEmpty(barcode_kits))
                args.Add("--barcode_kits " + barcode_kits);  // guppy v3+ から Fastモード 使う為に cfg に変更
            if (!string.IsNullOrEmpty(kit))                         // Old command の時に利用
                args.Add("--kit " + kit);

            args.Add("--input_path ");
            args.Add("\"" + inDir + "\"");
            args.Add("--save_path ");
            args.Add("\"" + outDir + "\" ");
            args.Add("--recursive  -q 0"); // inputを再起的に探す

            var commandArgs = string.Join(" ", args);
            return commandArgs;
        }

    }
}
