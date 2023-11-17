using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TimbratureCore
{
    public class DayAnalysisFactory
    {
        public static Boolean DayAnalysisExists ( string e )
        {
            return Type.GetType( "TimbratureCore.DayAnalysis" + e.ToUpper( ) ) != null;
        }
        public static DayAnalysisBase GetDayAnalysisClass(string e, dayResponse day, bool QuadraturaGiornaliera, dayResponse dayRespPreviousDay = null, 
            bool SimulaSMAP1min = false,
            bool skipIpotesiNoCarenza=false,
            bool skipTaglioMaggiorazioniCoda=false
            )
        {
            Type t = Type.GetType( "TimbratureCore.DayAnalysis" + e.ToUpper( ) );
            if ( t != null )
            {
                var cl = ( DayAnalysisBase ) Activator.CreateInstance( t );
                cl.SkipIpotesiNoCarenza = skipIpotesiNoCarenza;
                cl.SkipTaglioMaggiorazioniCoda = skipTaglioMaggiorazioniCoda;
                cl.SimulaSMAP1Min = SimulaSMAP1min;
                cl.QuadraturaGiornaliera = QuadraturaGiornaliera;
                cl.dayRespPrevious = dayRespPreviousDay;
                cl.dayresp = day;
               
                cl.CodiceEccezione = e.ToUpper( );
          
                return cl;
            }
            else
                return null;
        }
    }


    public class DayAnalysisBase
    {
        protected virtual List<string> FasceOrarie{
            get
            {
                return new List<string>();
            }
        }
        //properties
        public Boolean QuadraturaGiornaliera { get; set; }

        public string EccezioniEliminateSeOverSogliaNotturno { get; set; }
        public dayResponse dayRespPrevious { get; set; }
        private dayResponse _dayresp;
        public dayResponse dayresp
        {

            get { return _dayresp; }

            set
            {
                _dayresp = value;
                if ( _dayresp != null )
                {
                    HaLavorato = GetHaLavorato( _dayresp );
                    if ( HaLavorato )
                    {
                        this.MinutiInizioOrario = Convert.ToInt32( _dayresp.orario.entrata_iniziale );
                        this.MinutiFineOrario = Convert.ToInt32( _dayresp.orario.uscita_iniziale );
                        this.MinutiCarenza = GetCarenza(_dayresp);
                        this.MinutiMaggiorPresenza = GetMP(_dayresp);

                        this.HaSMAP1Min = GetSMAP1Min( );

                        this.MinutiIntervalloMensa = _dayresp.orario.intervallo_mensa.ToMinutes( );
                        this.MinutiPrevistaPresenza = Convert.ToInt32( _dayresp.orario.prevista_presenza ) + MinutiIntervalloMensa;
                        GetSMAP( );
                        this.MinutiPrimoIngresso = GetMinutiPrimoIngresso( );
                        this.MinutiUltimaUscita = GetMinutiUltimaUscita( );
                        this.MinutiFSH = GetFSH( );
                        this.MinutiSEH = GetSEH( );
                        this.HaMR = GetMR( );
                        this.PrevistoRiposo = GetPrevistoRiposo( );
                        GetDettaglioMinuti( );
                        GetDettaglioMinuti_DopoURH( );
                        GetDettagliMinuti_DopoUMH( );

                        GetLacune( );

                        int FascePresenti = DettaglioMinutiGiornata.Where( x => x.tipo != null ).Select( x => x.tipo ).Distinct( ).Count( );

                        if ( FascePresenti <= 1 )
                            AggiornaMinutiFSH_SEH_1fascia( );
                        else
                            AggiornaMinutiFSH_SEH_MultipleFasce( );

                        if ( this.QuadraturaGiornaliera )
                        {
                            foreach ( var m in DettaglioMinutiGiornata.Where( a => a.ProgressivoMinuto < MinutiInizioOrario || a.ProgressivoMinuto > MinutiFineOrario ) )
                            {
                                m.tipo = "QG";
                            }
                        }
                        if ( this.HaSMAP1Min || this.SimulaSMAP1Min )
                        {
                            foreach ( var m in DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto > MinutiFineOrario ) )
                            {
                                m.tipo = null;
                            }
                        }

                        if (DettaglioMinutiGiornata.Last().ProgressivoMinuto - MinutiFineOrario < this.MinutiMaggiorPresenza)
                            this.MinutiMaggiorPresenza = DettaglioMinutiGiornata.Last().ProgressivoMinuto - MinutiFineOrario;

                        if (this.MinutiMaggiorPresenza < 0) this.MinutiMaggiorPresenza = 0;

                        //////////////////////////////////////////////
                        string par = myRaiCommonTasks.CommonTasks.GetParametro<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AbilitaTaglioMaggCoda);
                        if ( ! String.IsNullOrWhiteSpace(par) )
                        {
                            if (!this.SkipTaglioMaggiorazioniCoda)
                            {
                                KillMaggCoda(par);
                            }
                        }
                        
                        /////////////////////////////////////////////

                        this.Fasce = DettaglioMinutiGiornata.Where( x => x.tipo != null )
                                    .GroupBy( x => x.tipo )
                                    .Select( x => new Fascia { fascia = x.Key , minuti = x.Count( ) } ).ToList( );


                        if (!this.SkipIpotesiNoCarenza)
                        {
                            this.FasceIpotesiNoCarenza = GetFasceIpotesiNocarenza();
                        }
                    }
                }
            }
        }


        private bool WouldChange()
        {

            this.Fasce = DettaglioMinutiGiornata.Where(x => x.tipo != null)
                                   .GroupBy(x => x.tipo)
                                   .Select(x => new Fascia { fascia = x.Key, minuti = x.Count() }).ToList();
            var real= this.GetEccezioneQuantita();

            this.Fasce = DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto<=MinutiFineOrario && x.tipo != null)
                                  .GroupBy(x => x.tipo)
                                  .Select(x => new Fascia { fascia = x.Key, minuti = x.Count() }).ToList();
            var noMP = this.GetEccezioneQuantita();

            return noMP.QuantitaMinuti < real.QuantitaMinuti;
        }

        private void KillMaggCoda(string tipoCar)
        {
            if (this.MinutiCarenza > 0 && this.MinutiMaggiorPresenza > 0)
            {
                int minMinuto = DettaglioMinutiGiornata.Select(x => x.ProgressivoMinuto).FirstOrDefault();
                int maxMinuto = DettaglioMinutiGiornata.Select(x => x.ProgressivoMinuto).LastOrDefault();

                if (minMinuto > 0 && maxMinuto > 0)
                {
                    if (this.FasceOrarie.Contains("6_21") || WouldChange())
                    {
                        int PrimoMinutoCoperto = DettaglioMinutiGiornata.Where(x => x.tipo != null).Select(x => x.ProgressivoMinuto).FirstOrDefault();
                        if (PrimoMinutoCoperto > 0)
                        {
                            DettaglioMinutiGiornata = Filler(DettaglioMinutiGiornata, minMinuto, maxMinuto);

                            this.pointerMaggiorPresenza = 0;
                            this.pointerCarenza = 0;

                            CopriConEccezioniQMP();

                            DettaglioMinutiGiornata = TiraMinutiTesta(DettaglioMinutiGiornata, minMinuto, maxMinuto, PrimoMinutoCoperto, tipoCar);
                            if (this.pointerMaggiorPresenza < this.MinutiMaggiorPresenza)
                            {
                                DettaglioMinutiGiornata = TiraMinutiMezzo(DettaglioMinutiGiornata, minMinuto, maxMinuto, tipoCar);
                            }
                        }
                    }
                }
            }
        }



        private void CopriConEccezioniQMP()
        {
            var ListQ = dayresp.eccezioni.Where(x => x.cod.Trim().EndsWith("Q") || x.cod.Trim()=="FOH" || x.cod.Trim() == "FFH" || x.cod.Trim() == "PVH").ToList();
            if (ListQ.Any())
            {
                foreach (var ecc in ListQ)
                {
                    int minStart = ecc.dalle.ToMinutes();
                    int minEnd = ecc.alle.ToMinutes();
                    if (minStart == 0 || minEnd == 0)
                    {
                        continue;
                    }
                    for (int i = minStart; i < minEnd; i++)
                    {
                        var m = DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == i).FirstOrDefault();
                        if (m != null)
                            m.Copertura = ecc.cod.Trim();
                    }
                    }
                }
            int Cop = Convert.ToInt32(dayresp.orario.prevista_presenza) / 2;
            var ListM = dayresp.eccezioni.Where(x => x.cod.Trim().EndsWith("M")).ToList();
            if (ListM.Any())
            {
                var ecc = ListM.First();
                for (int i = 1; i <= Cop; i++)
                {
                    var m = DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == this.MinutiInizioOrario + i).FirstOrDefault();
                    if (m != null)
                        m.Copertura = ecc.cod.Trim();
                }
            }

            var ListP = dayresp.eccezioni.Where(x => x.cod.Trim().EndsWith("P")).ToList();
            if (ListP.Any())
            {
                var ecc = ListP.First();
                for (int i = 1; i <= Cop; i++)
                {
                    var m = DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == this.MinutiFineOrario - i).FirstOrDefault();
                    if (m != null && String.IsNullOrWhiteSpace(m.Copertura))
                        m.Copertura = ecc.cod.Trim();
                }
            }
        }

        private List<DettaglioMinuti> TiraMinutiMezzo(List<DettaglioMinuti> dettaglioMinutiGiornata, int minMinuto, int maxMinuto,string tipoCar)
        {
            while (true)
            {
                var buco = dettaglioMinutiGiornata.Where(x => x.tipo == null && x.Copertura == null).Skip(1).FirstOrDefault();
                if (buco != null)
                {
                    if (this.pointerMaggiorPresenza >= MinutiMaggiorPresenza)
                        break;
                    else
                        this.pointerMaggiorPresenza++;

                    if (tipoCar == "G")
                    {
                        if (this.pointerCarenza >= MinutiCarenza)
                            break;

                        this.pointerCarenza++;
                    }

                    for (int minuto = buco.ProgressivoMinuto; minuto <= maxMinuto; minuto++)
                    {
                        var nextMinuto = dettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == buco.ProgressivoMinuto + 1).FirstOrDefault();
                        if (nextMinuto != null)
                            buco.Copertura = nextMinuto.Copertura;

                        buco.tipo = GetSegmento(buco.ProgressivoMinuto);
                    }

                    dettaglioMinutiGiornata.RemoveAll(x => x.ProgressivoMinuto == dettaglioMinutiGiornata.Last().ProgressivoMinuto);
                    var d = dettaglioMinutiGiornata.LastOrDefault();
                    if (d != null)
                        maxMinuto = d.ProgressivoMinuto;
                }
                else
                    break;
            }

            return dettaglioMinutiGiornata;
        }
        private List<DettaglioMinuti> TiraMinutiTesta(List<DettaglioMinuti> dettaglioMinutiGiornata, int minMinuto, int maxMinuto, int PrimoMinutoCoperto, string tipoCar)
        {

            while (true)
            {
                var buco = dettaglioMinutiGiornata.Where(x => x.tipo == null && x.Copertura == null).Skip(1).FirstOrDefault();
                if (buco != null && buco.ProgressivoMinuto >= PrimoMinutoCoperto) break;

                if (buco != null)
                {
                    if (this.pointerMaggiorPresenza >= MinutiMaggiorPresenza)
                        break;
                    else
                        this.pointerMaggiorPresenza++;

                    if (tipoCar == "G")
                    {
                        if (this.pointerCarenza >= MinutiCarenza)
                            break;

                        this.pointerCarenza++;
                    }

                    for (int minuto = buco.ProgressivoMinuto; minuto <= maxMinuto; minuto++)
                    {
                        var nextMinuto = dettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == buco.ProgressivoMinuto + 1).FirstOrDefault();
                        if (nextMinuto !=null)
                            buco.Copertura = nextMinuto.Copertura;

                        buco.tipo = GetSegmento(buco.ProgressivoMinuto);
                    }

                    dettaglioMinutiGiornata.RemoveAll(x => x.ProgressivoMinuto == dettaglioMinutiGiornata.Last().ProgressivoMinuto);
                }
                else
                    break;
            }
            return dettaglioMinutiGiornata;
        }


        private List<DettaglioMinuti> Filler(List<DettaglioMinuti> dettaglioMinutiGiornata, int minMinuto, int maxMinuto)
        {
            for (int i = minMinuto; i <= maxMinuto; i++)
            {
                if (dettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == i).FirstOrDefault() == null)
                {
                    dettaglioMinutiGiornata.Add(new DettaglioMinuti() { ProgressivoMinuto = i, HHMM = i.ToHHMM(), tipo = null, Copertura = null });
                }
            }
            dettaglioMinutiGiornata = dettaglioMinutiGiornata.OrderBy(x => x.ProgressivoMinuto).ToList();
            return dettaglioMinutiGiornata;
        }
        private List<Fascia> GetFasceIpotesiNocarenza()
        {
            if (this.MinutiCarenza > 0)
            {
                for (int i = this.MinutiInizioOrario; i <= this.MinutiFineOrario; i++)
                {
                    var minuto = DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == i).FirstOrDefault();
                    if (minuto != null && String.IsNullOrWhiteSpace(minuto.tipo))
                    {
                        minuto.tipo = GetSegmento(minuto.ProgressivoMinuto);
                        minuto.Copertura = "NOCAR";
                    }
                }
        }

            return DettaglioMinutiGiornata.Where(x => x.tipo != null)
                                    .GroupBy(x => x.tipo)
                                    .Select(x => new Fascia { fascia = x.Key, minuti = x.Count() }).ToList();

        }
        private int GetMP(dayResponse dayresp)
        {
            return dayresp.giornata.maggiorPresenza.ToMinutes();
        }
        private bool GetSMAP1Min ( )
        {
            if ( dayresp.eccezioni != null )
            {
                var smap = dayresp.eccezioni.Where( x => x.cod.Trim( ) == "SMAP" ).ToList( );
                if ( smap.Count > 0 )
                {
                    if ( smap.Any( x => x.dalle != null && x.dalle.Trim( ) == this.MinutiFineOrario.ToHHMM( ) && x.alle != null && x.alle.Trim( ) == ( this.MinutiFineOrario + 1 ).ToHHMM( ) ) )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private int GetCarenza(dayResponse dayresp)
        {
            string car = dayresp.eccezioni.Where(x => x.cod != null && x.cod.Trim() == "CAR").Select(x => x.qta).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(car))
                return 0;
            else
                return car.ToMinutes();
        }
        private bool GetHaLavorato ( dayResponse dayresp )
        {
            if (dayresp == null) return false;
            return (
                    ( dayresp.timbrature != null && dayresp.timbrature.Any( ) )
                    ||
                    (dayresp.eccezioni != null && dayresp.eccezioni.Any(x => x.cod.StartsWith("FS") || x.cod.StartsWith("SW")
                    || x.cod.StartsWith("SE") || x.cod.StartsWith("FF")))
                   );


        }

        public int MinutiInizioOrario { get; set; }
        public int MinutiFineOrario { get; set; }
        public int MinutiIntervalloMensa { get; set; }
        public int MinutiPrevistaPresenza { get; set; }

        public List<SMAP> SmapPresenti { get; set; }

        public int MinutiCarenza { get; set; }
        public int MinutiMaggiorPresenza { get; set; }

        public int MinutiPrimoIngresso { get; set; }
        public int MinutiUltimaUscita { get; set; }
        public int MinutiFSH { get; set; }
        public string DalleFSH { get; set; }
        public string AlleFSH { get; set; }

        public int MinutiSEH { get; set; }
        public string DalleSEH { get; set; }
        public string AlleSEH { get; set; }


        public int MinutiUMH { get; set; }
        public string DalleUMH { get; set; }
        public string AlleUMH { get; set; }

        public bool HaUMH { get; set; }
        public bool HaFS { get; set; }
        public bool HaSE { get; set; }
        public bool HaMR { get; set; }
        public List<DettaglioMinuti> DettaglioMinutiGiornata { get; set; }
        public int MinutiURH { get; private set; }
        public List<Lacuna> Lacune { get; set; }

        public List<Fascia> Fasce { get; set; }

        public List<Fascia> FasceIpotesiNoCarenza { get; set; }

        public string CodiceEccezione { get; set; }
        public bool PrevistoRiposo { get; set; }
        public bool HaLavorato { get; set; }

        public bool HaSMAP1Min { get; set; }

        public bool SimulaSMAP1Min { get; set; }
        public bool SkipIpotesiNoCarenza { get; set; }
        public bool SkipTaglioMaggiorazioniCoda { get; set; }
        public int pointerMaggiorPresenza { get; private set; }
        public int pointerCarenza { get; private set; }

        public virtual EccezioneQuantita GetEccezioneQuantita ( )
        {
            return null;
        }

        public virtual EccezioneQuantita GetEccezioneQuantitaIpotesiNocarenza()
        {
            if (this.FasceIpotesiNoCarenza != null)
                    this.Fasce = this.FasceIpotesiNoCarenza;

            return this.GetEccezioneQuantita();
        }

        //metodi
        private bool GetPrevistoRiposo ( )
        {
            return ( !string.IsNullOrWhiteSpace( dayresp.giornata.orarioTeorico ) && dayresp.giornata.orarioTeorico.StartsWith( "9" ) && !string.IsNullOrWhiteSpace( dayresp.giornata.orarioReale ) && !dayresp.giornata.orarioReale.StartsWith( "9" ) );
        }
        private bool GetMR ( )
        {
            return ( dayresp.eccezioni != null && dayresp.eccezioni.Any( x => x.cod.Trim( ) == "MR" ) );
        }
        public string GetSegmento ( int minuto )
        {
            if (minuto >= 0 && minuto <= 6 * 60) return "24_6";
            else if (minuto > 6 * 60 && minuto <= 21 * 60) return "6_21";
            else if (minuto > 21 * 60 && minuto <= 24 * 60) return "21_24";
            else return "24_6";
        }
        public void GetSMAP ( )
        {
            if ( dayresp.eccezioni != null )
            {
                var smap = dayresp.eccezioni.Where( x => x.cod.Trim( ) == "SMAP" ).ToList( );
                if ( smap.Count > 0 )
                {
                    this.SmapPresenti = new List<SMAP>( );
                    foreach ( var s in smap )
                    {
                        var newsmap = new SMAP
                        {
                            dalle = s.dalle ,
                            alle = s.alle ,
                            minuti = s.alle.ToMinutes( ) - s.dalle.ToMinutes( )
                        };
                        var t = dayresp.timbrature.FirstOrDefault( );
                        if ( t != null )
                            newsmap.coda = !( s.alle.ToMinutes( ) <= t.entrata.orario.ToMinutes( ) );

                        this.SmapPresenti.Add( newsmap );
                    }
                }
            }
        }
        public int GetMinutiPrimoIngresso ( )
        {
            if (dayresp.timbrature == null || !dayresp.timbrature.Any()) return 0;
            var t = dayresp.timbrature.First( );
            if (t.entrata == null || String.IsNullOrWhiteSpace(t.entrata.orario)) return 0;
            return t.entrata.orario.ToMinutes( );
        }
        public int GetMinutiUltimaUscita ( )
        {
            if (dayresp.timbrature == null || !dayresp.timbrature.Any()) return 0;
            var t = dayresp.timbrature.Last( );
            if (t.uscita == null || String.IsNullOrWhiteSpace(t.uscita.orario)) return 0;
            return t.uscita.orario.ToMinutes( );
        }
        public int GetFSH ( )
        {
            if ( dayresp.eccezioni != null )
            {
                var fs = dayresp.eccezioni.Where( x => x.cod.Trim( ) == "FS" ).FirstOrDefault( );
                if ( fs != null )
                {
                    this.HaFS = true;
                    return MinutiPrevistaPresenza;
                }

                var fsh = dayresp.eccezioni.Where( x => x.cod.Trim( ) == "FSH" ).FirstOrDefault( );
                if ( fsh != null )
                {
                    if (!String.IsNullOrWhiteSpace(fsh.dalle)) this.DalleFSH = fsh.dalle.Trim();
                    if (!String.IsNullOrWhiteSpace(fsh.alle)) this.AlleFSH = fsh.alle.Trim();

                    return fsh.qta.ToMinutes( );
                }
            }
            return 0;
        }
        public int GetSEH ( )
        {
            var se = dayresp.eccezioni.Where(x => x.cod.Trim() == "SE" || x.cod.Trim() == "SWH").FirstOrDefault();
            if ( se != null )
            {
                this.HaSE = true;
                return MinutiPrevistaPresenza;
            }

            if ( dayresp.eccezioni != null )
            {
                var seh = dayresp.eccezioni.Where( x => x.cod.Trim( ) == "SEH" ).FirstOrDefault( );
                if ( seh != null )
                {
                    if (!String.IsNullOrWhiteSpace(seh.dalle)) this.DalleSEH = seh.dalle.Trim();
                    if (!String.IsNullOrWhiteSpace(seh.alle)) this.AlleSEH = seh.alle.Trim();
                    return seh.qta.ToMinutes( );
                }
            }
            return 0;
        }
        public void GetDettaglioMinuti ( )
        {
            List<DettaglioMinuti> L = new List<DettaglioMinuti>( );
            for ( int i = MinutiInizioOrario ; i <= MinutiInizioOrario + MinutiPrevistaPresenza ; i++ )
            {
                if ( this.HaFS )
                {
                    L.Add( new DettaglioMinuti( )
                    {
                        ProgressivoMinuto = i ,
                        HHMM = i.ToHHMM( ) ,
                        Copertura = ( i > MinutiInizioOrario ? "FS" : null ) ,
                        tipo = ( i > MinutiInizioOrario ? GetSegmento( i ) : null )
                    } );
                }
                else if ( this.HaSE )
                {
                    L.Add( new DettaglioMinuti( )
                    {
                        ProgressivoMinuto = i ,
                        HHMM = i.ToHHMM( ) ,
                        Copertura = ( i > MinutiInizioOrario ? "SE" : null ) ,
                        tipo = ( i > MinutiInizioOrario ? GetSegmento( i ) : null )
                    } );
                }
                else
                    L.Add( new DettaglioMinuti( )
                    {
                        ProgressivoMinuto = i ,
                        HHMM = i.ToHHMM( ) ,
                        tipo = null
                    } );
            }
            if ( this.SmapPresenti != null && this.SmapPresenti.Count > 0 )
            {
                foreach ( var smap in SmapPresenti )
                {
                    for ( int i = smap.dalle.ToMinutes( ) ; i <= smap.alle.ToMinutes( ) ; i++ )
                    {
                        if ( !L.Any( x => x.ProgressivoMinuto == i ) )
                        {
                            L.Add( new DettaglioMinuti( )
                            {
                                ProgressivoMinuto = i ,
                                HHMM = i.ToHHMM( ) ,
                                Copertura = "SMAP" ,
                                tipo = GetSegmento( i )
                            } );
                        }
                    }
                }
            }
            foreach ( var timb in dayresp.timbrature )
            {
                if (timb.entrata == null || String.IsNullOrWhiteSpace(timb.entrata.orario)) continue;
                if (timb.uscita == null || String.IsNullOrWhiteSpace(timb.uscita.orario)) continue;
                int MinutiFrom = timb.entrata.orario.ToMinutes( ) + 1;
                int MinutiTo = timb.uscita.orario.ToMinutes( );
                for ( int i = MinutiFrom ; i <= MinutiTo ; i++ )
                {
                    if (i <= MinutiInizioOrario) continue;
                    var item = L.Where( x => x.ProgressivoMinuto == i ).FirstOrDefault( );
                    if ( item != null )
                    {
                        item.tipo = GetSegmento( i );
                        item.Copertura = "PRES";
                    }
                    else
                    {
                        L.Add( new DettaglioMinuti( ) { ProgressivoMinuto = i , HHMM = i.ToHHMM( ) , tipo = GetSegmento( i ) , Copertura = "PRES" } );
                    }
                }
            }
            this.DettaglioMinutiGiornata = L;
        }
        public void GetDettagliMinuti_DopoUMH ( )
        {
            if ( dayresp.eccezioni != null )
            {
                var umh = dayresp.eccezioni.Where( x => x.cod.Trim( ) == "UMH" ).FirstOrDefault( );
                if (umh == null) return;
                this.HaUMH = true;
                this.DalleUMH = umh.dalle;
                this.AlleUMH = umh.alle;

                int umhDalle = umh.dalle.ToMinutes( );
                int umhAlle = umh.alle.ToMinutes( );
                this.MinutiUMH = umh.qta.ToMinutes( );

                this.MinutiIntervalloMensa -= this.MinutiUMH;
                if (this.MinutiIntervalloMensa < 0) this.MinutiIntervalloMensa = 0;

                for ( int i = umhDalle + 1 ; i <= umhAlle ; i++ )
                {
                    var minute = this.DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto == i ).FirstOrDefault( );
                    if ( minute != null )
                    {
                       // if (umh.cod.Trim()=="UME") minute.tipo = GetSegmento(i);
                        minute.Copertura = "UMH " + ( i - umhDalle ).ToString( ) + "/" + this.MinutiUMH;
                    }
                }
            }

        }
   
        public void GetDettaglioMinuti_DopoURH ( )
        {
            if ( dayresp.eccezioni != null )
            {
                var urh = dayresp.eccezioni.Where(x => x.cod.Trim() == "URH" || x.cod.Trim() == "UME").FirstOrDefault();
                if ( urh == null )
                    return;

                this.MinutiURH = urh.qta.ToMinutes( );

                if ( this.MinutiURH == 0 )
                    return;

                int urhDalle = urh.dalle.ToMinutes( );
                int urhAlle = urh.alle.ToMinutes( );
                if ( urhAlle == 0 || urhDalle == 0 )
                {
                    for ( int i = 1 ; i <= this.MinutiURH ; i++ )
                    {
                        var minute = this.DettaglioMinutiGiornata.Where( x => x.Copertura == null ).OrderBy( x => x.ProgressivoMinuto ).FirstOrDefault( );
                        if ( minute != null )
                        {
                            // if (minute.ProgressivoMinuto <= MinutiUltimaUscita)
                            minute.tipo = GetSegmento( minute.ProgressivoMinuto );
                            minute.Copertura = "URH " + i.ToString( ) + "/" + this.MinutiURH;
                        }
                    }
                    return;
                }

                for ( int i = urhDalle + 1 ; i <= urhAlle ; i++ )
                {
                    var minute = this.DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto == i ).FirstOrDefault( );
                    if ( minute != null )
                    {
                        // if (minute.ProgressivoMinuto <= MinutiUltimaUscita)
                        minute.tipo = GetSegmento( minute.ProgressivoMinuto );
                        minute.Copertura = "URH " + ( i - urhDalle ).ToString( ) + "/" + this.MinutiURH;
                    }
                }
            }
        }
        public void GetLacune ( )
        {
            List<Lacuna> L = new List<Lacuna>( );
            Lacuna lac = null;
            foreach ( var d in this.DettaglioMinutiGiornata )
            {
                if ( d.tipo == null )
                {
                    if (lac == null) lac = new Lacuna() { dalleMin = d.ProgressivoMinuto, dalleHHMM = d.ProgressivoMinuto.ToHHMM() };
                }
                if ( d.tipo != null )
                {
                    if ( lac != null )
                    {
                        lac.alleMin = d.ProgressivoMinuto - 1;
                        lac.alleHHMM= (d.ProgressivoMinuto - 1).ToHHMM();
                        lac.durata = d.ProgressivoMinuto - lac.dalleMin;

                        if (lac.durata == 1 && d.ProgressivoMinuto-1 == MinutiInizioOrario && d.ProgressivoMinuto > MinutiPrimoIngresso)
                        {

                        }
                        else
                        {
                        L.Add( new Lacuna( )
                        {
                            dalleMin = lac.dalleMin ,
                            dalleHHMM = lac.dalleHHMM ,
                            alleMin = d.ProgressivoMinuto - 1 ,
                            alleHHMM = ( d.ProgressivoMinuto - 1 ).ToHHMM( ) ,
                            durata = d.ProgressivoMinuto - lac.dalleMin
                        } );
                        }
                       
                        lac = null;
                    }
                }
            }
            this.Lacune = L;
        }
        public void AggiornaMinutiFSH_SEH_1fascia ( )
        {
            List<DettaglioMinuti> mins = new List<DettaglioMinuti>( );
            if ( this.MinutiFSH > 0 )
            {

                if ( !String.IsNullOrWhiteSpace( this.DalleFSH ) && !String.IsNullOrWhiteSpace( this.AlleFSH ) )
                {
                    int fm = this.DalleFSH.ToMinutes( );
                    int to = this.AlleFSH.ToMinutes( );
                    if ( fm > 0 || to > 0 )
                        mins = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto > fm && x.ProgressivoMinuto <= to ).ToList( );
                    else
                        mins = DettaglioMinutiGiornata.Where( x => x.tipo == null && x.ProgressivoMinuto > MinutiInizioOrario ).Take( MinutiFSH ).ToList( );
                }
                else
                    mins = DettaglioMinutiGiornata.Where( x => x.tipo == null && x.ProgressivoMinuto > MinutiInizioOrario ).Take( MinutiFSH ).ToList( );

                int co = 0;
                foreach ( var min in mins )
                {
                    co++;
                    min.tipo = GetSegmento( min.ProgressivoMinuto );
                    min.Copertura = "FSH " + co + "/" + MinutiFSH;
                }
            }
            if ( MinutiSEH > 0 )
            {
                int co = 0;
                if ( !String.IsNullOrWhiteSpace( this.DalleSEH ) && !String.IsNullOrWhiteSpace( this.AlleSEH ) )
                {
                    int fm = this.DalleSEH.ToMinutes( );
                    int to = this.AlleSEH.ToMinutes( );
                    if ( fm > 0 || to > 0 )
                        mins = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto > fm && x.ProgressivoMinuto <= to ).ToList( );
                    else
                        mins = DettaglioMinutiGiornata.Where( x => x.tipo == null && x.ProgressivoMinuto > MinutiInizioOrario ).Take( MinutiSEH ).ToList( );
                }
                else
                    mins = DettaglioMinutiGiornata.Where( x => x.tipo == null && x.ProgressivoMinuto > MinutiInizioOrario ).Take( MinutiSEH ).ToList( );

                foreach ( var min in mins )
                {
                    co++;
                    min.tipo = GetSegmento( min.ProgressivoMinuto );
                    min.Copertura = "SEH " + co + "/" + MinutiSEH;
                }
            }

        }
        public void AggiornaMinutiFSH_SEH_MultipleFasce ( )
        {
            if ( MinutiFSH > 0 )
            {
                if ( !String.IsNullOrWhiteSpace( this.DalleFSH ) && !String.IsNullOrWhiteSpace( this.AlleFSH ) && ( this.DalleFSH.ToMinutes( ) > 0 || this.AlleFSH.ToMinutes( ) > 0 ) )
                {
                    int fm = this.DalleFSH.ToMinutes( );
                    int to = this.AlleFSH.ToMinutes( );
                    List<DettaglioMinuti> mins = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto > fm && x.ProgressivoMinuto <= to ).ToList( );
                    int co = 0;
                    foreach ( var min in mins )
                    {
                        co++;
                        min.tipo = GetSegmento( min.ProgressivoMinuto );
                        min.Copertura = "FSH " + co + "/" + MinutiFSH;
                    }
                }
                else
                {
                    bool Applicato = false;
                    foreach ( var lacuna in Lacune )
                    {
                        if ( lacuna.durata == MinutiFSH )
                        {
                            Applicato = true;
                            for ( int i = 1 ; i <= lacuna.durata ; i++ )
                            {
                                var m = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto == lacuna.dalleMin + i ).FirstOrDefault( );
                                if ( m != null )
                                {
                                    m.tipo = GetSegmento( m.ProgressivoMinuto );
                                    m.Copertura = "FSH " + i + "/" + MinutiFSH;
                                }
                            }
                            break;
                        }
                    }
                    if ( !Applicato )
                    {
                        int FSH = MinutiFSH;
                        foreach ( var lacuna in Lacune )
                        {
                            for ( int i = 1 ; i <= lacuna.durata ; i++ )
                            {
                                if (FSH == 0) break;
                                var m = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto == lacuna.dalleMin + i ).FirstOrDefault( );
                                if ( m != null )
                                {
                                    m.Copertura = "FSH " + i + "/" + MinutiFSH;
                                    m.tipo = GetSegmento( m.ProgressivoMinuto );
                                    FSH--;
                                }
                            }
                            if (FSH == 0) break;
                        }
                    }
                }
            }
            if ( MinutiSEH > 0 )
            {
                if ( !String.IsNullOrWhiteSpace( this.DalleSEH ) && !String.IsNullOrWhiteSpace( this.AlleSEH ) && ( this.DalleSEH.ToMinutes( ) > 0 || this.AlleSEH.ToMinutes( ) > 0 ) )
                {
                    int fm = this.DalleSEH.ToMinutes( );
                    int to = this.AlleSEH.ToMinutes( );
                    List<DettaglioMinuti> mins = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto > fm && x.ProgressivoMinuto <= to ).ToList( );
                    int co = 0;
                    foreach ( var min in mins )
                    {
                        co++;
                        min.tipo = GetSegmento( min.ProgressivoMinuto );
                        min.Copertura = "SEH " + co + "/" + MinutiSEH;
                    }
                }
                else
                {
                    bool Applicato = false;
                    foreach ( var lacuna in Lacune )
                    {
                        if ( lacuna.durata == MinutiSEH )
                        {
                            Applicato = true;
                            for ( int i = 0 ; i < lacuna.durata ; i++ )
                            {
                                var m = DettaglioMinutiGiornata.Where( x => x.ProgressivoMinuto == lacuna.dalleMin + i ).FirstOrDefault( );
                                if ( m != null )
                                {
                                    m.tipo = GetSegmento( m.ProgressivoMinuto );
                                    m.Copertura = "SEH " + i + "/" + MinutiSEH;
                                }
                            }
                            break;
                        }
                    }
                    if ( !Applicato )
                    {
                        int SEH = MinutiSEH;
                        foreach ( var lacuna in Lacune )
                        {
                            for ( int i = 1 ; i <= lacuna.durata ; i++ )
                            {
                                if (SEH == 0) break;
                                var m = DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto == lacuna.dalleMin + i-1).FirstOrDefault();
                                if (lacuna.dalleMin == MinutiInizioOrario && i == 1) continue;

                                if ( m != null )
                                {
                                    m.Copertura = "SEH " + (MinutiSEH - SEH + 1) + "/" + MinutiSEH ;
                                    m.tipo = GetSegmento( m.ProgressivoMinuto );
                                    SEH--;
                                }
                            }
                            if (SEH == 0) break;
                        }
                    }
                }
            }
        }
    }


    public class DayAnalysisSRH6 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "6_21" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {

            Fascia f = this.Fasce.Where(x => FasceOrarie.Contains (x.fascia)).FirstOrDefault();

            //se c'è umh , è 35 ed è a cavallo
            if ( !String.IsNullOrWhiteSpace( this.DalleUMH ) && !String.IsNullOrWhiteSpace( this.AlleUMH ) && this.MinutiUMH == 35 )
            {
                if ( this.DalleUMH.ToMinutes( ) < 21 * 60 && this.AlleUMH.ToMinutes( ) >= 21 * 60 )
                {
                    int DaScalare = 21 * 60 - this.DalleUMH.ToMinutes( );
                    if ( f != null )
                        f.minuti -= DaScalare;
                }
            }
            else
            {
                if ( f != null )
                    f.minuti -= MinutiIntervalloMensa;
            }

            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = ( f == null ? 0 : f.minuti ) ,
                DettMinuti = this.DettaglioMinutiGiornata
            };
            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class DayAnalysisSRH8 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "21_24", "24_6" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            Fascia f = this.Fasce.Where( x => x.fascia == "21_24" ).FirstOrDefault( );
            //se c'è umh , è 35 ed è a cavallo
            int DaScalare = 0;
            if ( !String.IsNullOrWhiteSpace( this.DalleUMH ) && !String.IsNullOrWhiteSpace( this.AlleUMH ) && this.MinutiUMH == 35 )
            {
                if ( this.DalleUMH.ToMinutes( ) <= 21 * 60 && this.AlleUMH.ToMinutes( ) > 21 * 60 )
                {
                    DaScalare = this.AlleUMH.ToMinutes( ) - ( 21 * 60 );
                }
            }
            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = Fasce.Where(x => FasceOrarie.Contains (x.fascia)).DefaultIfEmpty().Sum(x => x != null ? x.minuti : 0),
                DettMinuti = this.DettaglioMinutiGiornata
            };
            if ( DaScalare > 0 && e.QuantitaMinuti >= DaScalare )
                e.QuantitaMinuti -= DaScalare;

            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }
    public class DayAnalysisMFH6 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "6_21" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita()
        {

            var f = this.Fasce.Where(x => FasceOrarie.Contains(x.fascia)).FirstOrDefault();
            if (f != null)
                f.minuti -= MinutiIntervalloMensa;

            var e = new EccezioneQuantita()
            {
                Eccezione = this.CodiceEccezione,
                QuantitaMinuti = (f == null ? 0 : f.minuti),
                DettMinuti = this.DettaglioMinutiGiornata
            };
            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM();
            return e;
        }
    } 
    public class DayAnalysisLFH6 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "6_21" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {

            var f = this.Fasce.Where(x => FasceOrarie.Contains(x.fascia)).FirstOrDefault();
            if ( f != null )
                f.minuti -= MinutiIntervalloMensa;

            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = ( f == null ? 0 : f.minuti ) ,
                DettMinuti = this.DettaglioMinutiGiornata
            };
            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class DayAnalysisLFH8 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "21_24","24_6" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = Fasce.Where(x => FasceOrarie.Contains(x.fascia)).DefaultIfEmpty().Sum(x => x != null ? x.minuti : 0),
                DettMinuti = this.DettaglioMinutiGiornata
            };

            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class DayAnalysisLPH5 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "6_21" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            var f = this.Fasce.Where(x => FasceOrarie.Contains(x.fascia)).FirstOrDefault();
            if ( f != null )
                f.minuti -= MinutiIntervalloMensa;

            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = ( f == null ? 0 : f.minuti ) ,
                DettMinuti = this.DettaglioMinutiGiornata
            };
            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class DayAnalysisLPH6 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "21_24" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            var f = this.Fasce.Where(x => FasceOrarie.Contains(x.fascia)).FirstOrDefault();

            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = ( f == null ? 0 : f.minuti ) ,
                DettMinuti = this.DettaglioMinutiGiornata
            };
            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class DayAnalysisLPH7 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() {  "24_6" };
            }
        }
        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            var f = this.Fasce.Where(x =>FasceOrarie.Contains( x.fascia)).FirstOrDefault();
            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = ( f == null ? 0 : f.minuti ) ,
                DettMinuti = this.DettaglioMinutiGiornata
            };
            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class DayAnalysisLNH5 : DayAnalysisBase
    {
        protected override List<string> FasceOrarie
        {
            get
            {
                return new List<string>() { "21_24", "24_6" };
            }
        }
         

        public int GetMinutiConsiderandoSogliaNotturno ( int MinutiCalcolati )
        {
            if ( TimbratureManager.TurnoMaggioreDiSogliaNotturno( dayresp.orario.hhmm_entrata_48 , dayresp.orario.hhmm_uscita_48 ) )
            {
                int quantita;
                if ( int.TryParse( dayresp.orario.prevista_presenza , out quantita ) )
                {
                    if ( MinutiCalcolati < quantita )
                    {
                        if (this.CodiceEccezione == "DH60")
                        {
                            this.EccezioniEliminateSeOverSogliaNotturno = "DH40";
                            return this.Fasce.Where(x => x.fascia == "6_21" || x.fascia == "24_6").Select(x => x.minuti).Sum();
                    }

                        return this.DettaglioMinutiGiornata.Where(x => x.ProgressivoMinuto <= this.MinutiFineOrario && x.tipo != null).Count();
                    }
                }
            }
            return MinutiCalcolati;
        }

        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                QuantitaMinuti = Fasce.Where(x => FasceOrarie.Contains(x.fascia)).DefaultIfEmpty().Sum(x => x != null ? x.minuti : 0),
               // QuantitaMinuti = Fasce.Where(x => x.fascia == "21_24" || x.fascia == "24_6").DefaultIfEmpty().Sum(x => x != null ? x.minuti : 0),
                DettMinuti = this.DettaglioMinutiGiornata
            };

            e.QuantitaMinuti = GetMinutiConsiderandoSogliaNotturno( e.QuantitaMinuti );



            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }

    }

    public class DayAnalysisLMH5 : DayAnalysisLPH5 { }

    public class DayAnalysisLMH6 : DayAnalysisLPH6 { }

    public class DayAnalysisLMH7 : DayAnalysisLPH7 { }

    public class DayAnalysisDH40 : DayAnalysisLFH6 { }

    public class DayAnalysisDH60 : DayAnalysisLNH5 { }



    public class DayAnalysisAT30 : DayAnalysisBase
    {

        public override EccezioneQuantita GetEccezioneQuantita ( )
        {
            if ( dayRespPrevious == null )
            {
                throw new Exception( "Parametro mancante per il computo dell'eccezione" );
            }


            if ( this.MinutiPrimoIngresso > MinutiInizioOrario )
                MinutiInizioOrario = this.MinutiPrimoIngresso;


            int MinutiFineIeri = Convert.ToInt32( dayRespPrevious.orario.uscita_iniziale );
            if ( dayRespPrevious.eccezioni != null && dayRespPrevious.eccezioni.Any( x => x.cod.Trim( ) == "SMAP" ) )
            {
                foreach ( var smap in dayRespPrevious.eccezioni.Where( x => x.cod.Trim( ) == "SMAP" ) )
                {
                    if ( smap.alle.ToMinutes( ) > MinutiFineIeri )
                        MinutiFineIeri = smap.alle.ToMinutes( );
                }
            }

            int diff = ( ( 24 * 60 ) - MinutiFineIeri ) + MinutiInizioOrario;

            var e = new EccezioneQuantita( )
            {
                Eccezione = this.CodiceEccezione ,
                DettMinuti = this.DettaglioMinutiGiornata
            };

            if ( diff < 660 )
            {
                e.QuantitaMinuti = 660 - diff;
                e.Dalle = MinutiInizioOrario.ToHHMM( );
                e.Alle = ( MinutiInizioOrario + e.QuantitaMinuti ).ToHHMM( );
            }
            else
                e.QuantitaMinuti = 0;

            e.QuantitaMinutiHHMM = e.QuantitaMinuti.ToHHMM( );
            return e;
        }
    }

    public class SMAP
    {
        public string dalle { get; set; }
        public string alle { get; set; }
        public int minuti { get; set; }
        public bool coda { get; set; }
    }
}