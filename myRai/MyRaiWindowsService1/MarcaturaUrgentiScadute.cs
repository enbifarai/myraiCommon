using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiData;
using System.Collections.Generic;

namespace MyRaiWindowsService1
{
    class MarcaturaUrgentiScadute
    {
        public static void MarcaturaRichiesteUrgentiScadute()
        {
            var db = new digiGappEntities();
            //individua e marca le urgenti
            var oreUrgenti = Common.GetParametro<int>(EnumParametriSistema.OreRichiesteUrgenti);
            var dataUrgentiMod = DateTime.Now.AddHours(Convert.ToDouble(oreUrgenti));
            var listUrgenti = db.MyRai_Eccezioni_Richieste.Where(x =>
                                    x.id_stato < 20
                                    && (DateTime.Now <= x.data_eccezione && dataUrgentiMod >= x.data_eccezione)).ToList();

            foreach (var item in listUrgenti)
            {
                var update = db.MyRai_Richieste.Where(x => x.id_richiesta == item.id_richiesta).FirstOrDefault();
                if (update.urgente == false)
                {
                    update.urgente = true;
                    db.SaveChanges();
                    Logger.Log("Marcata urgente ID" + update.id_richiesta);
                    AggiungiNotificaMarcatura("Urgente", item.id_richiesta, item.cod_eccezione.ToString(), item.data_eccezione.ToString(), item.codice_sede_gapp.ToString());

                }
            }


            //individua e marca le scadute
            var listScadute = db.MyRai_Eccezioni_Richieste.Where(x =>
                                    x.id_stato < 20
                                    && x.data_eccezione < DateTime.Now && x.data_creazione < x.data_eccezione).ToList();

            foreach (var item in listScadute)
            {
                var update = db.MyRai_Richieste.Where(x => x.id_richiesta == item.id_richiesta).FirstOrDefault();
                if (update.scaduta == false)
                {
                    update.scaduta = true;
                    update.urgente = false;
                    db.SaveChanges();
                    Logger.Log("Marcata scaduta ID" + update.id_richiesta);
                    AggiungiNotificaMarcatura("Scaduta", item.id_richiesta, item.cod_eccezione.ToString(), item.data_eccezione.ToString(), item.codice_sede_gapp.ToString());
                }
            }
        }

        public static void AggiungiNotificaMarcatura(string tipo, int idRichiesta, string codEccezione, string dataEccezione, string codSedeGapp)
        {
            var db = new digiGappEntities();
            var tt = Common.GetMatricolaLivelloPerSede(codSedeGapp, 1);

            String matricola = "";
            for (int i = 0; i < tt.Count; i++)
            {
                if (tt[i].ToString().Substring(0, 1) == "P")
                { matricola = tt[i].Substring(1, tt[i].Length - 1); }
                else
                { matricola = tt[i].ToString(); }

                String email = Common.GetEmailMatricola(matricola);
                String[] listEmail = email.Split(';');

				string descrizioneNotifica = "Nuova marcatura " + tipo.ToLower() + ": " + codEccezione + " del " + dataEccezione.Substring( 0, 10 );

				if ( tipo == "Scaduta" || tipo == "Urgente" )
				{
					try
					{
						using ( digiGappEntities _db = new digiGappEntities() )
						{
							var k = _db.MyRai_Richieste.Where( r => r.id_richiesta.Equals( idRichiesta ) ).FirstOrDefault();

							if ( k != null )
							{
								descrizioneNotifica = String.Format( "{0} - da {1} il {2}", descrizioneNotifica, k.nominativo.Trim(), k.data_richiesta.ToString( "dd/MM/yyyy HH:mm" ) );
							}
						}
					}
					catch ( Exception ex )
					{
						descrizioneNotifica = "Nuova marcatura " + tipo.ToLower() + ": " + codEccezione + " del " + dataEccezione.Substring( 0, 10 );
					}
				}

                myRaiData.MyRai_Notifiche n = new MyRai_Notifiche()
                {
                    categoria = "Marcatura" + tipo,
                    data_inserita = DateTime.Now,
					descrizione = descrizioneNotifica,
                    inserita_da = "ServizioWindows",
                    id_riferimento = idRichiesta,
                    matricola_destinatario = matricola,
                    email_destinatario = listEmail[15].ToString()
                };
                db.MyRai_Notifiche.Add(n);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Log("AggiungiNotificaMarcatura errore:" + ex.ToString());
                }
            }

        }

    }
}
