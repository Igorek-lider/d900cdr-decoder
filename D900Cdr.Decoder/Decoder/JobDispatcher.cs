using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace D900Cdr.Decoder
{
    public class JobDispatcher
    {
        private BackgroundWorker _worker;
        private IDecoderJob _job;
        private JobStatus _status;
        private BasicLogger _logger;

        public JobDispatcher()
            : this(null)
        {
        }

        public JobDispatcher(IDecoderJob job)
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(DoJob);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(JobCompleted);
            _worker.ProgressChanged += new ProgressChangedEventHandler(JobProgressChanged);
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _job = job;
            _status = new JobStatus();
            _logger = new BasicLogger();

            AssemblyVersionInfo vInfo = new AssemblyVersionInfo(typeof(JobBase));
            _logger.WriteLogMessage(String.Format("*** {0} {1}.{2} - (build {3})", vInfo.Title, vInfo.Version.Major, vInfo.Version.Minor, vInfo.LastBuildDate.ToString("yyMMdd-HHmm")), LogLevel.Info);
            _logger.WriteLogMessage(String.Format("*** Xml Schema Version {0}", D900Cdr.Schema.D900CdrDefinitionProvider.Instance.XmlVersion), LogLevel.Info);
        }

        private void JobProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.OnJobProgressChanged != null)
                this.OnJobProgressChanged(sender, null);
        }

        private void JobCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _status.IsCompleted = true;
            if (this.AfterJobCompleted != null)
                this.AfterJobCompleted(sender, null);
        }

        private void DoJob(object sender, DoWorkEventArgs e)
        {
            _logger.WriteLogMessage("+++ Start new job:", LogLevel.Info);

            _status.ResultCode = JobResultCode.FatalError;

            CdrDecoder decoder = new CdrDecoder();
            decoder.ElementDefinitionProvider.CurrentSchema = _job.DefinitionSchemaName;
            D900CdrElement record;

            RecordFormatter formatter = (_job.IsFormatterActive && (_job.FormatterSettings != null)) ? new RecordFormatter(_job.FormatterSettings) : null;
            Regex filterRegex = (_job.IsFilterActive && !String.IsNullOrEmpty(_job.FilterText)) ? new Regex(_job.FilterText, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline) : null;

            StreamWriter dstFile = new StreamWriter(_job.DestinationPath);
            if ((formatter != null) && _job.FormatterSettings.PrintColumnsHeader)
            {
                dstFile.WriteLine(_job.FormatterSettings.ColumnsHeader);
            }

            FileInfo[] cdrFiles;

            cdrFiles = new DirectoryInfo(Path.GetDirectoryName(_job.SourcePath)).GetFiles(Path.GetFileName(_job.SourcePath), SearchOption.TopDirectoryOnly);
            FileStream cdr;
            long cdrLength;
            long rem;
            string recText;

            _status.CdrFilesIn = cdrFiles.Length;
            _logger.WriteLogMessage(String.Format("Files to decode: {0}", _status.CdrFilesIn), LogLevel.Info, false);
            _logger.WriteLogMessage(String.Format("Schema: {0}", _job.DefinitionSchemaName), LogLevel.Info, false);
            _worker.ReportProgress(_status.Percent);
            foreach (FileInfo fi in cdrFiles)
            {
                cdr = new FileStream(fi.FullName, FileMode.Open);
                cdrLength = cdr.Length;

                if (_job.StartOffset > 0) cdr.Seek(_job.StartOffset, SeekOrigin.Begin);

                _status.RecordsOut = 0;
                _status.CurrentCdrFile = fi.Name;
                rem = 0;

                _logger.WriteLogMessage(String.Format("{0} ... ", fi.Name), LogLevel.Info);
                _worker.ReportProgress(_status.Percent);
                for (; ; )
                {
                    if (_status.RecordsOut == 0)
                    {
                        record = decoder.DecodeRecord(cdr, false);
                    }
                    else
                    {
                        record = decoder.DecodeRecord(cdr, true);
                    }
                    if (record == null)
                        break;

                    _status.RecordsOut++;
                    _status.RecordsOutTotal++;
                    _status.Percent = (int)Math.Ceiling((double)cdr.Position / cdrLength * 100);

                    recText = (formatter == null) ? String.Format("{0,8} > {1} {2}", record.Offset, _status.RecordsOut, record.ToString()) : formatter.FormatRecord(record);

                    if ((filterRegex == null) || (filterRegex.Match(recText).Success))
                        dstFile.WriteLine(recText);

                    Math.DivRem(_status.RecordsOut, 1000, out rem);
                    if (rem == 0) _worker.ReportProgress(_status.Percent);
                    if (_worker.CancellationPending)
                    {
                        break;
                    }
                }

                cdr.Close();

                _logger.AppendLogMessage(_status.RecordsOut.ToString());

                if (_worker.CancellationPending)
                {
                    break;
                }
                else
                {
                    _status.CdrFilesIn--;
                    _status.CdrFilesOut++;
                    _status.Percent = 100;
                    _worker.ReportProgress(_status.Percent);
                }
            }

            dstFile.Close();
            if (_worker.CancellationPending)
            {
                _status.ResultCode = JobResultCode.CanceledByUser;
                _logger.WriteLogMessage("+++ Process aborted by user.", LogLevel.Info);
            }
            else
            {
                _status.ResultCode = JobResultCode.AllOK;
                _logger.WriteLogMessage("+++ Decoding is successful done.", LogLevel.Info);
            }
        }

        public event EventHandler AfterJobCompleted;
        public event EventHandler OnJobProgressChanged;

        public IDecoderJob Job
        {
            get { return _job; }
            set { _job = value; }
        }

        public BasicLogger Logger
        {
            get { return _logger; }
        }

        public JobStatus Status
        {
            get { return _status; }
        }

        public void ExecuteJob()
        {
            _status.IsRunning = true;
            _status.StartTime = DateTime.Now;
            _worker.RunWorkerAsync();
        }

        public void CancelJob()
        {
            _worker.CancelAsync();
        }
    }
}
