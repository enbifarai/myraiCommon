using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using myRaiData;


namespace myRaiHelper
{
    public class sidebarModel
    {
		private const string DEFAULT_HEADER = "Svil_Header";

		public sidebarModel ()
		{
			this.HeaderMenu = new List<myRaiData.MyRai_HeaderMenu>();

			try
			{
                var filtro = CommonHelper.GetHeaderFiltroContesto();
				using ( var db = new myRaiData.digiGappEntities() )
				{
					this.HeaderMenu = new List<myRaiData.MyRai_HeaderMenu>();
					this.HeaderMenu = db.MyRai_HeaderMenu.Where(filtro).OrderBy( m => m.IdParent ).OrderBy( m2 => m2.Posizione ).ToList();
				}
			}
			catch ( Exception ex )
			{
				this.HeaderMenu = new List<myRaiData.MyRai_HeaderMenu>();
			}
		}

        public List<sezioniMenu> sezioni = new List<sezioniMenu>();
        public List<MyRai_Profili> myProfili { get; set; }
        public string ViewHelp;
        public sidebarModel(string matricola, string pMatricola, int attivo, string viewhelp = "") 
        {
            var filtro = CommonHelper.GetHeaderFiltroContesto();
            using ( var db = new myRaiData.digiGappEntities() )
			{
				this.HeaderMenu = new List<myRaiData.MyRai_HeaderMenu>();
                try
                {
                    this.HeaderMenu = db.MyRai_HeaderMenu.Where(filtro).OrderBy(m => m.IdParent).OrderBy(m2 => m2.Posizione).ToList();
                }
                catch (Exception e)
                {
                }
			}

			if ( attivo != 0 )
			{

				Boolean GappDown = UtenteHelper.GappChiuso();

				ViewHelp = viewhelp;
                List<string> grADpersonali = UtenteHelper.ADgroups();

				sezioni = new List<sezioniMenu>();

				digiGappEntities cont = new digiGappEntities();

				bool abilitatoGapp = UtenteHelper.IsAbilitatoGapp();

				var itemColl = from p in cont.MyRai_Voci_Menu
								   .OrderBy( x => x.progressivo )
								   .Where( x =>
					( GappDown == true && x.richiede_gapp == false ) || GappDown == false &&
					( abilitatoGapp || ( !abilitatoGapp && x.richiede_gapp == false ) ) )
							   where ( cont.MyRai_Abilita_VociMenu.Where( a => ( ( a.Abilitato.Substring( 0, 3 ) == "GAD" ) || ( a.Abilitato.Substring( 0, 3 ) == "LRP" ) || ( a.Abilitato == "*" ) || ( a.Abilitato == matricola ) ) ).Select( z => z.Id_Voce_Menu ) ).Contains( p.ID )
							   orderby p.Titolo != "Situazione", p.Titolo
							   select p;

				bool carica = true;

				foreach ( var item in itemColl )
				{

					if ( sezioni.Any( x => x.ID == item.ID ) )
						continue;

					sezioniMenu nuovavoce = new sezioniMenu();
					nuovavoce.Titolo = item.Titolo;
					nuovavoce.nomeSezione = item.nomeSezione;
					nuovavoce.icon = item.icon;
					nuovavoce.codiceMy = item.codiceMy;
					nuovavoce.ID = item.ID;
					nuovavoce.attivo = false;
					nuovavoce.progressivo = item.progressivo;

					if ( item.ID == attivo )
					{
						nuovavoce.attivo = true;
					}
					nuovavoce.customView = item.customView;

					nuovavoce.vociMenu = new List<voceMenu>();

					MyRai_Abilita_VociMenu abilita = cont.MyRai_Abilita_VociMenu.Where( a => ( ( a.Abilitato.Substring( 0, 3 ) == "GAD" ) && ( a.Id_Voce_Menu == item.ID ) ) ).FirstOrDefault();

					string group = "";
					carica = true;
					if ( abilita != null )
					{

						group = grADpersonali.Where( a => a == abilita.Abilitato.Substring( 3 ) ).FirstOrDefault();
						if ( group == null )
						{
							carica = false;
						}
						else
						{
							if ( abilita.Tipo_Inc == 1 )
							{
								//da togliere
								carica = false;
								sezioni.Remove( sezioni.Where( a => a.ID == abilita.Id_Voce_Menu ).FirstOrDefault() );
							}
						}
					}


					MyRai_Abilita_VociMenu abilita2 = cont.MyRai_Abilita_VociMenu.Where( a => ( ( a.Abilitato.Substring( 0, 3 ) == "LRP" ) && ( a.Id_Voce_Menu == item.ID ) ) ).FirstOrDefault();
					//string group = "";
					carica = true;
					if ( abilita2 != null )
					{
						if ( abilita2.Abilitato.Substring( 3 ) == null
							|| ( abilita2.Abilitato.Substring( 3 ) == "01" && !UtenteHelper.IsBoss(pMatricola) )
							|| abilita2.Abilitato.Substring( 3 ) == "02" && !UtenteHelper.IsBossLiv2(pMatricola) )
						{
							carica = false;
							sezioni.Remove( sezioni.Where( a => a.ID == abilita2.Id_Voce_Menu ).FirstOrDefault() );
						}
						else
						{
							if ( abilita2.Tipo_Inc == 1 )
							{
								//da togliere
								carica = false;
								sezioni.Remove( sezioni.Where( a => a.ID == abilita2.Id_Voce_Menu ).FirstOrDefault() );
							}
						}
					}



					MyRai_Abilita_VociMenu abilita3 = cont.MyRai_Abilita_VociMenu.Where( a => ( ( a.Abilitato == matricola ) && ( a.Id_Voce_Menu == item.ID ) ) ).FirstOrDefault();
					//string group = "";
					carica = true;
					if ( abilita3 != null )
					{

						if ( abilita3.Tipo_Inc == 1 )
						{
							//da togliere
							carica = false;
							sezioni.Remove( sezioni.Where( a => a.ID == abilita3.Id_Voce_Menu ).FirstOrDefault() );
						}

					}



					if ( carica )
					{
						if ( item.Id_Padre != null )
						{
							var itempadre = itemColl.Where( x => x.ID == item.Id_Padre ).FirstOrDefault();
							if ( itempadre != null )
							{
								sezioniMenu se = sezioni.Where( f => f.ID == item.Id_Padre ).FirstOrDefault();

								voceMenu v = new voceMenu();
								v.Titolo = item.Titolo;
								v.nomeSezione = item.nomeSezione;
								v.customView = item.customView;
								v.codiceMy = item.codiceMy;
								v.icon = item.icon;
								v.attivo = false;
								v.progressivo = item.progressivo;
								if ( item.ID == attivo )
									v.attivo = true;

								if ( se.vociMenu != null )
								{
									se.vociMenu.Add( v );
									//sezioni.Add(se);
								}
								else
								{
									se.Titolo = itempadre.Titolo;
									se.nomeSezione = itempadre.nomeSezione;
									se.customView = itempadre.customView;
									se.codiceMy = itempadre.codiceMy;
									se.icon = itempadre.icon;
									se.attivo = false;
									se.ID = itempadre.ID;
									se.progressivo = itempadre.progressivo;

									//nuovavoce.vociMenu.Add(v);
									se.vociMenu = new List<voceMenu>();
									se.vociMenu.Add( v );

									sezioni.Add( se );
								}
							}
						}
						else
						{

							sezioni.Add( nuovavoce );
						}
					}

				}



				if ( CommonHelper.GetParametro<Boolean>( EnumParametriSistema.AutorizzaMinimale ) == false )
				{
					return;
					if ( sezioni.Where( a => ( a.customView != null ) ).Count() == 1 && sezioni.Where( a => ( a.customView != null ) ).FirstOrDefault().nomeSezione.Trim().ToUpper() == "SCRIVANIA" )
					{
						HttpContext.Current.Response.Redirect( "/home/notauth" );
					}


					Uri f = HttpContext.Current.Request.Url;
					string url = f.AbsolutePath.ToUpper();
					if ( url == "/" )
					{
						url = "/SCRIVANIA/";
					}
					url = url.Split( '/' )[0];
					if ( sezioni.Where( a => a.customView != null ).Count() > 0 )
					{
						if ( sezioni.Where( a => ( a.customView != null && a.customView.ToUpper().Contains( url ) ) ).Count() == 0 )
						{
							//new redirect

							HttpContext.Current.Response.Redirect( sezioni.Where( d => d.customView != null ).FirstOrDefault().customView.ToUpper() );
						}
						else
						{
							sezioniMenu sz = sezioni.Where( a => ( a.customView != null && a.customView.ToUpper().Contains( url ) ) ).FirstOrDefault();
							sz.attivo = false;

							//sezioni.Where(a => (a.customView != null && a.customView.ToUpper().Contains(url))).FirstOrDefault().attivo = true;
						}
					}
					else
					{
						HttpContext.Current.Response.Redirect( "/home/notauth" );
					}
				}
			}

			//if (attivo == 0) return ;

        }

