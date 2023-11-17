using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class FiltriCatalogoModel
    {
        public FiltriCatalogoModel() { 
            Sezioni = new List<Sezione>();
            SearchString = "";
            SelectedFilter = "";
        }
        public List<Sezione> Sezioni { get; set; }
        public string SearchString { get; set; }
        public string OrderBy { get; set; }
        public string SelectedTag { get; set; }
        public string SelectedFilter { get; set; }

    }

    public class Sezione
    {
        public Sezione()
        {
            Voci = new List<voce>();
        }
        public Sezione(string nome)
        {
            Voci = new List<voce>();
            NomeSezione = nome;
        }
        public string NomeSezione { get; set; }
        public List<voce> Voci { get; set; }
    }

    public enum OrderTypeEnum
	{
	    None,String,Number         
	}

    public class voce
    {
        public voce() {
            SottoVoci = new List<voce>();
            showCheckbox = true;
            OrderType = OrderTypeEnum.None;
        }
        public voce(string nome)
        {
            SottoVoci = new List<voce>();
            showCheckbox = true;
            NomeVoce = nome;
            OrderType = OrderTypeEnum.None;
        }
        public string NomeVoce { get; set; }
        public string RefField { get; set; }
        public Boolean Impostato { get; set; }
        public Boolean showCheckbox { get; set; }
        public List<voce> SottoVoci { get; set; }
        public Boolean IsTag { get; set; }
        public string StatoAttribute { get; set; }
        public OrderTypeEnum OrderType {get; set; }
        public string OrderTag { get; set; }
    }
}