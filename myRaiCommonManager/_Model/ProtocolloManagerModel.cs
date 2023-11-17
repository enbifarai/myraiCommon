using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonManager._Model
{
    [Serializable]
    public class BaseProtocolloResult
    {
        public bool Esito { get; set; }
        public string DescrizioneErrore { get; set; }
        public string Id_Documento { get; set; }
    }

    [Serializable]
    public class CreaProtocolloResult : BaseProtocolloResult
    {
        public CreaProtocolloServiceResult ServiceResult { get; set; }
    }

    [Serializable]
    public class CreaProtocolloServiceResult
    {
        public byte[] File { get; set; }
        public string Protocollo { get; set; }
        public string Id_Documento { get; set; }
    }

    [Serializable]
    public class ApplicaDocumentoAlProtocolloResult : BaseProtocolloResult
    {
    }
}