		public List<myRaiData.MyRai_HeaderMenu> HeaderMenu { get; set; }

		/// <summary>
		/// Nome dell'header da visualizzare
		/// per sviluppo verrà renderizzato l'header di RaiPlace,
		/// mentre per produzione finchè non verrà lanciato RaiPlace avremo
		/// l'header vecchio stile
		/// </summary>
		public string HeaderName
		{
			get
			{
				if ( !String.IsNullOrEmpty( this._headerName ) )
				{
					return this._headerName;
				}
				else if ( SessionHelper.Get("HeaderName") == null )
				{
					using ( var db = new myRaiData.digiGappEntities() )
					{
						var headerData = db.MyRai_ParametriSistema.Where( p => p.Chiave.Equals( "HeaderName", StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault();

						if ( headerData != null )
						{
							SessionHelper.Set("HeaderName", headerData.Valore1);
							this._headerName = headerData.Valore1;
							return this._headerName;
						}
						else
						{
							SessionHelper.Set("HeaderName", DEFAULT_HEADER);
							this._headerName = DEFAULT_HEADER;
							return this._headerName;
						}
					}
				}
				else
				{
					this._headerName = SessionHelper.Get("HeaderName").ToString();
					return this._headerName;
				}
			}
		}

		private string _headerName { get; set; }
    }
}