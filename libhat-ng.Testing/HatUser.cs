/*
 * Created by SharpDevelop.
 * User: Масяня
 * Date: 06.12.2009
 * Time: 18:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using libhat_ng.DB;
using libhat_ng.Entity;
using NUnit.Framework;
using System.Collections.Generic;

namespace libhat_ng.Testing {
	[TestFixture]
	public class HatUserTest
	{
		[Test]
		public void TestMethod() {
			HatUser u = new HatUser();
			
			u.Login = "test_login";
			
			IFactory<HatUser> factory = HatUserFactory.Instance();
			
			factory.Save( u );
			
			HatUser ut = factory.Load( new object() )[0];
			
			Assert.AreEqual( u.Login, ut.Login );
		}
		
		[TestFixtureSetUp]
		public void Init()
		{
			// TODO: Add Init code.
		}
		
		[TestFixtureTearDown]
		public void Dispose()
		{
			// TODO: Add tear down code.
		}
	}
}
