using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web.UI.WebControls;
using myRaiCommonModel.raiplace;
using myRaiHelper;

namespace myRai.Controllers.RaiPlace
{
    public class StampaCestiniDataController
	{
		public byte[] StampaPdf ( Ordine ordine, string imgPath )
		{
			try
			{
				byte[] bytes = null;
				int pages = 1;

				int currentY = 750;
				const int lStartX = 25;
				const int fontSize = 10;

				int border = 0; // none
				int textAlign = 0; // left

				BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED );
				Font myFont = new Font( bf, fontSize, Font.NORMAL );
				Font myFontBold = new Font( bf, fontSize, Font.BOLD );

				if ( CestiniControllerScope.Instance.Cestino == null )
					throw new Exception( "Si è verificato un errore durante la creazione del pdf.\nDati di riepilogo non trovati." );

				string nOrdine = ordine.codiceOrdine;

				//Ordine ordine = CestiniControllerScope.Instance.Cestino.ordine;

				using ( MemoryStream ms = new MemoryStream() )
				{
					// Creazione dell'istanza del documento, impostando tipo di pagina e margini
					Document document = new Document( PageSize.A4, 25, 25, 30, 30 );
					PdfWriter writer = PdfWriter.GetInstance( document, ms );

					writer.PageEvent = new ITextEvents( ordine, imgPath );
					document.Open();

					PdfContentByte cb = writer.DirectContent;

					currentY = ( ( ITextEvents )writer.PageEvent ).CurrentY;

					// Scrittura dell'intestazione della tabella contenente le singole richieste di cestino
					currentY = this.WriteIntestazioneTabellaCestini( cb, document, currentY );

					PdfPTable tableCestini = new PdfPTable( 3 );
					tableCestini.DefaultCell.BorderWidth = 1;
					tableCestini.TotalWidth = document.PageSize.Width - 50;
					tableCestini.LockedWidth = true;
					int[] cestiniWidths = new int[] { 220, 80, 200 };
					tableCestini.SetWidths( cestiniWidths );

					List<Richiesta> richieste = CestiniControllerScope.Instance.Cestino.richieste;

					if ( richieste != null &&
						richieste.Any() )
					{
						int printedRows = 0;
						int tablePosition = currentY;
						int elementCounter = 0;
						richieste.ForEach( r =>
						{
							elementCounter++;
							if ( currentY > 100 )
							{
								string nominativo = "";
								if ( String.IsNullOrEmpty( r.matricolaRisorsa ) )
								{
									nominativo = String.Format( "{0} {1}", r.cognomeRisorsa, r.nomeRisorsa );
								}
								else
								{
									nominativo = String.Format( "{0} {1} ({2})", r.cognomeRisorsa, r.nomeRisorsa, r.matricolaRisorsa );
								}

								tableCestini.AddCell( this.WriteCell( nominativo.ToUpper(), border, 1, textAlign, myFont ) );
								tableCestini.AddCell( this.WriteCell( r.tipoCestino.GetDescription(), border, 1, textAlign, myFont ) );
								tableCestini.AddCell( this.WriteCell( r.codiceRichiesta, border, 1, textAlign, myFont ) );
								printedRows++;
								currentY -= 12; // carattere 10 + offset
							}
							else
							{
								//tableCestini.FlushContent();
								tableCestini.WriteSelectedRows( 0, ( tableCestini.Rows.Count + 1 ), lStartX, tablePosition, cb );
								document.NewPage();
								printedRows = 0;

								currentY = ( ( ITextEvents )writer.PageEvent ).CurrentY;
								tableCestini.DeleteBodyRows();

								// Se non ha finito di stampare le richieste allora ristampa l'intestazione della tabella
								// Scrittura dell'intestazione della tabella contenente le singole richieste di cestino
								if (elementCounter != richieste.Count())
									currentY = this.WriteIntestazioneTabellaCestini( cb, document, currentY );
							}
						} );

						tableCestini.WriteSelectedRows( 0, ( tableCestini.Rows.Count + 1 ), lStartX, tablePosition, cb );

						// riepilogo
						PdfPTable tableRiepilogo = new PdfPTable( 5 );
						tableRiepilogo.DefaultCell.BorderWidth = 1;
						tableRiepilogo.TotalWidth = document.PageSize.Width - 50;
						//fix the absolute width of the table
						tableRiepilogo.LockedWidth = true;
						int[] riepilogoWidths = new int[] { 80, 80, 10, 80, 250 };
						tableRiepilogo.SetWidths( riepilogoWidths );

						tableRiepilogo.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

						//currentY -= ( int )tableCestini.CalculateHeights();

						var distinctList = richieste.GroupBy( t => t.tipoCestino )
												.Select( g => g.First() )
												.ToList();

						if ( distinctList != null )
						{

							foreach ( var d in distinctList )
							{
								var count = richieste.Where( m => m.tipoCestino.Equals( d.tipoCestino ) ).Count();

								tableRiepilogo.AddCell( this.WriteCell( "Tipologia: ", border, 1, textAlign, myFontBold ) );
								tableRiepilogo.AddCell( this.WriteCell( d.tipoCestino.GetDescription(), border, 1, textAlign, myFont ) );
								tableRiepilogo.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
								tableRiepilogo.AddCell( this.WriteCell( "Quantita: ", border, 1, textAlign, myFontBold ) );
								tableRiepilogo.AddCell( this.WriteCell( count.ToString(), border, 1, textAlign, myFont ) );
							}
						}

						tableRiepilogo.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );
						tableRiepilogo.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );
						tableRiepilogo.AddCell( this.WriteCell( "Totale pasti: ", border, 1, textAlign, myFontBold ) );
						tableRiepilogo.AddCell( this.WriteCell( richieste.Count.ToString(), border, 1, textAlign, myFont ) );
						tableRiepilogo.AddCell( this.WriteCell( " ", border, 3, textAlign, myFont ) );


						int riepilogoHeight = (int)tableRiepilogo.CalculateHeights();

						if ( ( currentY - riepilogoHeight ) <= 100 )
						{
							document.NewPage();
							currentY = tablePosition;
						}

						tableRiepilogo.WriteSelectedRows( 0, ( tableRiepilogo.Rows.Count + 1 ), lStartX, currentY, cb );
					}

