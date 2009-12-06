/*
 * Created by SharpDevelop.
 * User: Масяня
 * Date: 06.12.2009
 * Time: 17:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using BerkeleyDB;
using libhat_ng.Entity;

namespace libhat_ng.DB
{
	/// <summary>
	/// Description of HatUserFactory.
	/// </summary>
	public class HatUserFactory : IFactory<HatUser>, IDisposable
	{		
		static HatUserFactory factory;
		Database db;		
		
		protected HatUserFactory(){
			string path;
			
			try{
				path = System.Configuration.ConfigurationSettings.AppSettings["db_path"];
			} catch( Exception ex ) {
				throw new System.IO.FileNotFoundException ("db_path setting not found", ex );				
			}
			
			try{
				HashDatabaseConfig cfg = new HashDatabaseConfig();
				cfg = new HashDatabaseConfig();
	            cfg.Duplicates = DuplicatesPolicy.NONE;
	            cfg.ErrorPrefix = "HatUserFactoryError_";
	            cfg.Creation = CreatePolicy.IF_NEEDED;
	            cfg.CacheSize = new CacheInfo(0, 64 * 1024, 1);
	            cfg.PageSize = 8 * 1024;
				
	            db = HashDatabase.Open( path, this.GetType().Name ,cfg);
			} catch ( DatabaseException ex ) {
				Console.WriteLine( ex.Message );
				throw;
			}
		}
		
		public static HatUserFactory Instance(){
			lock( factory ) {
				if( factory == null ) factory = new HatUserFactory();
			}
			
			return factory;
		}
		
		public void Save(HatUser obj)
		{
			if( db == null ) throw new ArgumentNullException("database not initialized");
				
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream mem = new MemoryStream() ){
				bf.Serialize( mem, obj );
				
				db.Put( new DatabaseEntry(System.Text.Encoding.Unicode.GetBytes(obj.Login)), new DatabaseEntry(mem.ToArray()));
			}
		}
		
		public IList<HatUser> Load(object criteria)
		{
			//TODO: criteria;
			BinaryFormatter bf = new BinaryFormatter();
			Cursor c = db.Cursor();
			List<HatUser> users = new List<HatUser>();
			
			while( c.MoveNext() ) {
				using( MemoryStream mem = new MemoryStream( c.Current.Value.Data ) ){
					HatUser user = bf.Deserialize( mem ) as HatUser;
					
					if( user == null ) continue;
					
					users.Add( user );
				}
			}
			
			return users;
		}
		
		public HatUser LoadOne(object criteria)
		{
			throw new NotImplementedException();
		}		
		
		
		
		public void Dispose()
		{
			db.Close();
			db.Dispose();
		}
				
		public void Remove(HatUser obj)
		{
			throw new NotImplementedException();
		}
	}
}
