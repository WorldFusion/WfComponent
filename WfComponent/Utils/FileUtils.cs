﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WfComponent.Utils
{
    public static class FileUtils
    {

        public static string[] ReadFile(string filepath, ref string errorMessage)
        {
            try
            {
                errorMessage = string.Empty;
                filepath = LinuxPath2WindowsPath(filepath);
                if (!File.Exists(filepath))
                {
                    errorMessage = "not found file:" + filepath;
                    return new string[] { }; // ファイルない
                }
                return File.ReadAllLines(filepath, Encoding.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine(filepath);
                Console.WriteLine(e.Message);
                errorMessage = "read file error : " + filepath + Environment.NewLine +  e.Message;
                return new string[] { };
            }
        }

        public static string[] ReadFileStream(string filepath, ref string errorMessage)
        {
            try
            {
                filepath = LinuxPath2WindowsPath(filepath);
                if (!File.Exists(filepath))
                {
                    errorMessage = "not found file:" + filepath;
                    return new string[] { }; // ファイルない
                }
                errorMessage = string.Empty;
                string allLine = null;

                using (StreamReader sr = new StreamReader(filepath))
                {
                    allLine = sr.ReadToEnd();
                }
                return allLine.Split('\n').Select(s => s.Trim()).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(filepath);
                Console.WriteLine(e.Message);
                errorMessage = e.Message;
                return new string[] { };
            }
        }

        public static string[] ReadGzFile(string gzfilepath, ref string errorMessage)
        {
            gzfilepath = LinuxPath2WindowsPath(gzfilepath);
            if (!File.Exists(gzfilepath) ||
                !Path.GetExtension(gzfilepath).EndsWith("gz", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "not found file:" + gzfilepath;
                return new string[] { }; // gzファイルでない
            }

            // errorMessage = null;
            var gzlines = new List<string>();
            try
            {
                using (FileStream fin = new FileStream(gzfilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (GZipStream gzin = new GZipStream(fin, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzin))
                {
                    while (!reader.EndOfStream)
                    {
                        gzlines.Add(reader.ReadLine().Trim());
                    }
                }
            }
            catch (Exception e)
            {
                // エラーの場合からのリストを返すだけ
                errorMessage = e.Message;
                return new string[] { };
            }
            // var lines = gzlines.ToArray();
            return gzlines.ToArray();
        }

        // ファイルの新規or上書き   // isAppend=true で 追記。
        public static void WriteFile(string outFilePath, IEnumerable<string> lines, ref string errorMessage, bool isAppend = false)
        {
            outFilePath = LinuxPath2WindowsPath(outFilePath);
            errorMessage = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(outFilePath))
                {
                    errorMessage = "not set write file path";
                    return;
                }

                if (!IsDirectoryCreate(Path.GetDirectoryName(outFilePath)))
                {
                    errorMessage = "not create directory";
                    return;
                }
                using (StreamWriter writer = new StreamWriter(outFilePath, isAppend, Encoding.Default))
                {
                    writer.WriteLine(string.Join(Environment.NewLine, lines));
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        public static void WriteFileFromString(string outFilePath, string line, ref string errorMessage)
        {
            outFilePath = LinuxPath2WindowsPath(outFilePath);
            errorMessage = string.Empty;
            try
            {
                if (!IsDirectoryCreate(Path.GetDirectoryName(outFilePath))) {
                    errorMessage = "not create directory";
                    return;
                }

                if (File.Exists(outFilePath)) File.Delete(outFilePath);
                using (StreamWriter writer = new StreamWriter(outFilePath, false, Encoding.Default))
                {
                    writer.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        public static void WriteUniqDateLog(string outLogMessage)
        {
            var ignoreMessage = string.Empty;
            WriteFileFromString(GetUniqDateLogFile(), outLogMessage, ref ignoreMessage);
        }

        public static string GetUniqDateLogFile(string prefix = null)
        {
            var logDir = Path.Combine(
             AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
             ConstantValues.CurrentLogDir);
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var logFileName = string.IsNullOrEmpty(prefix) ?
                                            FileUtils.UniqueDateString() + ".log" :
                                            prefix + FileUtils.UniqueDateString() + ".log";
            var logfile = Path.Combine(
                                        logDir,
                                        logFileName);
            return logfile;
        }

        public static bool IsDirectoryCreate(string directoryPath)
        {
            try { 
                var parentDir = Path.GetDirectoryName(directoryPath);
                if (! System.IO.Directory.Exists(parentDir))
                    IsDirectoryCreate(parentDir);

                if (! System.IO.Directory.Exists(directoryPath))
                    System.IO.Directory.CreateDirectory(parentDir);
            } catch {
                return false;
            }
            return true;
        }

        // windows のPath を LinuxSubSystem のPath へ変換
        public static string WindowsPath2LinuxPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            var d = string.IsNullOrWhiteSpace(path) ? new string[] { } : path.Split(':');
            var linuxPath = string.Empty;
            if (d.Count() > 1)
                linuxPath = "/mnt/" + d[0].ToLower() + d[1].Replace("\\", "/");
            else
                linuxPath = path.Replace("\\", "/");

            return linuxPath;
        }

        public static string LinuxPath2WindowsPath(string linuxPath)
        {
            if (string.IsNullOrEmpty(linuxPath)) return string.Empty;
            linuxPath = linuxPath.Replace("\\", "/");
            if (!linuxPath.StartsWith("/mnt/")) return linuxPath;

            var pieces = linuxPath.Split('/');
            var drive = pieces[2].ToUpper();
            var dirct = string.Join("\\", pieces.Skip(3));

            var winPath = drive + ":\\" + dirct;
            return winPath;
        }

        // MiSeq 由来のFastq ファイル名から、共通部分（-- の 前半部分を共通としている）
        // 13I-005_S6_L001_R1_001.fastq.gz / 13I-005_S6_L001_R2_001.fastq.gz
        // -> 13I-005_S6 が basename
        private static readonly string[] certainWords = new string[] { "--", "_L001_" };
        private static readonly string[] pairLetter = new string[] { "_R1_", "_R2_","_1.fastq", "_2.fastq", "_1.fastq.gz", "_2.fastq.gz" };
        public static string GetMiseqFastqBaseName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return string.Empty;
            // C:\test\test.add.txt -> test

            // var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileName = Path.GetFileName(filePath);
            if (pairLetter.Where(s => fileName.Contains(s)).Any())
            {
                fileName = fileName.Split(pairLetter, StringSplitOptions.None).First();
            }

            if (certainWords.Where(s => fileName.Contains(s)).Any())
            {
                fileName = fileName.Split(certainWords, StringSplitOptions.None).First();
            }

            
            // var baseName = fileName.Split(basenameSplitStr, StringSplitOptions.None).First();
            return fileName;
        }

        // 引数のstring 文字列がデフォルトエンコードならtrue
        public static bool IsOneByteString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            byte[] byte_data = Encoding.Default.GetBytes(str);
            if (byte_data.Length == str.Length)
                return true;
            else
                return false;
        }

        // FilePath で与えられたファイルがLineCount以上ならtrue
        public static bool IsFileNumberOfLinesAbove(string filePath, long lineCount)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;

            var filetext = File.ReadAllText(filePath, Encoding.Default).Split('\n'); ;
            if (filetext.Count() >= lineCount) return true;
            return false;

        }

        // FilePath で与えられたファイルのByte数を返します
        public static long FileSize(string filePath, ref string message)
        {
            if ( string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return 0L;
            var findfo = new FileInfo(filePath);
            return findfo.Length;
        }

        // 正常終了の場合は False
        public static bool FileCopy(string fromFile, string toFile, ref string message, bool isBackup = true)
        {
            try
            {
                if (isBackup && File.Exists(toFile))
                {
                    File.Move(toFile,
                                   Path.Combine(Path.GetDirectoryName(toFile),
                                          Path.GetFileNameWithoutExtension(toFile)
                                          + UniqueDateString()
                                          + Path.GetExtension(toFile)));
                }
                File.Copy(fromFile, toFile);
            }
            catch (IOException e)
            {
                message = "File copy error, Please check log.\n" + e.Message;
                return true;
            }
            return false;
        }

        public static bool FileMove(string fromFile, string toFile, ref string message, bool isBackup = true)
        {
            try
            {
                if (isBackup && File.Exists(toFile))
                {
                    File.Move(toFile,
                                   Path.Combine(Path.GetDirectoryName(toFile),
                                          Path.GetFileNameWithoutExtension(toFile) + UniqueDateString() + Path.GetExtension(toFile)));
                }
                // 同じファイルがあると IOError
                if (File.Exists(toFile))File.Delete(toFile);

                File.Move(fromFile, toFile);
            }
            catch (Exception e)
            {
                message = "File move error, Please check log.\n" + e.Message;
                return true;
            }
            return false;
        }

        public static bool FileBackupAddUniqDatetime(string bkFile, ref string message)
        {
            message = string.Empty;
            if (!File.Exists(bkFile)) return false;
            try
            {
                File.Move(bkFile,
                               Path.Combine(Path.GetDirectoryName(bkFile),
                                      Path.GetFileNameWithoutExtension(bkFile) + UniqueDateString() + Path.GetExtension(bkFile)));
            }
            catch (Exception e)
            {
                message = "File backup (System.IO.File.Move) error, Please check log." + Environment.NewLine + e.Message;
                return true;
            }
            return false;

        }


        public static string GetFileBaseName(string file)
        {
            if (string.IsNullOrEmpty(file)) return string.Empty;
            var isValid = true;
            while (isValid)
            {
                file = Path.GetFileNameWithoutExtension(file);
                if (!file.EndsWith(".gz") && !file.EndsWith(".fasta") && !file.EndsWith(".fastq"))
                    isValid = false;
            }

            return file;

            // var splitChars = new char[] { '.', '_' };
            // var name = Path.GetFileNameWithoutExtension(file).Split(splitChars).First();
            // return name;
        }

        ///  
        /// 複数Fasta/Fastq の マージを行います(省メモリー版?)
        /// files : fasta or fastq (full path) file list.
        /// outFile : out put merged file name(full path)
        /// 
        public static string MergeFiles(string outFilePath, IEnumerable<string> files, bool isAppend = false)
        {
            // init.
            var message = string.Empty;

            try
            {
                using (StreamWriter writer = new StreamWriter(outFilePath, isAppend, Encoding.Default))
                {
                    foreach(var file in files)
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            writer.Write(sr.ReadToEnd() );
                        }
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return message;
        }

        ///  
        ///  複数Fasta/Fastq の マージを行います
        /// files : fasta or fastq (full path) file list.
        /// outFile : out put merged file name(full path)
        /// 
        public static string MergeGzFiles(string outFilePath, IEnumerable<string> files, bool isAppend = false)
        {
            // init.
            var message = string.Empty;

            try
            {
                using (StreamWriter writer = new StreamWriter(outFilePath, isAppend, Encoding.Default))
                {
                    foreach (var file in files)
                    {
                        using (FileStream fin = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (GZipStream gzin = new GZipStream(fin, CompressionMode.Decompress))
                        using (var reader = new StreamReader(gzin))
                        {
                            writer.Write(reader.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            return message;
        }

        public static string DirectoryCopy(string sourcePath, string destinationPath, bool overwrite = true, bool copySubDirrectory = true)
        {

            try { 

                var sourceDirectory = new DirectoryInfo(sourcePath);
                var destinationDirectory = new DirectoryInfo(destinationPath);

                // コピー元データの存在確認
                if (!sourceDirectory.Exists)
                   return ("not found souece directory: " + sourcePath);

                // ディレクトリの作成
                if (!destinationDirectory.Exists)
                {
                    destinationDirectory.Create();
                    destinationDirectory.Attributes = sourceDirectory.Attributes;
                }

                // ディレクトリの中身のファイルをコピー
                foreach (var file in sourceDirectory.GetFiles())
                    file.CopyTo(Path.Combine(destinationDirectory.FullName, file.Name), overwrite);


                // 再帰コピー
                if (copySubDirrectory)
                {
                    foreach (var directory in sourceDirectory.GetDirectories())
                    {
                        DirectoryCopy(
                            directory.FullName,
                            Path.Combine(destinationDirectory.FullName, directory.Name),
                            overwrite,
                            copySubDirrectory
                        );
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return ConstantValues.NormalEndMessage;
        }


        public static string RemoveInvalidFileChar(string fileName)
        {
            // char[] invalidChars = Path.GetInvalidFileNameChars();   // not use character
            var nonValidName = string.Concat(
                    fileName.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));  
            var fixedFileName = nonValidName.Replace(" ", string.Empty);    // 空白は削除
            return fixedFileName;
        }

        public static string CompressGzFile(string filePath, ref string message)
        {
            var gzFile = filePath + ".gz";
            try
            {
                // 入力用ストリーム
                using (FileStream fin = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (FileStream fout = File.Create(gzFile))                 // 出力用ストリーム
                using (GZipStream gzout = new GZipStream(fout, CompressionMode.Compress))
                    fin.CopyTo(gzout);
            }
            catch (Exception e)
            {
                message = e.Message;
                return string.Empty;
            }
            return gzFile;
        }

        // File の 検索する: 先頭ファイル名一致
        public static IEnumerable<string> FindFile(string searchDir, string fileName, string footer = "")
        {

            if (footer != "" && !footer.StartsWith("."))
                footer = "." + footer;
            try { 
                var di = new DirectoryInfo(searchDir);
                var files = di.EnumerateFiles(fileName + "*" + footer, SearchOption.AllDirectories);

                if (files.Any())
                {
                    var fastas = files.Select(s => s.FullName).ToArray();
                    return fastas;
                }
            } catch (Exception e)
            {
                // 管理者権限の必要な時に
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return new string[] { };
        }

        // File の 検索する: 中間ファイル名一致
        public static IEnumerable<string> PartOfSearchFile(string searchDir, string fileBaseName, string footer = "")
        {
            if (footer != "" && !footer.StartsWith("."))
                footer = "." + footer;

            // 対象ファイルを検索する
            string[] fileList = Directory.GetFileSystemEntries(searchDir, "*" + fileBaseName + "*" + footer);
            return fileList;
        }

            public static string GetEscapePath(string filePath)
        {
            Regex matcher = new Regex(Regex.Escape(filePath));
            return matcher.ToString();
        }

        public static char[] DubleqoteChar = { '"' };
        public static string GetDoubleQuotationPath(string path)
        {

            var pathElm = path.TrimStart(DubleqoteChar)
                                         .TrimEnd(DubleqoteChar);

            path = "\"" + pathElm + "\"";
            return path;
        }

        public static IEnumerable<string> GetDoubleQuotationPaths(IEnumerable<string> paths)
        {
            var pathElm = paths.Select(s => GetDoubleQuotationPath(s))
                                            .ToArray();

            return pathElm;
        }

        // Pairend を取得する
        public static IDictionary<string, string> GetPairFile(string[] fileNames)
        {
            var resDic = new Dictionary<string, string>();
            Array.Sort(fileNames);
            for(int i =0; i < fileNames.Count(); i++)
            {
                // 最後の要素がSingle
                if (fileNames.Last() == fileNames[i])
                {
                    resDic.Add(fileNames.Last(), string.Empty);
                    return resDic;
                }
                var f1 = fileNames[i];
                var f2 = fileNames[i+1];
                // ソート済みの2つを比較して、1文字違い（fwd.rev）なら Pairend
                if (IsToleranceString(f1, f2, 1))
                {
                    resDic.Add(f1, f2);
                    i++;
                }
                else
                {
                    resDic.Add(f1, string.Empty);
                }
            }
            return resDic;
        }

        // 2つの文字列で、異なる文字が許容範囲であるかを判定
        public static bool IsToleranceString(string s1 , string s2, int allowable)
        {
            var disc = System.Math.Abs(s1.Length - s2.Length);
            var shortLength = (s1.Length < s2.Length) ? s1.Length : s2.Length;
            for(int i=1; i < shortLength; i++)
                if (!s1[i].Equals(s2[i]))
                    disc++;

            return disc <= allowable;
        }

        // date pattern の 集約
        public static string UniqueDateString()
            => DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
        // date pattern の 集約
        public static string LogDateString()
            => DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss");
    }
}
