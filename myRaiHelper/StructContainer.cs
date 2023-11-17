using System.Collections.Generic;

namespace myRaiHelper
{
    public struct voceMenu
    {
        public string Titolo;
        public string customView;
        public string nomeSezione;
        public string icon;
        public string codiceMy;
        public int ID;
        public int IdPadre;
        public bool attivo;
        public int? progressivo;
        public bool RichiedeGapp;
    }


    public struct sezioniMenu
    {
        public string Titolo;
        public string customView;
        public string nomeSezione;
        public string icon;
        public string codiceMy;
        public int ID;
        public bool attivo;
        public List<voceMenu> vociMenu;
        public int? progressivo;
        public bool DaEscludere;
        public bool RichiedeGapp;
    }
}
