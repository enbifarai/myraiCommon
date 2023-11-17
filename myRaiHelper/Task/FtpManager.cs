using FluentFTP;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using WinSCP;
using static myRaiHelper.AccessoFileHelper;

namespace myRaiHelper.Task
{
    public class FtpConnectionParam
    {
        public FtpConnectionParam()
        {
            EncryptionMode = FtpEncryptionMode.None;
            SslProtocols = SslProtocols.Default;
        }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public FtpKeyFile KeyFile { get; set; }
        public FtpEncryptionMode EncryptionMode { get; set; }
        public SslProtocols SslProtocols { get; set; }
    }
    public class FtpKeyFile
    {
        public string KeyFileName { get; set; }
        public string KeyFilePath { get; set; }
        public byte[] KeyFileByte { get; set; }
        public string KeyPassPhrase { get; set; }
        public HrisParam? KeyHrisParam { get; set; }
    }
    public class FtpManager : BaseTask
    {
        public FtpManager()
        {
            LocalExistsMode = FtpLocalExists.Overwrite;
            RemoteExistsMode = FtpRemoteExists.Overwrite;
        }
        public enum OperationEnum
        {
            None,
            ReadFile,
            WriteFile
        }
        public enum FtpType
        {
            Ftp,
            Ftps,
            Sftp
        }
        public OperationEnum Operation { get; set; }
        public FtpType ConnectionType { get; set; }
        public FtpConnectionParam ConnectionParam { get; set; }
        public FtpLocalExists LocalExistsMode { get; set; }
        public FtpRemoteExists RemoteExistsMode { get; set; }
        public string RemoteDir { get; set; }
        public string RemoteName { get; set; }
        public string LocalDir { get; set; }
        public string LocalName { get; set; }
        private string _remoteFile
        {
            get
            {
                string tmp = String.Format(RemoteName, DateTime.Now);
                if (!String.IsNullOrWhiteSpace(RemoteDir) && !tmp.StartsWith(RemoteDir))
                    tmp = RemoteDir + "/" + tmp;
                return tmp;
            }
        }
        private string _localFile
        {
            get
            {
                string tmp = String.Format(LocalName, DateTime.Now);
                if (!String.IsNullOrWhiteSpace(LocalDir) && !tmp.StartsWith(LocalDir))
                    tmp = Path.Combine(LocalDir, tmp);
                return tmp;
            }
        }

        public override bool CheckParam(out string errore)
        {
            bool result = true;
            errore = null;

            if (Operation == OperationEnum.None)
            {
                result = false;
                errore += "Operazione non indicata\r\n";
            }

            if (Impersonate && !ImpersonateHrisParam.HasValue)
            {
                result = false;
                errore += "Parametro Impersonate non indicato";
            }
            else if (Impersonate)
            {
                string[] credenziali = HrisHelper.GetParametri<string>(ImpersonateHrisParam.Value);
                if (credenziali == null)
                {
                    result = false;
                    errore += "Credenziali Impersonate non trovate";
                }
            }

            if (String.IsNullOrWhiteSpace(RemoteDir) && String.IsNullOrWhiteSpace(RemoteName))
            {
                result = false;
                errore += "Directory/File remoto non indicati";
            }

            return result;
        }

        public override bool Esegui(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            try
            {
                string tmpOutput = "";
                string tmpErrore = "";

                if (Impersonate)
                {
                    string[] credenziali = HrisHelper.GetParametri<string>(ImpersonateHrisParam.Value);
                    ImpersonationHelper.Impersonate(credenziali[2], credenziali[0], credenziali[1], delegate
                    {
                        result = InternalEsegui(out tmpOutput, out tmpErrore);
                    });
                }
                else
                {
                    result = InternalEsegui(out tmpOutput, out tmpErrore);
                }

                output = tmpOutput;
                errore = tmpErrore;
            }
            catch (Exception ex)
            {
                errore = ex.Message;
            }



            return result;
        }

