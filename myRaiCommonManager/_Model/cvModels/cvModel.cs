using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using myRaiHelper;
using myRai.Data.CurriculumVitae;

namespace myRaiCommonModel.cvModels
{
    public class cvModel
    {
        public cvModel(bool loadBoadingImages=true, bool loadTourElements=true)
        {
            if (loadBoadingImages)
            {
                string dir = HttpContext.Current.Server.MapPath("~/assets/img/boarding");
                this.BoardingImages = new List<string>();

                foreach (string s in System.IO.Directory.GetFiles(dir).OrderBy(x => x).ToList())
                {
                    this.BoardingImages.Add("assets/img/boarding/" + System.IO.Path.GetFileName(s));
                }
            }
            if (loadTourElements)
                TourElements = new myRaiData.digiGappEntities().MyRai_Tour.ToList();
        }
        public List<Box> listaBox { get; set; }
        public List<Languages> lingue { get; set; }
        public sidebarModel menuSidebar { get; set; }
        public List<Studies> curricula { get; set; }
        public List<AreeInteresse> areeInteresse { get; set; }
        public List<CompetenzeDigitali> competenzeDigitali { get; set; }
        public List<Formazione> formazione { get; set; }
        public List<ConoscenzeInformatiche> conoscenzeInformatiche { get; set; }
        public AltreInfo altreInformazioni { get; set; }
        public List<ImpegniRAI> impegniEditoriali { get; set; }
        public List<CompetenzeRAI> competenzeRai { get; set; }
        public List<CompetenzeSpecialistiche> competenzeSpecialistiche { get; set; }
        public List<Experiences> experencies { get; set; }
        public List<Certificazioni> certificazioni { get; set; }
        public List<Allegati> allegati { get; set; }
        public Privacy privacy { get; set; }
        public List<string> BoardingImages { get; set; }
        public List<myRaiData.MyRai_Tour> TourElements { get; set; }
        public AutoPresentazioneBox AutoPresentazione { get; set; }
        public bool CanPrenMappatura { get; set; }

        public class Box
        {
            public string _titolo { get; set; }
            public string _icon { get; set; }
            public string _sottotitolo { get; set; }
            public string _colore { get; set; }
            public string _testo { get; set; }
            public string _url { get; set; }
            public int _idMenu { get; set; }
            //coefficiente di peso per il box
            public int _coefficienteBox { get; set; } //freak - coefficiente del peso
            public char _flagObbligatorio { get; set; }
            public string _titoloAggiungi { get; set; }
            public string _funzioniAggiungi { get; set; }
            public sidebarModel menuSidebar { get; set; }
            public Box()
            {

            }
        }




        //classe model per box "Titoli di Studio e Specializzazioni"

