using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static WfComponent.Utils.FileUtils;
namespace WfComponent.Utils
{
    public class WebClientWrap
    {
        public async Task<string> DownloadFileAsync(string url, string saveFilePath, CancellationToken cancellationToken)
        {
            using (var webClient = new WebClient())
            {
                cancellationToken.Register(webClient.CancelAsync);

                try
                {
                    var task = webClient.DownloadFileTaskAsync(url, saveFilePath);
                    await task; // This line throws an exception when cancellationTokenSource.Cancel() is called.
                }
                catch (WebException ex) when (ex.Message == "The request was aborted: The request was canceled.")
                {
                    throw new OperationCanceledException();
                }
                catch (TaskCanceledException)
                {
                    throw new OperationCanceledException();
                }

                return saveFilePath;
            }
        }

        private bool isCancel = false;
        public void DownloadCancel()
            => this.isCancel = true;

        public string Download(string url, string saveFilePath, ref string message, IProgress<string> logger = null)
        {
            logger ??= new Progress<string>(s => System.Diagnostics.Debug.WriteLine(s));

            long filesize = 10000; // byte
            var tmpFile = Path.Combine(
                                    Path.GetDirectoryName(saveFilePath),
                                    Path.GetFileNameWithoutExtension(saveFilePath) +
                                        UniqueDateString() +
                                        Path.GetExtension(saveFilePath));

            // download file
            using (var wc = new WebClient())
            {
                wc.DownloadProgressChanged += OnDownloadProgressChanged;
                wc.DownloadFileCompleted += OnDownloadFileCompleted;

                logger.Report("## download start ##");
                logger.Report(saveFilePath + " as " + tmpFile);
                DownloadResult downloadResult = DownloadResult.CompletedSuccessfuly;

                void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
                {
                    if (filesize < e.BytesReceived)
                    {
                        logger.Report("file download.... " + e.BytesReceived / 1000 + "k Byte");
                        filesize = e.BytesReceived + filesize; // +1k
                    }
                    if (isCancel)
                    {
                        ((WebClient)sender).CancelAsync();
                    }
                }

                void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
                {
                    if (e.Cancelled)
                    {
                        downloadResult = DownloadResult.Cancelled;
                        return;
                    }
                    if (e.Error != null)
                    {
                        downloadResult = DownloadResult.ErrorOccurred;
                        return;
                    }
                }

                try
                {
                    Task task = wc.DownloadFileTaskAsync(url, tmpFile);
                    task.Wait();
                }
                catch (AggregateException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                switch (downloadResult)
                {
                    case DownloadResult.CompletedSuccessfuly:
                        break;
                    case DownloadResult.Cancelled:
                        break;
                    case DownloadResult.ErrorOccurred:
                        break;
                }
            }

            // 正常にDL出来たはず
            if (File.Exists(saveFilePath))
                if (!FileUtils.FileBackupAddUniqDatetime(saveFilePath, ref message))
                    return string.Empty;  // error

            File.Move(tmpFile, saveFilePath);
            return saveFilePath;
        }



        public string DownloadHttpWebRequest(string url, string saveFilePath, ref string message, IProgress<string> logger = null)
        {
            logger ??= new Progress<string>(s => System.Diagnostics.Debug.WriteLine(s));

            int cycle = 0; 
            int dlsize = 0;
            var tmpFilePath = Path.Combine(
                                            Path.GetDirectoryName(saveFilePath),
                                            Path.GetFileNameWithoutExtension(saveFilePath) +
                                                UniqueDateString() +
                                                Path.GetExtension(saveFilePath));

            logger.Report("## download start ##");
            logger.Report(Path.GetFileName(saveFilePath)  + " as " + tmpFilePath);

            Task task = Task.Run(() =>
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                using (FileStream fs = new FileStream(tmpFilePath, FileMode.Create))
                {
                    using (Stream st = res.GetResponseStream())
                    {
                        Byte[] buf = new Byte[1024];
                        int count = 0;
                        do
                        {
                            cycle += 1;
                            count = st.Read(buf, 0, buf.Length);
                            fs.Write(buf, 0, count);

                            dlsize += count;
                            // logger.Report(dlsize.ToString());
                            if (cycle % 1000 == 0)
                                logger.Report("file download.... " + dlsize / 1000 + "k Byte");

                        } while (count != 0);
                    }
                }
                res.Close();
            });

            try
            {
                task.Wait();

            } catch(Exception e)
            {
                message +=  "error, Download http request. " + Environment.NewLine + e.Message;
                logger.Report(message);
            
                return string.Empty;
            }

            // tmp -> save
            if (File.Exists(saveFilePath))
                FileUtils.FileBackupAddUniqDatetime(saveFilePath, ref message);
            File.Move(tmpFilePath, saveFilePath);

            return saveFilePath;
        }


        public enum DownloadResult
        {
            CompletedSuccessfuly,
            Cancelled,
            ErrorOccurred
        }
    }
}
