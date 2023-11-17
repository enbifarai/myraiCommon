using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyRaiWindowsService1
{
    public enum EnumCategoriaEccezione
    {
        ApprovazioneEccezione,
        RifiutoEccezione,
        MarcaturaScaduta,
        MarcaturaUrgente,
        InsRichiesta,
        InsStorno

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


    public enum EnumParametriSistema
    {
        GiornoEsecuzioneBatchPDF,
        IntervalloBatchSecondi,
        IntervalloUrgentiScadute,
        EsecuzioneInvioEmail,
        MailApprovazioneFrom,
        MailApprovazioneSubject,
        MailRichiesteSubject,
        MailTemplateUrgentiScadute,
        MailTemplateScadenzario,
        AccountUtenteServizio,
        OreRichiesteUrgenti,
        UtentePerConvalida,

        MailTemplatePDFinFirma,
        RedirectEmailSuSviluppo
    }
    class Enum
    {

    }
}
