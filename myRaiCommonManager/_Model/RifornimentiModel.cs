using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;


//FILE/CLASSE CANCELLABILE SE SI USA OMONIMA CLASSE IN DLL ESTERNA

namespace myRaiCommonModel
{
    public class SituazRifornimentiViewModel
    {
        public List<Rifornimento> RifornimentiAnno { get; set; }
        public RiepilogoRifornimentiAnno RiepilogoAnno { get; set; }
        public bool authEditSkCarb { get; set; }

        public SituazRifornimentiViewModel()
        {
            RifornimentiAnno = new List<Rifornimento>();
            RiepilogoAnno = new RiepilogoRifornimentiAnno();
        }
        public string MessaggioAssegnatario { get; set; }
    }

    public class SchedaCarburante
    {
        [Required]
        [Range(2018, 2100, ErrorMessage = "Errore: anno contabile di riferimento non valido")]
        public int Anno { get; set; }

        [Required(ErrorMessage = "Il campo tipo scheda carburante è obbligatorio")]
        [checkTipoSkCarburante(ErrorMessage = "La tipologia di scheda carburante non è valida")]
        public string TipoSkCarb { get; set; }

        [Required(ErrorMessage = "Il campo tipo carburante è obbligatorio")]
        [checkTipoCarburante(ErrorMessage = "La tipologia di carburante non è valida")]
        public string TipoCarb { get; set; }

        [Required(ErrorMessage = "Il campo targa è obbligatorio")]
        [StringLength(7, ErrorMessage = "Il campo targa non può contenere più di {1} caratteri")]
        public string TargaAssociata { get; set; }

        public List<Rifornimento> RifornimentiInseriti { get; set; }
        public string IdSkCarb { get; set; }
        public int idxEvidenziaRif { get; set; }
        public string ReturnMessage { get; set; }
        public string FileNameSkCarb { get; set; }
        public string FileContentSkCarb { get; set; }
        //public HttpPostedFileBase Allegato { get; set; }

        public SchedaCarburante()
        {
            RifornimentiInseriti = new List<Rifornimento>();
        }
    }

    public class RiepilogoRifornimentiAnno
    {
        public int Anno { get; set; }
        public decimal TotContabiliz { get; set; }
        public decimal TotFuelCard { get; set; }
        public decimal TotSkCarbItalia { get; set; }
        public decimal TotContabilizSkCarbItalia { get; set; }
        public decimal TotImportoContabilizSkCarbItalia { get; set; }
        public decimal TotSkCarbEstero { get; set; }
        public decimal TotContabilizSkCarbEstero { get; set; }
        public decimal TotImportoContabilizSkCarbEstero { get; set; }
        public DateTime InizioPeriodoInsertSkCarb { get; set; }
        public DateTime FinePeriodoInsertSkCarb { get; set; }
        public bool AuthInsertSkCarb { get; set; }
    }

    public class Rifornimento
    {
        public int? Id_Anno { get; set; }
        public string Id_Matricola { get; set; }
        public DateTime? Id_DataTransaz { get; set; }
        public int? Id_StatoLogico { get; set; }

        [Required(ErrorMessage = "Il campo data è obbligatorio")]
        [checkDate(ErrorMessage = "La data dev'essere scritta nel formato gg/mm/aaaa")]
        public DateTime? Data { get; set; }

        [Required(ErrorMessage = "Il campo orario è obbligatorio")]
        [checkTime(ErrorMessage = "L'orario dev'essere scritto nel formato hh:mm")]
        public TimeSpan? Orario { get; set; }

        [Required(ErrorMessage = "Il campo quantità è obbligatorio")]
        [Range(0.01, 99.99, ErrorMessage = "Il campo quantità dev'essere un valore compreso fra {1} e {2}")]
        public string Quantita { get; set; }

        [Required(ErrorMessage = "Il campo importo è obbligatorio")]
        [Range(0.01, 150.00, ErrorMessage = "Il campo quantità dev'essere un valore compreso fra {1} e {2}")]
        public string Importo { get; set; }

        [Required(ErrorMessage = "Il campo contaKm è obbligatorio")]
        [Range(1, 999999, ErrorMessage = "Il campo contaKm dev'essere un valore compreso fra {1} e {2}")]
        public string ContaKm { get; set; }

        [Required(ErrorMessage = "Il campo nazione è obbligatorio")]
        [checkNazione(ErrorMessage = "Il campo nazione non è valido")]
        public string Nazione { get; set; }

        public string Targa { get; set; }
        public string Carburante { get; set; }
        public string Dove { get; set; }
        public string NumScheda { get; set; }
        public string NumRicevuta { get; set; }
        public string StatoLavorazione { get; set; }
        public string DataContabilizzazione { get; set; }
        public bool AuthInsEditSkCarb { get; set; }
    }

    public static class RifornimentiConfig
    {
        public static string[] TipiNazione = new string[] { "Italia", "Estero" };

        public static string[] TipiCarburante = new string[] { "Benzina", "Gasolio" };

        public static string[] TipiSkCarburante = new string[] { "Auto Assegnata", "Auto Sostitutiva" };

        public enum TipiSkCarburanteEnum
        {
            [Display(Name = "Auto Assegnata")]
            NoSost,
            [Display(Name = "Auto Sostitutiva")]
            Sost
        }

        public static string[] EstensioniFilePermesse = new string[] { ".pdf" };

        public static string StatoForEditSkCarb = "Inserito";

        public static int BytesMaxSkCarbAllegato = 2000000;
    }

    public class checkTime : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            try
            {
                TimeSpan time = (TimeSpan)value;
                if (time != new TimeSpan(time.Hours, time.Minutes, 0))
                {
                    return false;
                }
            }
            catch (Exception) { return false; }
            return true;
        }
    }

    public class checkDate : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            try
            {
                DateTime date = (DateTime)value;
                if (date != new DateTime(date.Year, date.Month, date.Day))
                {
                    return false;
                }
            }
            catch (Exception) { return false; }
            return true;
        }
    }

    public class checkTipoCarburanteAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return RifornimentiConfig.TipiCarburante.Contains(value);
        }
    }

    public class checkTipoSkCarburanteAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return RifornimentiConfig.TipiSkCarburante.Contains(value);
        }
    }

    public class checkNazioneAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return RifornimentiConfig.TipiNazione.Contains(value);
        }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayAttributeFrom(this Enum enumValue, Type enumType)
        {
            string displayName = enumValue.ToString();
            System.Reflection.MemberInfo info = enumType.GetMember(enumValue.ToString()).First();

            if (info != null)
            {
                Type type = enumValue.GetType();
                FieldInfo fieldInfo = type.GetField(enumValue.ToString());
                DisplayAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];
                return attribs.Length > 0 ? attribs[0].Name : displayName;
            }

            return displayName;
        }
    }
}