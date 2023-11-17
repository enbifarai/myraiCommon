using System;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace myRaiHelper
{
    public class WSDigigappDataController
	{
		public WSDigigappDataController ()
		{
		}

        public dayResponse GetEccezioniForBatch(string matricola, string data, string options, int livelloUtente)
        {
            try
            {
                WSDigigapp service = new WSDigigapp( )
                {
                    Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( myRaiHelper.EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( myRaiHelper.EnumParametriSistema.AccountUtenteServizio )[1] )
                };
                return service.getEccezioni(matricola, data, options, livelloUtente);
            }
            catch (Exception ex)
            {
                string msg = String.Format("WSDigigapp - GetEccezioniForBatch - Parametri: matricola = {0}, data = {1}, options = {2}, livelloUtente = {3} \n", matricola, data, options, livelloUtente);
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = matricola,
                    error_message = msg + ex.ToString(),
                    provenienza = "WSDigigappDataController.GetEccezioniForBatch"
                });
                return null;
            }
        }

        public dayResponse GetEccezioni (string currentMatricola, string matricola, string data, string options, int livelloUtente )
		{
			try
			{
                matricola = matricola.PadLeft(7, '0');

                WSDigigappDataControllerScope.Instance.GetEccezioniData = new GetEccezioniData();

                WSDigigapp service = new WSDigigapp( )
                {
                    Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( myRaiHelper.EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( myRaiHelper.EnumParametriSistema.AccountUtenteServizio )[1] )
                };

                return service.getEccezioni(matricola, data, options, livelloUtente);

            }
			catch ( Exception ex )
			{
				string msg = String.Format( "WSDigigapp - GetEccezioni - Parametri: matricola = {0}, data = {1}, options = {2}, livelloUtente = {3} \n", matricola, data, options, livelloUtente );
				Logger.LogErrori( new myRaiData.MyRai_LogErrori()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					matricola = currentMatricola,
					error_message = msg + ex.ToString(),
					provenienza = "WSDigigappDataController.GetEccezioni"
				} );
				return null;
			}
		}

		public Eccezione[] ListaEccezioni (string currentMatricola, string matricola, string data, string area, int livelloUtente )
		{
			try
			{
                matricola = matricola.PadLeft(7, '0');

				//if ( WSDigigappDataControllerScope.Instance.ListaEccezioniData != null )
				//{
				//	if ( WSDigigappDataControllerScope.Instance.ListaEccezioniData.HasData() &&
				//		WSDigigappDataControllerScope.Instance.ListaEccezioniData.IsEqualsTo( matricola, data, area, livelloUtente ) )
				//	{
				//		return WSDigigappDataControllerScope.Instance.ListaEccezioniData.GetResponse();
				//	}
				//}
				//else
				//{
				//	WSDigigappDataControllerScope.Instance.ListaEccezioniData = new ListaEccezioniData();
				//}

                WSDigigappDataControllerScope.Instance.ListaEccezioniData = new ListaEccezioniData();

                WSDigigapp service = new WSDigigapp( )
                {
                    Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>( myRaiHelper.EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( myRaiHelper.EnumParametriSistema.AccountUtenteServizio )[1] )
                };

                //WSDigigappDataControllerScope.Instance.ListaEccezioniData.SetKeyResponse(matricola, data, area, livelloUtente);
                //WSDigigappDataControllerScope.Instance.ListaEccezioniData.SetValueResponse(service.listaEccezioni( matricola, data, area, livelloUtente ) );

                //return WSDigigappDataControllerScope.Instance.ListaEccezioniData.GetResponse();
                return service.listaEccezioni(matricola, data, area, livelloUtente);

            }
			catch ( Exception ex )
			{
				string msg = String.Format( "WSDigigapp - ListaEccezioni - Parametri: matricola = {0}, data = {1}, area = {2}, livelloUtente = {3} \n", matricola, data, area, livelloUtente );
				Logger.LogErrori( new myRaiData.MyRai_LogErrori()
				{
					applicativo = "PORTALE",
					data = DateTime.Now,
					matricola = currentMatricola,
					error_message = msg + ex.ToString(),
					provenienza = "WSDigigappDataController.GetEccezioni"
				} );
				return null;
			}
		}

		public void ClearGetEccezioni ()
		{
			if ( WSDigigappDataControllerScope.Instance.GetEccezioniData != null )
			{
				WSDigigappDataControllerScope.Instance.GetEccezioniData.Clear();
			}
		}

		public void ClearListaEccezioni ()
		{
			if ( WSDigigappDataControllerScope.Instance.ListaEccezioniData != null )
			{
				WSDigigappDataControllerScope.Instance.ListaEccezioniData.Clear();
			}
		}

		public void ClearAll ()
		{
			if ( WSDigigappDataControllerScope.Instance.GetEccezioniData != null )
			{
				WSDigigappDataControllerScope.Instance.GetEccezioniData.Clear();
			}
			if ( WSDigigappDataControllerScope.Instance.ListaEccezioniData != null )
			{
				WSDigigappDataControllerScope.Instance.ListaEccezioniData.Clear();
			}
		}
    }

	#region Classi scope

	public class WSDigigappDataControllerScope : SessionScope<WSDigigappDataControllerScope>
    {
		public WSDigigappDataControllerScope ()
		{
		}

		public GetEccezioniData GetEccezioniData { get; set; }
		public ListaEccezioniData ListaEccezioniData { get; set; }

		public void Clear ()
		{
			this.GetEccezioniData.Clear();
			this.ListaEccezioniData.Clear();
		}
	}

	#endregion

	#region Definizioni Classi

	public class GetEccezioniData
	{
		private string Matricola { get; set; }
		private string Data { get; set; }
		private string Options { get; set; }
		private int LivelloUtente { get; set; }
		private DateTime? DataUltimoAggiornamento { get; set; }
		private dayResponse Response { get; set; }

        private bool IsWriting { get; set; }

		public bool HasData ()
		{
			if ( this.DataUltimoAggiornamento.HasValue )
			{
				DateTime current = DateTime.Now;

				double sec = ( current - this.DataUltimoAggiornamento.Value ).TotalSeconds;

				if ( sec > 5 )
					return false;
				else
					return true;
			}
			else
			{
				return false;
			}
		}

        public bool IsEqualsTo(string matricola, string data, string options)
        {
            if (this.Matricola.Trim().Equals(matricola.Trim()) &&
                this.Data.Trim().Equals(data.Trim()) &&
                this.Options.Trim().Equals(options.Trim()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private byte[] ToByteArray<T> ( T obj )
		{
			if ( obj == null )
				return null;

			BinaryFormatter bf = new BinaryFormatter();
			using ( MemoryStream ms = new MemoryStream() )
			{
				bf.Serialize( ms, obj );
				return ms.ToArray();
			}
		}

		private T FromByteArray<T> ( byte[] data )
		{
			if ( data == null )
				return default( T );
			BinaryFormatter bf = new BinaryFormatter();
			using ( MemoryStream ms = new MemoryStream( data ) )
			{
				object obj = bf.Deserialize( ms );
				return ( T )obj;
			}
		}

		public dayResponse GetResponse ()
		{
            while (this.IsWriting) { }

			byte[] clone = ToByteArray( this.Response );

			dayResponse toReturn = FromByteArray<dayResponse>( clone );

			return toReturn;
		}

        public void SetKeyResponse(string matricola, string data, string options, int livelloUtente)
        {
            this.Matricola = matricola;
			this.Data = data;
			this.Options = options;
			this.LivelloUtente = livelloUtente;
            this.DataUltimoAggiornamento = DateTime.Now;

            //this.IsWriting = true;
        }
        public void SetValueResponse(dayResponse value)
        {
            this.Response = value;

            this.IsWriting = false;
        }


        public void SetResponse ( string matricola, string data, string options, int livelloUtente, dayResponse value )
		{
			this.Matricola = matricola;
			this.Data = data;
			this.Options = options;
			this.LivelloUtente = livelloUtente;
			this.Response = value;
            this.DataUltimoAggiornamento = DateTime.Now;
            //Debug.Print("SET " +this.Response.eccezioni.Count().ToString());
		}

		public void Clear ()
		{
			this.Matricola = String.Empty;
			this.Data = String.Empty;
			this.Matricola = String.Empty;
			this.Options = String.Empty;
			this.LivelloUtente = 0;
			this.DataUltimoAggiornamento = null;
			this.Response = null;
		}
	}

	public class ListaEccezioniData
	{
		private string Matricola { get; set; }
		private string Data { get; set; }
		private string Area { get; set; }
		private int LivelloUtente { get; set; }
		private DateTime? DataUltimoAggiornamento { get; set; }
		private Eccezione[] Response { get; set; }

        private bool IsWriting { get; set; }

		public bool HasData ()
		{
			if ( this.DataUltimoAggiornamento.HasValue )
			{
				DateTime current = DateTime.Now;

				double sec = ( current - this.DataUltimoAggiornamento.Value ).TotalSeconds;

				if ( sec > 5.0 )
					return false;
				else
					return true;
			}
			else
			{
				return false;
			}
		}

		public bool IsEqualsTo ( string matricola, string data, string area, int livelloUtente )
		{
			if ( this.Matricola.Trim().Equals( matricola.Trim() ) &&
				this.Data.Trim().Equals( data.Trim() ) &&
				this.Area.Trim().Equals( area.Trim() ) &&
				this.LivelloUtente.Equals( livelloUtente ) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public Eccezione[] GetResponse ()
		{
            while (this.IsWriting) {  }

			return this.Response;
		}

        public void SetKeyResponse(string matricola, string data, string area, int livelloUtente)
        {
            this.Matricola = matricola;
            this.Data = data;
            this.Area = area;
            this.LivelloUtente = livelloUtente;
            this.DataUltimoAggiornamento = DateTime.Now;

            //this.IsWriting = true;
        }
        public void SetValueResponse(Eccezione[] value)
        {
            this.Response = value;
            this.IsWriting = false;
        }

        public void SetResponse ( string matricola, string data, string area, int livelloUtente, Eccezione[] value )
		{
			this.Matricola = matricola;
			this.Data = data;
			this.Area = area;
			this.LivelloUtente = livelloUtente;
			this.Response = value;
			this.DataUltimoAggiornamento = DateTime.Now;
		}

		public void Clear ()
		{
			this.Matricola = String.Empty;
			this.Data = String.Empty;
			this.Matricola = String.Empty;
			this.Area = String.Empty;
			this.LivelloUtente = 0;
			this.DataUltimoAggiornamento = null;
			this.Response = null;
		}
	}

	#endregion
}