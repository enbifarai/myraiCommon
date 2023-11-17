using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRai.Zip
{
	/// <summary>
	/// interfaccia per la gestione dei contenuti compressi
	/// </summary>
	public interface ICompressionDataController
	{
		/// <summary>
		/// Metodo per la compressione dei file passati
		/// </summary>
		/// <param name="filesToCompress">Lista di file formati da nome file e contenuto</param>
		/// <param name="options">Opzioni aggiuntive</param>
		/// <returns></returns>
		byte[] Compress ( List<CompressionFileItem> filesToCompress, CompressionOptions options );

		/// <summary>
		/// Metodo per la decompressione dell'archivio fornito in input
		/// </summary>
		/// <param name="compressedContent"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		List<CompressionFileItem> Decompress ( byte[] compressedContent, CompressionOptions options );
	}

	/// <summary>
	/// Opzioni aggiuntive per la compressione/decompressione
	/// </summary>
	public class CompressionOptions
	{
		/// <summary>
		/// Costruttore
		/// </summary>
		public CompressionOptions ()
		{
			this.CompressionLevel = 3;
		}
		public string Password { get; set; }
		public int CompressionLevel { get; set; }
	}

	/// <summary>
	/// Oggetto che contiene i dati del file da comprimere
	/// </summary>
	public class CompressionFileItem
	{
		/// <summary>
		/// Nome del file acquisito
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Contenuto del file
		/// </summary>
		public byte[] Content { get; set; }
	}
}