        public class Studies :TourReady
        {
            public string _matricola { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codTitolo { get; set; }

            /// <summary>
            /// Codice del tipo di istituto valorizzato nel caso di update.
            /// Questo valore durante il salvataggio viene confrontato con _codTitolo, se sono 
            /// diversi significa che è stato modifica il tipo di istituto, quindi il sistema dovrà rimovere il
            /// vecchio record ed inserirne uno nuovo
            /// </summary>
            public string OldCodTitolo { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codTipoTitolo { get; set; }//è anche il campo TipoSpecial nel caso della tabella TCVSpecializz
            
            public string _descTipoTitolo { get; set; } //campo che andrà riempito tramite store procedure dbo.sp_GETDESCTITOLO
            
            public string _descTitolo { get; set; } //campo che andrà riempito tramite store procedure dbo.sp_GETDESCTITOLO
            
            [StringLength(3, ErrorMessage = "Max 3 caratteri")]
            public string _voto { get; set; }

            [VotoScalaCheck]
            [StringLength(3, ErrorMessage = "Max 3 caratteri")]
            public string _scala { get; set; }
            public char? _lode { get; set; }
            public string _durata { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            [StringLength(250, ErrorMessage = "Max 250 caratteri")]
            public string _istituto { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            [StringLength(250, ErrorMessage = "Max 250 caratteri")]
            public string _corsoLaurea { get; set; }

            [StringLength(250, ErrorMessage = "Max 250 caratteri")]
            public string _titoloTesi { get; set; }

            public char _stato { get; set; }

            public char _tipoAgg { get; set; }

            public DateTime? _dataoraAgg { get; set; }

            public int _prog { get; set; } //numero progressivo che fa da chiave primaria insieme a matricola

            [Required(ErrorMessage = "Campo obbligatorio")]
            [StringLength(250, ErrorMessage = "Max 250 caratteri")]
            public string _titoloSpecializ { get; set; }

            //[Required(ErrorMessage="Campo obbligatorio")]
            public string _dataInizio { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            [InizioFineDataCheck]
            public string _dataFine { get; set; }

            [StringLength(250, ErrorMessage = "Max 250 caratteri")]
            public string _localitaStudi { get; set; }
            public string _codNazione { get; set; }
            public string _descNazione { get; set; }

            [StringLength(250, ErrorMessage = "Max 250 caratteri")]
            public string _indirizzoStudi { get; set; }
            public string _logo { get; set; }
            public string _tableTarget { get; set; } // ="I"->TCVIstruzione ;;; ="S"->TCVSpecializz

            [StringLength(50, ErrorMessage = "Max 50 caratteri")]
            public string _riconoscimento { get; set; }
            public string _note { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codIstituto { get; set; }

            /// <summary>
            /// elenco degli istituti visualizzabili nella schermata di inserimento o modifica diploma
            /// </summary>
            public List<SelectListItem> Diplomi
            {
                get
                {
                    if (_diplomi != null && _diplomi.Any())
                        return _diplomi;
                    else
                    {
                        this._diplomi = this.GetIstituti("DI", this._codIstituto);
                        return this._diplomi;
                    }
                }
                set { }
            }

            /// <summary>
            /// Elenco degli istituti visualizzabili nella schermata di inserimento o modifica di un master
            /// </summary>
            public List<SelectListItem> Master {
                get
                {
                    if (_master != null && _master.Any())
                        return _master;
                    else
                    {
                        this._master = this.GetIstituti("MA", this._codIstituto);
                        return this._master;
                    }
                }
                set { }
            }

            /// <summary>
            /// Elenco degli istituti visualizzabili nella schermata di inserimento o modifica di un corso di laurea
            /// </summary>
            public List<SelectListItem> Lauree {
                get
                {
                    return this.GetIstituti(this._codTipoTitolo, this._codIstituto);
                }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            public List<SelectListItem> TipiLauree
            {
                get
                {
                    if (_tipiLauree != null && _tipiLauree.Any())
                        return _tipiLauree;
                    else
                    {
                        this._tipiLauree = this.GetTipoTitoli(this._codTipoTitolo);
                        return this._tipiLauree;
                    }
                }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            public List<SelectListItem> Paesi
            {
                get
                {
                    if (_paesi != null && _paesi.Any())
                        return _paesi;
                    else
                    {
                        this._paesi = this.GetPaesi(this._localitaStudi);
                        return this._paesi;
                    }
                }
                set { }
            }

            public List<SelectListItem> EntiErogatori
            {
                get
                {
                    if (_entiErogatori != null && _entiErogatori.Any())
                        return _entiErogatori;
                    else
                    {
                        this._entiErogatori = this.GetEntiErogatori(null);
                        return this._entiErogatori;
                    }
                }
                set { }
            }

            private List<SelectListItem> _diplomi = null;
            private List<SelectListItem> _master = null;
            private List<SelectListItem> _tipiLauree = null;
            private List<SelectListItem> _paesi = null;
            private List<SelectListItem> _entiErogatori = null;

			/// <summary>
			/// Attributo utilizzato per ordinare la lista dei titoli di studio in base alla
			/// data di conseguimento
			/// </summary>
			public DateTime? DataConseguimento 
			{
				get
				{
					return this._dataFineInternal;
				}
				set
				{
					this._dataFineInternal = value;
				}
			}

			/// <summary>
			/// Attributo utilizzato per convertire la data stringa in formato DateTime
			/// </summary>
			private DateTime? _dataFineInternal 
			{
				get
				{
					try
					{
						if ( !String.IsNullOrWhiteSpace(this._dataFine) )
						{
							// se la lunghezza è 4 contiene solo l'anno
							if ( this._dataFine.Length == 4 )
							{
								int year = int.Parse( this._dataFine );
								return new DateTime( year, 01, 01 );
							}
							else // altrimenti la data è nel formato gg/mm/aaaa
							{
								return DateTime.Parse( this._dataFine );
							}
						}
						else
						{
							return null;
						}
					}
					catch
					{
						return null;
					}
				}
				set
				{
				}
			}

            public Studies() : base(CommonHelper.GetCurrentUserMatricola())
            {
                this.Diplomi = new List<SelectListItem>();
                this.Master = new List<SelectListItem>();
                this.Lauree = new List<SelectListItem>();
                this.TipiLauree = new List<SelectListItem>();
                this.EntiErogatori = new List<SelectListItem>();
            }

            public Studies(string matricola):base(matricola)
            {
                this.Diplomi = new List<SelectListItem>();
                this.Master = new List<SelectListItem>();
                this.Lauree = new List<SelectListItem>();
                this.TipiLauree = new List<SelectListItem>();
                this.EntiErogatori = new List<SelectListItem>();
            }

            /// <summary>
            /// Reperimento degli istituti in base alla tipologia, Master, Laurea, Diploma
            /// </summary>
            /// <param name="tipoIstituto">Possibili valori: MA, DI etc etc</param>
            /// <param name="selectedItem"></param>
            private List<SelectListItem> GetIstituti(string tipoIstituto, string selectedItem = null)
            {
                List<DTitolo> results = new List<DTitolo>();
                try
                {
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        results = (from titolo in db.DTitolo
                                   where titolo.CodTipoTitolo.Equals(tipoIstituto, StringComparison.InvariantCultureIgnoreCase)
                                   select titolo).ToList();

                        results = results.OrderBy(c => c.DescTitolo).ToList();
                    }

                    var institutes = new List<SelectListItem>();
                    // Inserimento dell'elemento vuoto
                    institutes.Add(new SelectListItem()
                    {
                        Text = null,
                        Value = null,
                        Selected = false
                    });

                    if (results != null && results.Any())
                    {
                        results.ForEach(d =>
                        {
                            bool selected = false;
                            if (!String.IsNullOrEmpty(selectedItem))
                            {
                                if (selectedItem.Equals(d.CodTitolo, StringComparison.InvariantCultureIgnoreCase))
                                    selected = true;
                            }

                            institutes.Add(new SelectListItem()
                            {
                                Text = d.DescTitolo,
                                Value = d.CodTitolo,
                                Selected = selected
                            });
                        });
                    }

                    return institutes;
                }
                catch (Exception ex)
                {
                    return new List<SelectListItem>();
                }
            }

            /// <summary>
            /// Reperimento dei tipi titoli che sono selezionabili dalla view di inserimento/modifica di una laurea
            /// </summary>
            /// <param name="selectedItem"></param>
            /// <returns></returns>
            private List<SelectListItem> GetTipoTitoli(string selectedItem = null)
            {
                List<DTipoTitolo> tipoTitoli = new List<DTipoTitolo>();
                try
                {
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        var tt = (from tipotitolo in db.DTipoTitolo
                                  select tipotitolo).ToList();
                        tt = tt.Where(m => Convert.ToInt32(m.Livello) > 40 && Convert.ToInt32(m.Livello) < 90).ToList();

                        tipoTitoli = tt.OrderBy(c => c.DescTipoTitolo).ToList();
                    }

                    var institutes = new List<SelectListItem>();
                    // Inserimento dell'elemento vuoto
                    institutes.Add(new SelectListItem()
                    {
                        Text = null,
                        Value = null,
                        Selected = false
                    });

                    if (tipoTitoli != null && tipoTitoli.Any())
                    {
                        tipoTitoli.ForEach(d =>
                        {
                            bool selected = false;
                            if (!String.IsNullOrEmpty(selectedItem))
                            {
                                if (selectedItem.Equals(d.CodTipoTitolo, StringComparison.InvariantCultureIgnoreCase))
                                    selected = true;
                            }

                            institutes.Add(new SelectListItem()
                            {
                                Text = d.DescTipoTitolo,
                                Value = d.CodTipoTitolo,
                                Selected = selected
                            });
                        });
                    }

                    return institutes;
                }
                catch (Exception ex)
                {
                    return new List<SelectListItem>();
                }
            }

            /// <summary>
            /// Reperimento dell'elenco dei paesi selezionabili
            /// </summary>
            /// <param name="selectedItem"></param>
            /// <returns></returns>
            private List<SelectListItem> GetPaesi(string selectedItem = null)
            {
                List<DNazione> nazioni = new List<DNazione>();
                try
                {
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        var naz = (from nazione in db.DNazione
                                   orderby nazione.QTA_ORDINE, nazione.DES_NAZIONE
								   select nazione ).OrderBy( x => new 
								   { x.QTA_ORDINE, x.DES_NAZIONE } ).ToList();

						if ( naz != null )
						{
							foreach ( var itm in naz )
							{
								itm.DES_NAZIONE = CommonHelper.ToTitleCase( itm.DES_NAZIONE );
							}
						}

                        nazioni = naz.ToList();
                    }

                    var nations = new List<SelectListItem>();
                    // Inserimento dell'elemento vuoto
                    nations.Add(new SelectListItem()
                    {
                        Text = null,
                        Value = null,
                        Selected = false
                    });

                    if (nazioni != null && nazioni.Any())
                    {
                        nazioni.ForEach(d =>
                        {
                            bool selected = false;
                            if (!String.IsNullOrEmpty(selectedItem))
                            {
                                if (selectedItem.Equals(d.COD_SIGLANAZIONE, StringComparison.InvariantCultureIgnoreCase))
                                    selected = true;
                            }

                            nations.Add(new SelectListItem()
                            {
                                Text = d.DES_NAZIONE,
                                Value = d.COD_SIGLANAZIONE,
                                Selected = selected
                            });
                        });
                    }

                    return nations;
                }
                catch (Exception ex)
                {
                    return new List<SelectListItem>();
                }
            }

            /// <summary>
            /// Reperimento dell'elenco degli atenei/enti erogatori
            /// </summary>
            /// <param name="selectedItem"></param>
            /// <returns></returns>
            private List<SelectListItem> GetEntiErogatori(string selectedItem = null)
            {
                List<DAteneoCV> atenei = new List<DAteneoCV>();
                try
                {
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        atenei = db.DAteneoCV.OrderBy(x => x.Descrizione).ToList();
                    }

                    var resultList = new List<SelectListItem>();
                    // Inserimento dell'elemento vuoto
                    resultList.Add(new SelectListItem()
                    {
                        Text = null,
                        Value = null,
                        Selected = false
                    });

                    if (atenei != null && atenei.Any())
                    {
                        atenei.ForEach(d =>
                        {
                            bool selected = false;
                            if (!String.IsNullOrEmpty(selectedItem))
                            {
                                if (selectedItem.Equals(d.Codice, StringComparison.InvariantCultureIgnoreCase))
                                    selected = true;
                            }

                            resultList.Add(new SelectListItem()
                            {
                                Text = d.Descrizione,
                                Value = d.Codice,
                                Selected = selected
                            });
                        });

                        if (!String.IsNullOrEmpty(this._codIstituto) && this._codIstituto.Equals("-1", StringComparison.InvariantCultureIgnoreCase))
                        {
                            resultList.Insert(1, new SelectListItem()
                            {
                                Selected = true,
                                Text = this._istituto,
                                Value = this._codIstituto
                            });
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(this._codIstituto) && this._codIstituto.Equals("-1", StringComparison.InvariantCultureIgnoreCase))
                        {
                            resultList.Add(new SelectListItem()
                            {
                                Selected = true,
                                Text = this._istituto,
                                Value = this._codIstituto
                            });
                        }
                    }

                    return resultList;
                }
                catch (Exception ex)
                {
                    return new List<SelectListItem>();
                }
            }
        }

        public class Languages
        {
            public string _matricola { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codLingua { get; set; }
            public string _altraLingua { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codLinguaLiv { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _livAscolto { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _livLettura { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _livInterazione { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _livProdOrale { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _livScritto { get; set; }
            [StringLength(250, ErrorMessage = "Lunghezza massima di 250 caratteri")]
            public string _note { get; set; }
            public string _descLingua { get; set; }
            public string _descLinguaLiv { get; set; }
            public string _flagStato { get; set; }


            public Languages()
            {
                this._matricola = UtenteHelper.EsponiAnagrafica()._matricola;
                this._codLinguaLiv = "01";
            }
            public Languages (string matricola )
            {
                this._matricola = UtenteHelper.EsponiAnagrafica( )._matricola;
                this._codLinguaLiv = "01";
            }

        }

        public class AreeInteresse : TourReady
        {
            public string _matricola { get; set; }
            public int _prog { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codAreaOrg { get; set; }
            public string _codServizio { get; set; }
            [StringLength(1000, ErrorMessage = "Inserire massimo 1000 caratteri")]
            public string _areeIntDispo { get; set; }
            public string _codTipoDispo { get; set; }
            [StringLength(1000, ErrorMessage = "Inserire massimo 1000 caratteri")]
            public string _profIntDispo { get; set; }
            public string _flagEsteroDispo { get; set; }
            public string _codAreaGeo { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _descAreaOrg { get; set; } //prendo la descrizione da: DAreaOrg
            public string _descServizio { get; set; } //prendo la descrizione da: VDServizio
            public string _descTipoDispo { get; set; } // prenso la descrizione da: DTipoDispo
            public string _descAreaGeo { get; set; } //prendo la descrizione da: DAreaGeoGio
            public List<SelectListItem> AreaInteresseItems { get; set; }

            public AreeInteresse() : base(CommonHelper.GetCurrentUserMatricola())
            {
                this.AreaInteresseItems = this.getAreeInteresse();
            }
            public AreeInteresse(string matricola) : base(matricola)
            {
                this.AreaInteresseItems = this.getAreeInteresse();
            }
            public List<SelectListItem> getAreeInteresse()
            {
                var aree = new List<SelectListItem>();
                aree.Add(new SelectListItem()
                {
                    Text = "",
                    Value = null
                });

                var db = new myRai.Data.CurriculumVitae.cv_ModelEntities();
                var list = db.DTabellaCV.Where(x => x.NomeTabella == "AreaInteresse").OrderBy(x => x.Descrizione).ToList();
                foreach (var item in list)
                {
                    aree.Add(new SelectListItem()
                    {
                        Text = item.Descrizione,
                        Value = item.Codice
                    });
                }
                return aree;
            
            }
        }

        public class CompetenzeDigitali
        {
            public string _matricola { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _codCompDigit { get; set; }

            public string _codCompDigitLiv { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _descCompDigit { get; set; }
            public string _descCompDigitLiv { get; set; }
            public string _descCompDigitLivLunga { get; set; }

            public CompetenzeDigitali()
            {

            }
        }

        public class Formazione :TourReady 
        {
            public string _matricola { get; set; }
            public int _prog { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _corso { get; set; }
            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _anno { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            [StringLength(50, ErrorMessage="Massimo 50 caratteri")]
            public string _durata { get; set; }
            public string _presso { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _localitaCorso { get; set; }
            public string _codNazione { get; set; } //riferimento a tabella DNazione
            [StringLength(1000, ErrorMessage = "Massimo 1000 caratteri")]
            public string _note { get; set; }
            public string _descNazione { get; set; }

            public Formazione() : base(CommonHelper.GetCurrentUserMatricola())
            {

            }

            public Formazione(string matricola) : base(matricola)
            {

            }
        }

        public class ConoscenzeInformatiche
        {
            public string _matricola { get; set; }
            public string _codConInfo { get; set; }
            public int _prog { get; set; }
            public string _codConInfoLiv { get; set; }
            [StringLength(50, ErrorMessage = "Inserire massimo 50 caratteri")]
            public string _altraConInfo { get; set; }
            public string _note { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _descConInfo { get; set; } //tabella riferimento DConInfo
            public string _codGruppoConInfo { get; set; }
            public string _codPosizione { get; set; }
            public string _descConInfoLiv { get; set; } //tabella riferimento DConInfoLiv
            public string _descGruppoConInfo { get; set; } //tabella riferimento DGruppoConInfo
            public bool _selectedConInfo { get; set; } //proprietà che flagga se CodConInfo è selezionato

            public ConoscenzeInformatiche()
            {

            }
        }

        public class AltreInfo:TourReady
        {
            public string _matricola { get; set; }

            public string _indirizzo_residenza { get; set; }
            public string _comune_residenza { get; set; }
            public string _descNazione_residenza { get; set; }
            public string _cap_residenza { get; set; }
            public string _provincia_residenza { get; set; }

            public string _indirizzo_domicilio { get; set; }
            public string _comune_domicilio { get; set; }
            public string _descNazione_domicilio { get; set; }
            public string _cap_domicilio { get; set; }
            public string _provincia_domicilio { get; set; }
            //completare il modello con i dati che mi passa Tesoroooo
            public string _tipoTel1 { get; set; }
            public string _tipoTel2 { get; set; }
            public string _numTel1 { get; set; }
            public string _numTel2 { get; set; }
            //blic string _codPatente { get; set; } //tabella con relazione molti <-> molti
            [StringLength(50, ErrorMessage = "Inserire massimo 50 caratteri")]
            public string _email { get; set; }
            public string _sitoWeb { get; set; }
            public string _stato { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _tipoAgg { get; set; }
            public string _note { get; set; }
            public List<DTipoPatente> _tipoPatente { get; set; }

            public AltreInfo() : base(CommonHelper.GetCurrentUserMatricola())
            {

            }
            public AltreInfo(string matricola) : base(matricola)
            {

            }
        }

        public class ImpegniRAI
        {
            public string _matricola { get; set; }
            public int _idEsperienze { get; set; }
            public int _progDaStampare { get; set; }
            public string _desTitoloDefinit { get; set; }
            public string _matricolaSpett { get; set; }
            public string _dtDataInizio { get; set; }
            public string _dtDataFine { get; set; }
            public string _ruolo { get; set; }

            public ImpegniRAI()
            {

            }
        }

        public class CompetenzeRAI
        {
            public string _matricola { get; set; }
            public string _codConProf { get; set; }
            public int? _prog { get; set; }
            public string _descConProf { get; set; }
            public string _flagPrincipale { get; set; }
            public string _flagSecondario { get; set; }
            public string _flagChoice { get; set; }
            public string _flagExtraRai { get; set; }
            public string _stato { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _tipoAgg { get; set; }
            public string _figuraProfessionale { get; set; }
            public string descrittiva_lunga { get; set; }

            public CompetenzeRAI()
            {

            }
        }

        public class CompetenzeSpecialistiche
        {
            public string _matricola { get; set; }
            public string _codConProf { get; set; }
            public int? _prog { get; set; }
            public string _descConProf { get; set; }
            public string _descConProfLunga { get; set; }
            public string _codConProfLiv { get; set; } // può assumere valori "01" - "02" - "04"
            public string _stato { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _tipoAgg { get; set; }
            public string _figuraProfessionale { get; set; }
            public int _posizione { get; set; }
            public string _codConProfAggr { get; set; }
            public bool _isTitle { get; set; }
            public bool _isSelected { get; set; }
            public string _flagPrincipale { get; set; } //maggiormente presidiato

            public CompetenzeSpecialistiche()
            {

            }
        }
        public class TourReady
        {
            public List<myRaiData.MyRai_Tour> TourElements;
            public List<sp_GETSTORFIGPRO_Result> ListeFigureRai;
            public TourReady(string matricola)
            {
                TourElements = new myRaiData.digiGappEntities().MyRai_Tour.ToList();
                using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", matricola);
                    //var param_naz = new SqlParameter("@param_naz", istr.CodNazione);
                    //freak - devo crearmi una classe con i tre campi "descTipoTitolo descTitolo Logo
                    //var param_naz = new SqlParameter("@param_naz", "AND");
                    List<sp_GETSTORFIGPRO_Result> tmp = ctx.Database.SqlQuery<sp_GETSTORFIGPRO_Result>("exec sp_GETSTORFIGPRO @param", param).ToList();
                    ListeFigureRai = tmp;
                }
                
            }
        }
        public class Experiences : TourReady 
        {
            public Experiences():base(CommonHelper.GetCurrentUserMatricola())
            {
                
            }
            public Experiences(string matricola) : base(matricola)
            {

            }

            public string _matricola { get; set; }
            public int _prog { get; set; }
            //[Required(ErrorMessage = "Campo obbligatorio")]
            private string __dataInizio;
            public string _dataInizio {
                get
                {
                    return __dataInizio;
                }
                set
                {
                    __dataInizio = value;
                    SplitDataInizio();
                }
            }
            //[Required(ErrorMessage = "Campo obbligatorio")]
            private string __dataFine;
            public string _dataFine {
                get
                {
                    return __dataFine;
                }
                set
                {
                    __dataFine = value;
                    SplitDataFine();
                }
            }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _societa { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _codSocieta { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _areaAtt { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _ultRuolo { get; set; } //Campo Incarico nella form
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _codRedazione { get; set; }
            public string _descRedazione { get; set; }
            public string _flagEspRai { get; set; }
            public string _flagEspEstero { get; set; }
            public string _codContinente { get; set; }
            public string _descContinente { get; set; }
            public string _nazione { get; set; }
            public string _direzione { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _codDirezione { get; set; }
            public string _codLocalitaEsp { get; set; }
            public string _localitaEsp { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _codiceFiguraProf { get; set; }
            public string _titoloEspQual { get; set; }
            public string _descrizioneEsp { get; set; }
            public string _isGiornalista { get; set; }
            public string _codProcura { get; set; }
            public string _procura { get; set; }
            public string _codRisorse { get; set; }
            public string _risorseGest { get; set; }
            public string _codBudget { get; set; }
            public string _budgetGest { get; set; }
            public string _codIndustry { get; set; }
            public string _industry { get; set; }
            public string _codFigProExtra { get; set; }
            public string _figProExtra { get; set; }
            //public List<VDFiguraProfCVStored> _listafigurererai { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _annoInizio { get; set; }
            public string _meseInizio { get; set; }
            public string _giornoInizio { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            public string _annoFine { get; set; }
            public string _meseFine { get; set; }
            public string _giornoFine { get; set; }

            private void SplitDataInizio()
            {
                if (String.IsNullOrWhiteSpace(_dataInizio)) return;

                if (_dataInizio.Contains('/'))
                {
                    string[] token = _dataInizio.Split('/');
                    if (token.Length == 3)
                    {
                        _giornoInizio = token[0];
                        _annoInizio = token[2];
                    }
                    else if (token.Length == 2)
                    {
                        _meseInizio = token[0];
                        _annoInizio = token[1];
                    }
                    else
                    {
                        _annoInizio = token[0];
                    }
                }
                else
                {
                    _annoInizio = _dataInizio.Substring(0, 4);
                    if (_dataInizio.Length > 4)
                        _meseInizio = _dataInizio.Substring(4, 2);
                    if (_dataInizio.Length > 6)
                        _giornoInizio = _dataInizio.Substring(6);
                }
            }
            private void SplitDataFine()
            {
                if (String.IsNullOrWhiteSpace(_dataFine)) return;

                if (_dataFine.Contains('/'))
                {
                    string[] token = _dataFine.Split('/');
                    if (token.Length == 3)
                    {
                        _giornoFine = token[0];
                        _annoFine = token[2];
                    }
                    else if (token.Length == 2)
                    {
                        _meseFine = token[0];
                        _annoFine = token[1];
                    }
                    else
                    {
                        _annoFine = token[0];
                    }
                }
                else
                {
                    _annoFine = _dataFine.Substring(0, 4);
                    if (_dataFine.Length > 4)
                        _meseFine = _dataFine.Substring(4, 2);
                    if (_dataFine.Length > 6)
                        _giornoFine = _dataFine.Substring(6);
                }
            }
            public string GetDataInizio(string separator)
            {
                string retValue = null;
                if (!String.IsNullOrWhiteSpace(_giornoInizio) && _giornoInizio!="00")
                    retValue += _giornoInizio + separator;
                if (!String.IsNullOrWhiteSpace(_meseInizio) && _meseInizio!="00")
                    retValue += _meseInizio + separator;
                if (!String.IsNullOrWhiteSpace(_annoInizio))
                    retValue += _annoInizio;
                return retValue;
            }
            public string GetDataFine(string separator)
            {
                string retValue = null;
                if (!String.IsNullOrWhiteSpace(_giornoFine) && _giornoFine!="00")
                    retValue += _giornoFine + separator;
                if (!String.IsNullOrWhiteSpace(_meseFine) && _meseFine!="00")
                    retValue += _meseFine + separator;
                if (!String.IsNullOrWhiteSpace(_annoFine))
                    retValue += _annoFine;
                return retValue;
            }

            public List<SelectListItem> GetGiorni()
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Value = "00", Text = "Giorno" });
                for (int i = 1; i <= 31; i++)
                {
                    list.Add(new SelectListItem() { Value = String.Format("{0:00}", i), Text = i.ToString() });
                }
                return list;
            }

            public List<SelectListItem> GetMesi()
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Value = "00", Text = "Mese" });

                list.Add(new SelectListItem(){ Value = "01", Text = "Gennaio" });
                list.Add(new SelectListItem(){ Value = "02", Text = "Febbraio" });
                list.Add(new SelectListItem(){ Value = "03", Text = "Marzo" });
                list.Add(new SelectListItem(){ Value = "04", Text = "Aprile" });
                list.Add(new SelectListItem(){ Value = "05", Text = "Maggio" });
                list.Add(new SelectListItem(){ Value = "06", Text = "Giugno" });
                list.Add(new SelectListItem(){ Value = "07", Text = "Luglio" });
                list.Add(new SelectListItem(){ Value = "08", Text = "Agosto" });
                list.Add(new SelectListItem(){ Value = "09", Text = "Settembre" });
                list.Add(new SelectListItem(){ Value = "10", Text = "Ottobre" });
                list.Add(new SelectListItem(){ Value = "11", Text = "Novembre" });
                list.Add(new SelectListItem(){ Value = "12", Text = "Dicembre" });
                return list;
            }
        }

        public class Certificazioni :TourReady
        {
            public string _matricola { get; set; }
            public int _prog { get; set; }
            public string _tipo { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _nomeCertifica { get; set; }
            public string _autCertifica { get; set; }
            public string _noteCertifica { get; set; }
            public string _numLicenza { get; set; }
            public string _meseIni { get; set; }
            public string _meseFin { get; set; }
            public string _annoIni { get; set; }
            public string _annoFin { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _dataIni { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _dataFin { get; set; }
            [RegularExpression(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$", ErrorMessage = "url non valido")]
            public string _urlCertifica { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _titoloPubblica { get; set; }
            public string _editorePubblica { get; set; }



            [Required(ErrorMessage="Campo obbligatorio")]
            public string _dataPubblica { get; set; }

            [Required(ErrorMessage = "Campo obbligatorio")]
            public int? AnnoPubblicazione { get; set; }
            public string MesePubblicazione { get; set; }
            public int? GiornoPubblicazione { get; set; }

            [StringLength(250, ErrorMessage = "Inserire massimo 250 caratteri")]
            //[RegularExpression(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$", ErrorMessage = "url non valido")]
            public string _urlPubblica { get; set; }
            public string _notePubblica { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _tipoBrevetto { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _uffBrevetto { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _numBrevetto { get; set; }
            public string _inventore { get; set; }
            public string _flagRegBrevetto { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _dataBrevetto { get; set; }
            [RegularExpression(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$", ErrorMessage = "url non valido")]
            public string _urlBrevetto { get; set; }
            public string _noteBrevetto { get; set; }
            public string _codAlboProf { get; set; }
            public string _descAlboProf { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _pressoAlboProf { get; set; }
            [Required(ErrorMessage="Campo obbligatorio")]
            public string _dataAlboProf { get; set; }
            public string _noteAlboProf { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public List<SelectListItem> MesiPubblicazione { get; set; }
            public List<SelectListItem> GiorniPubblicazione { get; set; }
            public string _tipoPubblicazione { get; set; }
            public string _tipoPubRiferimento { get; set; }
            public string _riferimentoPub { get; set; }
            public Certificazioni() : base(CommonHelper.GetCurrentUserMatricola())
            {
                this.MesiPubblicazione = this.getMesi();
                this.GiorniPubblicazione = this.getGiorni();

                this._tipo = "1";
            }
            public Certificazioni(string matricola) : base(matricola)
            {
                this.MesiPubblicazione = this.getMesi();
                this.GiorniPubblicazione = this.getGiorni();
               
                this._tipo = "1";
            }
            public List<SelectListItem>  getMesi()
            {
                var mp = new List<SelectListItem>();
                for (int m = 0; m < 12; m++) mp.Add(new SelectListItem() { Text = new DateTime(2018, m + 1, 1).ToString("MMMM"), Value = (m + 1).ToString() });
                return mp; 
            }
            public List<SelectListItem> getGiorni()
            {
                var gp = new List<SelectListItem>();
                for (int i = 1; i < 32; i++) gp.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
                return gp;
            }
        }

        public class Allegati
        {
            public string _matricola { get; set; }
            public int _id { get; set; }
            public string _name { get; set; }
            public string _contentType { get; set; }
            public long _size { get; set; }
            public string _pathName { get; set; }
            public int? _idBox { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _note { get; set; }

            public Allegati()
            {

            }
        }

        public class Privacy
        {
            public Dictionary<string, string> infoPrivacy { get; set; }
            public bool privacyAccepted { get; set; }
            public DateTime? privacyAcceptedAt { get; set; }
        }

        public class AutoPresentazioneBox
        {
            public string _matricola { get; set; }
            public int _id { get; set; }
            public string _name { get; set; }
            public string _contentType { get; set; }
            public long _size { get; set; }
            public string _pathName { get; set; }
            public int? _idBox { get; set; }
            public string _stato { get; set; }
            public string _tipoAgg { get; set; }
            public DateTime? _dataOraAgg { get; set; }
            public string _note { get; set; }

            public AutoPresentazioneBox()
            {

            }
        }

        public class Mappatura
        {
            public TSVPrenPrenota Prenotazione { get; set; }
            public TSVPrenSlot prenSlot { get; set; }
            public TSVPrenStanza prenStanza { get; set; }

            public IQueryable<TSVPrenSlot> ElencoSlot { get; set; }
            public IQueryable<TSVPrenStanza> ElencoStanze { get; set; }
            

            public bool CanBook { get; set; }
            public bool CanModify { get; set; }
            public bool CanCancel { get; set; }

            public string MessageCannotBook { get; set; }
        }
    }

    public class Voci
    {
        public string id { get; set; }
        public string titolo { get; set; }
    }

    public class MenuModel
    {
        public string sez { get; set; }
        public List<Voci> voci { get; set; }
    }
}