					document.Close();
					writer.Close();
					bytes = ms.ToArray();
					return bytes;
				}
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		private int WriteIntestazioneTabellaCestini ( PdfContentByte cb, Document document, int currentY )
		{
			const int fontSize = 10;
			const int lStartX = 25;

			int border = 0; // none
			int textAlign = 0; // left

			BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED );
			Font myFont = new Font( bf, fontSize, Font.NORMAL );
			Font myFontBold = new Font( bf, fontSize, Font.BOLD );

			PdfPTable tableCestini = new PdfPTable( 3 );
			tableCestini.DefaultCell.BorderWidth = 1;
			tableCestini.TotalWidth = document.PageSize.Width - 50;
			//fix the absolute width of the table
			tableCestini.LockedWidth = true;
			int[] cestiniWidths = new int[] { 220, 80, 200 };
			tableCestini.SetWidths( cestiniWidths );
			tableCestini.AddCell( this.WriteCell( "Destinatari pasti", border, 3, 1, myFontBold ) );
			tableCestini.AddCell( this.WriteCell( " ", border, 3, 1, myFont ) );

			tableCestini.AddCell( this.WriteCell( "Destinatario ", border, 1, textAlign, myFontBold ) );
			tableCestini.AddCell( this.WriteCell( "Tipologia ", border, 1, textAlign, myFontBold ) );
			tableCestini.AddCell( this.WriteCell( "Codice ", border, 1, textAlign, myFontBold ) );
			tableCestini.AddCell( this.WriteCell( " ", border, 3, 1, myFont ) );
			tableCestini.WriteSelectedRows( 0, ( tableCestini.Rows.Count + 1 ), lStartX, currentY, cb );

