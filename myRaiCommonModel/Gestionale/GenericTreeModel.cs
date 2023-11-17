using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace myRaiCommonModel.Gestionale
{
    public class GenericTreeModel
    {
        public GenericTreeModel()
        {
            TreeItems = new List<TreeItem>();
        }
        public string DBorigine { get; set; }
        public string DataStruttura { get; set; }
        public List<TreeItem> TreeItems { get; set; }

        public DettaglioProcessoModel SelectedProcess { get; set; }

        public GestioneSistemiModel GestioneSistemi { get; set; }

    }
    public class TreeItem
    {
        public myRaiDataTalentia.XR_STR_TALBERO_GEN ItemAlbero { get; set; }
        public myRaiDataTalentia.XR_STR_PROCESSI ItemProcesso { get; set; }
    }


    public class GestioneSistemiModel
    {
        public List<myRaiDataTalentia.XR_STR_SISTEMI_IT> Sistemi { get; set; }
    }

    public class GestioneProcessoModel
    {
        public myRaiDataTalentia.XR_STR_PROCESSI ProcessoPadre { get; set; }
        public myRaiDataTalentia.XR_STR_PROCESSI ProcessoCorrente { get; set; }
        public List<myRaiDataTalentia.XR_STR_SISTEMI_IT> Sistemi { get; set; }
        public List<myRaiDataTalentia.XR_STR_SISTEMI_IT> SistemiCollegati { get; set; }
        public string SistemiCollegatiOld { get; set; }

        public List<myRaiDataTalentia.XR_STR_TSEZIONE> DirezioniOwner { get; set; }
        public List<myRaiDataTalentia.XR_STR_TSEZIONE> DirezioniOwnerCollegate { get; set; }
        public string DirezioniOwnerCollegateOld { get; set; }

        public List<myRaiDataTalentia.XR_STR_TSEZIONE> DirezioniCoinvolte { get; set; }
        public List<myRaiDataTalentia.XR_STR_TSEZIONE> DirezioniCoinvolteCollegate { get; set; }
        public string DirezioniCoinvolteCollegateOld { get; set; }

        public List<myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI> Allegati { get; set; }
    }
    public class DettaglioProcessoModel
    {
        public DettaglioProcessoModel()
        {
            SistemiCollegati = new List<myRaiDataTalentia.XR_STR_SISTEMI_IT>();
            DirezioniOwnerCollegati = new List<myRaiDataTalentia.XR_STR_TSEZIONE>();
            DirezioniCoinvolteCollegati = new List<myRaiDataTalentia.XR_STR_TSEZIONE>();
            Allegati = new List<myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI>();
        }
        public myRaiDataTalentia.XR_STR_PROCESSI Processo { get; set; }
        public bool IsStartingProcess { get; set; }
        public List<myRaiDataTalentia.XR_STR_SISTEMI_IT> SistemiCollegati { get; set; }
        public List<myRaiDataTalentia.XR_STR_TSEZIONE> DirezioniOwnerCollegati { get; set; }
        public List<myRaiDataTalentia.XR_STR_TSEZIONE> DirezioniCoinvolteCollegati { get; set; }
        public List<myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI> Allegati { get; set; }
    }
    public class SaveProcessoModel
    {
        public string nomeprocesso { get; set; }
        public string descrizioneprocesso { get; set; }
        public string codiceprocesso { get; set; }
        public int idprocessopadre { get; set; }
        public int idprocesso { get; set; }
        public string tipomodifica { get; set; }
        public int[] sistemaIT { get; set; }
        public string SistemiCollegatiOld { get; set; }

        public int[] dirOwner { get; set; }
        public string DirezioniOwnerCollegateOld { get; set; }

        public int[] dirCoinvolte { get; set; }
        public string DirezioniCoinvolteCollegateOld { get; set; }

       public Doc[] Documenti { get; set; }
      
    }
    public class Doc
    {
        public HttpPostedFileBase Filecontent { get; set; }
        public string Desc { get; set; }
        public string Action { get; set; }
        public string Nome { get; set; }
        public TipoDocumento Tipo { get; set; }
        public int id { get; set; }
    }
    public enum TipoDocumento
    {
        Processo,
        Master,
        FlowChart,
        ProcessMap,
        ProcessIdentity,
        Allegati,
        Procedura,
        IstruzioniOperative
    }
    public class BoxAllegatoModel
    {
        public string tipo { get; set; }
        public myRaiDataTalentia.XR_STR_ALLEGATI_PROCESSI Allegato { get; set; }
        public int progressivo { get; set; }
    }
}