        private bool InternalEsegui(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            switch (ConnectionType)
            {
                case FtpType.Ftp:
                case FtpType.Ftps:
                    result = EseguiFtp(out output, out errore);
                    break;
                case FtpType.Sftp:
                    result = EseguiSftp(out output, out errore);
                    break;
                default:
                    break;
            }
            return result;
        }

        private bool EseguiSftpRenci(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            try
            {
                PrivateKeyFile keyFile = null;
                if (!String.IsNullOrWhiteSpace(ConnectionParam.KeyFile.KeyFilePath))
                    keyFile = new PrivateKeyFile(ConnectionParam.KeyFile.KeyFilePath, ConnectionParam.KeyFile.KeyPassPhrase);
                else if (ConnectionParam.KeyFile.KeyFileByte != null)
                    keyFile = new PrivateKeyFile(new MemoryStream(ConnectionParam.KeyFile.KeyFileByte), ConnectionParam.KeyFile.KeyPassPhrase);
                else if (ConnectionParam.KeyFile.KeyHrisParam.HasValue)
                {
                    var hrisParam = HrisHelper.GetParametro(ConnectionParam.KeyFile.KeyHrisParam.Value);
                    keyFile = new PrivateKeyFile(new MemoryStream(hrisParam.COD_CONTENT), hrisParam.COD_VALUE1);
                }

                var keyFiles = new[] { keyFile };

                var authMethod = new List<AuthenticationMethod>();
                authMethod.Add(new PrivateKeyAuthenticationMethod(ConnectionParam.User, keyFiles));
                ConnectionInfo conn = new ConnectionInfo(ConnectionParam.Host, ConnectionParam.Port.Value, ConnectionParam.User, authMethod.ToArray());

                using (var client = new SftpClient(conn))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        switch (Operation)
                        {
                            case OperationEnum.None:
                                break;
                            case OperationEnum.ReadFile:
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    client.DownloadFile(_remoteFile, ms);
                                    if (ms != null)
                                    {
                                        string localDir = System.IO.Path.GetDirectoryName(_localFile);
                                        if (!Directory.Exists(localDir))
                                            Directory.CreateDirectory(localDir);

                                        using (FileStream file = new FileStream(_localFile, FileMode.Create, FileAccess.Write))
                                        {
                                            ms.WriteTo(file);
                                        }
                                        result = true;
                                    }
                                    else
                                        errore = "File remoto non trovato";
                                }
                                break;
                            case OperationEnum.WriteFile:
                                using (FileStream ms = new FileStream(_localFile, FileMode.Open))
                                {
                                    client.UploadFile(ms, _remoteFile, true, null);
                                    result = true;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        errore = "Impossibile stabilire la connessione.";
                    }
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message + " - " + ex.StackTrace;
            }

            return result;
        }