			currentY = currentY - ( int )tableCestini.CalculateHeights();
			tableCestini.FlushContent();
			return currentY;
		}

		private PdfPCell WriteCell ( string text, int border, int colspan, int textAlign, Font f )
		{
			PdfPCell cell = new PdfPCell( new Phrase( text, f ) );
			cell.Border = border;
			cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
			cell.Colspan = colspan;
			return cell;
		}

		private PdfPCell WriteLine ( int border, int textAlign )
		{
			Chunk c = new Chunk( new iTextSharp.text.pdf.draw.LineSeparator( 0.0F, 530.0F, BaseColor.YELLOW, textAlign, 1 ) );
			PdfPCell cellSeparator = new PdfPCell( new Phrase( c ) );
			cellSeparator.Border = border;
			cellSeparator.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
			return cellSeparator;
		}
	}

	public class ITextEvents : PdfPageEventHelper
	{
		Ordine _ordine;

		string _imgPath;

		public ITextEvents ( Ordine ordine, string imgPath )
		{
			this._imgPath = imgPath;
			this._ordine = ordine;
		}

		// This is the contentbyte object of the writer
		PdfContentByte cb;

		// we will put the final number of pages in a template
		PdfTemplate headerTemplate, footerTemplate;

		// This keeps track of the creation time
		DateTime PrintTime = DateTime.Now;

		#region Fields
		private string _header;
		#endregion

		#region Properties
		public string Header
		{
			get { return _header; }
			set { _header = value; }
		}

		public int CurrentY { 
			get {
				return this.currentY;
			}
			set
			{
				this.currentY = value;
			}
		}
		#endregion

		int currentY = 750;
		const int lStartX = 25;
		const int fontSize = 10;
		BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED );


		private int WriteHeader ( PdfWriter writer, Document document )
		{
			// Creazione del PdfContentByte per il posizionamento esatto degli elementi sul documento
			PdfContentByte cb = writer.DirectContent;

			footerTemplate = cb.CreateTemplate( 50, 50 );
			int intestazioneHeight = this.WriteIntestazione( cb, document, _ordine );

			headerTemplate = cb.CreateTemplate( 500, intestazioneHeight );
			cb.AddTemplate( headerTemplate, document.LeftMargin, document.PageSize.GetTop( document.TopMargin ) );

			return currentY - intestazioneHeight;
		}

		public override void OnStartPage ( PdfWriter writer, Document document )
		{
			// Creazione del PdfContentByte per il posizionamento esatto degli elementi sul documento
			PdfContentByte cb = writer.DirectContent;

			footerTemplate = cb.CreateTemplate( 50, 50 );
			int intestazioneHeight = this.WriteIntestazione( cb, document, _ordine );

			headerTemplate = cb.CreateTemplate( 500, intestazioneHeight );
			cb.AddTemplate( headerTemplate, document.LeftMargin, document.PageSize.GetTop( document.TopMargin ) );
		}
		
		public override void OnOpenDocument ( PdfWriter writer, Document document )
		{
			try
			{
				// Aggiunta metadata al documento
				document.AddAuthor( "digiGapp" );

				document.AddCreator( "digiGapp con l'ausilio di iTextSharp" );

				document.AddKeywords( "PDF report ordine" );

				document.AddSubject( "Documento riepilogativo" );

				document.AddTitle( String.Format( "Riepilogo ordine" ) );
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		public override void OnEndPage ( PdfWriter writer, Document document )
		{
			base.OnEndPage( writer, document );

			int pageN = writer.PageNumber;
			String text = "Pagina " + pageN.ToString() + " di ";

			float len = bf.GetWidthPoint( text, fontSize );

			iTextSharp.text.Rectangle pageSize = document.PageSize;

			cb = writer.DirectContent;

			cb.SetRGBColorFill( 100, 100, 100 );

			cb.BeginText();
			cb.SetFontAndSize( bf, fontSize );
			cb.SetTextMatrix( document.LeftMargin, document.PageSize.GetBottom( document.BottomMargin ) );
			cb.ShowText( text );

			cb.EndText();
			footerTemplate = cb.CreateTemplate( 50, 50 );
			cb.AddTemplate( footerTemplate, document.LeftMargin + len, document.PageSize.GetBottom( document.BottomMargin ) );
		}

		public override void OnCloseDocument ( PdfWriter writer, Document document )
		{
			base.OnCloseDocument( writer, document );

			footerTemplate.BeginText();
			footerTemplate.SetFontAndSize( bf, fontSize );
			footerTemplate.SetTextMatrix( 0, 0 );
			footerTemplate.ShowText( "" + ( writer.PageNumber ) );
			footerTemplate.EndText();
		}

		private int WriteIntestazione ( PdfContentByte cb, Document document, Ordine ordine )
		{
			int _currentY = 750;
			const int lStartX = 25;
			const int fontSize = 10;

			BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED );
			Font myFont = new Font( bf, fontSize, Font.NORMAL );
			Font myFontBold = new Font( bf, fontSize, Font.BOLD );

			Font titleFont = new Font( bf, 14, Font.BOLD );

			int border = 0; // none
			int textAlign = 0; // left

			// disegno del logo
			//iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance( Server.MapPath( "~/assets/img/rai.png" ) );
			iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance( _imgPath );
			png.ScaleAbsolute( 45, 45 );
			png.SetAbsolutePosition( 25, 750 );
			cb.AddImage( png );

			PdfPTable table = new PdfPTable( 5 );
			table.DefaultCell.Border = Rectangle.NO_BORDER;
			table.DefaultCell.BorderWidth = 0;
			table.TotalWidth = document.PageSize.Width - 50;
			table.LockedWidth = true;
			int[] widths = new int[] { 100, 140, 20, 100, 140 };
			table.SetWidths( widths );

			table.AddCell( this.WriteCell( "Richiesta pasti in catering ", border, 5, 1, titleFont ) );
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 1
			table.AddCell( this.WriteCell( "Identificativo: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.codiceOrdine, border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "Data inserimento: ", border, 1, textAlign, myFontBold ) );

			// controlla se la data di inserimento è valorizzata, poichè il campo non è nullable il controllo
			// viene effettuato sul valore minimo che può assumere un campo di tipo datetime
			if ( ordine.dataInserimento.Equals( DateTime.MinValue ) )
			{
				table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			}
			else
			{
				table.AddCell( this.WriteCell( ordine.dataInserimento.ToString( "dd/MM/yyyy HH:mm:ss" ), border, 1, textAlign, myFont ) );
			}

			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 2
			table.AddCell( this.WriteCell( "Riferimenti: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.telefonoReferente, border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );

			// Linea
			table.AddCell( this.WriteLine( border, textAlign ) );

			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 3
			table.AddCell( this.WriteCell( "Pasto: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.tipoPasto.ToUpper(), border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "Cespite: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.luogoConsegna, border, 1, textAlign, myFont ) );
			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 4
			table.AddCell( this.WriteCell( "Data consegna: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.dataOraPasto.ToString( "dd/MM/yyyy" ), border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "Orario di consegna: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.dataOraPasto.ToString( "HH:mm:ss" ), border, 1, textAlign, myFont ) );
			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 5
			table.AddCell( this.WriteCell( "Luogo di consegna: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.luogoConsegna, border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "Referente Rai per la consegna: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.referenteConsegna, border, 1, textAlign, myFont ) );

			// Riga 6
			table.AddCell( this.WriteCell( "Note: ", border, 5, textAlign, myFontBold ) );

			// Riga 7
			table.AddCell( this.WriteCell( ordine.note, border, 5, textAlign, myFont ) );

			// Linea
			table.AddCell( this.WriteLine( border, textAlign ) );

			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 8
			table.AddCell( this.WriteCell( "Matricola richiedente: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.matricolaOrdine, border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "Nominativo richiedente: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.richiedente, border, 1, textAlign, myFont ) );
			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 9
			table.AddCell( this.WriteCell( "Struttura: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.struttura, border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "Telefono: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.telefonoReferente, border, 1, textAlign, myFont ) );
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );
			// Riga 10
			table.AddCell( this.WriteCell( "Motivo della richiesta: ", border, 5, textAlign, myFontBold ) );

			// Riga 11
			table.AddCell( this.WriteCell( ordine.motivoOrdine, border, 5, textAlign, myFont ) );

			// Linea
			table.AddCell( this.WriteLine( border, textAlign ) );

			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			// Riga 12
			table.AddCell( this.WriteCell( "Titolo: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.titoloProduzione, border, 4, textAlign, myFont ) );
			table.AddCell( this.WriteCell( "UORG: ", border, 1, textAlign, myFontBold ) );
			table.AddCell( this.WriteCell( ordine.centroCosto, border, 4, textAlign, myFont ) );
			// Linea
			table.AddCell( this.WriteLine( border, textAlign ) );
			// interlinea
			table.AddCell( this.WriteCell( " ", border, 5, textAlign, myFont ) );

			table.WriteSelectedRows( 0, table.Rows.Count + 1, lStartX, _currentY, cb );

			_currentY = _currentY - ( int )table.CalculateHeights();

			this.CurrentY = _currentY;

			return ( int )table.CalculateHeights();
		}

		private PdfPCell WriteCell ( string text, int border, int colspan, int textAlign, Font f )
		{
			PdfPCell cell = new PdfPCell( new Phrase( text, f ) );
			cell.Border = border;
			cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
			cell.Colspan = colspan;
			return cell;
		}

		private PdfPCell WriteLine ( int border, int textAlign )
		{
			Chunk c = new Chunk( new iTextSharp.text.pdf.draw.LineSeparator( 0.0F, 530.0F, BaseColor.YELLOW, textAlign, 1 ) );
			PdfPCell cellSeparator = new PdfPCell( new Phrase( c ) );
			cellSeparator.Border = border;
			cellSeparator.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
			return cellSeparator;
		}

	}
}