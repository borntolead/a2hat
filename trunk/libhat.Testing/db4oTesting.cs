using System;
using System.Collections.Generic;
using System.Text;
using libhat.DBFactory;
using NUnit.Framework;

namespace libhat.Testing {
    [TestFixture]
    public class db4oTesting {
        private IDBFactory factory;

        [SetUp]
        public void Configure() {
            Db4oFactory.Configure();
            factory = Db4oFactory.GetInstance();
        }

        [Test]
        public void SaveAndLookupTest() {
            HatCharacter chr = new HatCharacter();

            chr.CharacterID = 1;
            chr.CharacterData = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            chr.ParentUserID = 0xfa;
            chr.Code = "testing";

            factory.Save( chr );

            HatCharacter getChr = factory.LookupFirst<HatCharacter>( new SelectByCodeCondition( "testing" ) );

            Assert.AreEqual( chr.CharacterID, getChr.CharacterID );
            Assert.AreEqual( chr.CharacterData, getChr.CharacterData );
            Assert.AreEqual( chr.ParentUserID, getChr.ParentUserID );
        }

        [TearDown]
        public void Teardown() {
            
        }
    }
}
