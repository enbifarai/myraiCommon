using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace myRai.Zip
{
	public class SharpZipLibCompressionDataController : ICompressionDataController
	{
		#region Public

		/// <summary>
		/// Metodo per la compressionde dei file
		/// </summary>
		/// <param name="filesToCompress"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public byte[] Compress ( List<CompressionFileItem> filesToCompress, CompressionOptions options )
		{
			try
			{
				byte[] result = null;

				if ( filesToCompress == null )
					throw new Exception( "Non ci sono files da comprimere" );

				if ( options == null )
				{
					options = new CompressionOptions();
				}

				using ( MemoryStream ms = new MemoryStream() )
				{
					using ( ZipOutputStream zipStream = new ZipOutputStream( ms ) )
					{
						zipStream.SetLevel( options.CompressionLevel );

						if ( !String.IsNullOrEmpty( options.Password ) )
							zipStream.Password = options.Password;

						filesToCompress.ForEach( f =>
						{
							string entryName = f.FileName;

							ZipEntry newEntry = new ZipEntry( entryName );

							newEntry.DateTime = DateTime.Now;
							newEntry.Size = f.Content.Length;
							zipStream.PutNextEntry( newEntry );

							byte[] buffer = new byte[f.Content.Length];
							using ( MemoryStream streamReader = new MemoryStream( f.Content ) )
								StreamUtils.Copy( streamReader, zipStream, buffer );

							zipStream.CloseEntry();
						} );

						zipStream.IsStreamOwner = true;
						zipStream.Close();
					}

					result = ms.ToArray();
				}

				return result;
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		public List<CompressionFileItem> Decompress ( byte[] compressedContent, CompressionOptions options )
		{
			try
			{
				List<CompressionFileItem> result = new List<CompressionFileItem>();

				if ( compressedContent == null )
					throw new Exception( "Non ci sono files da decomprimere" );

				if ( options == null )
				{
					options = new CompressionOptions();
				}

				ZipFile zf = null;
				using ( MemoryStream fs = new MemoryStream( compressedContent, 0, compressedContent.Length ) )
				{
					zf = new ZipFile( fs );

					if ( !String.IsNullOrEmpty( options.Password ) )
					{
						zf.Password = options.Password;
					}
					foreach ( ZipEntry zipEntry in zf )
					{
						String entryFileName = zipEntry.Name;

						byte[] buffer = new byte[4096];
						Stream zipStream = zf.GetInputStream( zipEntry );

						using ( MemoryStream ms = new MemoryStream() )
						{
							StreamUtils.Copy( zipStream, ms, buffer );
							ms.Flush();
							result.Add( new CompressionFileItem()
							{
								Content = ms.ToArray(),
								FileName = zipEntry.Name
							} );
						}
					}
					zf.IsStreamOwner = true; // Makes close also shut the underlying stream
					zf.Close(); // Ensure we release resources
				}
				return result;
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

		#endregion
	}
}