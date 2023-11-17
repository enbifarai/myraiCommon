using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
    public enum EnumCategoriaEccezione
    {
        ApprovazioneEccezione,
        RifiutoEccezione,
        MarcaturaScaduta,
        MarcaturaUrgente,
        InsRichiesta,
        InsStorno,
        EliminataGapp

    }
    public enum EnumTipoDestinatarioNotifica
    {
        ESS_DIP,
        ESS_LIV1,
        ESS_LIV2

    }
    public enum EnumTipoEventoNotifica
    {
        APPR,
        RIF,
        SCAD,
        CH,
        INSR,
        INSS,
        URG,
        ST_RESO,
        ST_RICH,
        ST_FIRM
    }
    public enum EnumTipoInvioNotifica
    {
        I, // ISTANTANEO
        G, // GIORNALIERO
        S, // SETTIMANALE
        M  // MENSILE

    }
    public enum EnumTipoDiStampa
    {
        ro,
        ss,
        sc
    }

    class Enum
    {

    }
}