        private bool EseguiSftp(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            bool useTempKey = false;
            string tmpKeyRandom = null;


            try
            {
                // Imposta le opzioni di sessione
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = ConnectionParam.Host,
                    PortNumber = ConnectionParam.Port.Value,
                    UserName = ConnectionParam.User,
                    SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny
                };

                if (!String.IsNullOrWhiteSpace(ConnectionParam.KeyFile.KeyFilePath))
                {
                    sessionOptions.SshPrivateKeyPath = ConnectionParam.KeyFile.KeyFilePath;
                    sessionOptions.PrivateKeyPassphrase = ConnectionParam.KeyFile.KeyPassPhrase;
                }
                else if (!String.IsNullOrWhiteSpace(ConnectionParam.KeyFile.KeyFileName))
                {
                    sessionOptions.SshPrivateKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SFTP", ConnectionParam.KeyFile.KeyFileName);
                    sessionOptions.PrivateKeyPassphrase = ConnectionParam.KeyFile.KeyPassPhrase;
                }
                else if (ConnectionParam.KeyFile.KeyFileByte != null)
                {
                    useTempKey = true;
                    tmpKeyRandom = Path.Combine(CommonHelper.GetAppSettings("TempDirectory"), Guid.NewGuid().ToString()+".tmp");
                    File.WriteAllBytes(tmpKeyRandom, ConnectionParam.KeyFile.KeyFileByte);
                    sessionOptions.SshPrivateKeyPath = tmpKeyRandom;
                    sessionOptions.PrivateKeyPassphrase = ConnectionParam.KeyFile.KeyPassPhrase;
                }
                else if (ConnectionParam.KeyFile.KeyHrisParam.HasValue)
                {
                    var hrisParam = HrisHelper.GetParametro(ConnectionParam.KeyFile.KeyHrisParam.Value);
                    useTempKey = true;
                    tmpKeyRandom = Path.Combine(CommonHelper.GetAppSettings("TempDirectory"), Guid.NewGuid().ToString() + ".tmp");
                    File.WriteAllBytes(tmpKeyRandom, hrisParam.COD_CONTENT);
                    sessionOptions.SshPrivateKeyPath = tmpKeyRandom;
                    sessionOptions.PrivateKeyPassphrase = hrisParam.COD_VALUE1;
                }

                using (WinSCP.Session session = new WinSCP.Session())
                {
                    // Connetti
                    session.Open(sessionOptions);
                    if (session.Opened)
                    {
                        switch (Operation)
                        {
                            case OperationEnum.None:
                                break;
                            case OperationEnum.ReadFile:
                                {
                                    var ms = session.GetFile(_remoteFile);

                                    if (ms != null)
                                    {
                                        string localDir = System.IO.Path.GetDirectoryName(_localFile);
                                        if (!Directory.Exists(localDir))
                                            Directory.CreateDirectory(localDir);

                                        using (FileStream file = new FileStream(_localFile, FileMode.Create, FileAccess.Write))
                                        {
                                            ms.CopyTo(file);
                                        }
                                        result = true;
                                    }
                                    else
                                        errore = "File remoto non trovato";
                                }
                                break;
                            case OperationEnum.WriteFile:
                                using (FileStream ms = new FileStream(_localFile, FileMode.Open))
                                {
                                    session.PutFile(ms, _remoteFile, new TransferOptions()
                                    {
                                        OverwriteMode = OverwriteMode.Overwrite
                                    });
                                    result = true;
                                }
                                break;
                            default:
                                break;
                        }

                        session.Close();

                        if (useTempKey)
                            File.Delete(tmpKeyRandom);
                    }
                }
            }
            catch (Exception ex)
            {
                errore = ex.Message + " - " + ex.StackTrace;
            }

            return result;
        }

        public bool EseguiFtp(out string output, out string errore)
        {
            bool result = false;
            output = null;
            errore = null;

            try
            {
                FtpClient client = null;
                if (ConnectionParam.Port.HasValue)
                    client = new FtpClient(ConnectionParam.Host, ConnectionParam.Port.Value, ConnectionParam.User, ConnectionParam.Password);
                else
                    client = new FtpClient(ConnectionParam.Host, ConnectionParam.User, ConnectionParam.Password);

                client.EncryptionMode = ConnectionParam.EncryptionMode;
                client.SslProtocols = ConnectionParam.SslProtocols;
                client.ValidateCertificate += new FtpSslValidation(OnValidateCertificate);

                client.Connect();

                FtpStatus status = FtpStatus.Success;
                switch (Operation)
                {
                    case OperationEnum.None:
                        break;
                    case OperationEnum.ReadFile:
                        status = client.DownloadFile(_localFile, _remoteFile, LocalExistsMode);
                        break;
                    case OperationEnum.WriteFile:
                        status = client.UploadFile(_localFile, _remoteFile, RemoteExistsMode);
                        break;
                    default:
                        break;
                }

                switch (status)
                {
                    case FtpStatus.Failed:
                        errore = "Operazione non riuscita";
                        break;
                    case FtpStatus.Success:
                        result = true;
                        break;
                    case FtpStatus.Skipped:
                        errore = "File destinazione già esistente";
                        break;
                    default:
                        errore = "Errore non previsto";
                        break;
                }

                client.Disconnect();
            }
            catch (Exception ex)
            {
                errore = ex.Message + " - " + ex.StackTrace;
            }

            return result;
        }

        private void OnValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }
    }
}
