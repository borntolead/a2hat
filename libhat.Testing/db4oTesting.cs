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
            chr.Code = "testing";

            factory.Save( chr );

            HatCharacter getChr = factory.LookupFirst<HatCharacter>( new SelectByCodeCondition( "testing" ) );

            Assert.AreEqual( chr.CharacterID, getChr.CharacterID );
            Assert.AreEqual( chr.CharacterData, getChr.CharacterData );
            Assert.AreEqual( chr.ParentUserCode, getChr.ParentUserCode );
        }

        [Test]
        public void ListAndLookupTest() {
            HatUser usr = new HatUser();
            usr.Code = "newCharacter";
            usr.IsLocked = false;
            usr.Login = "login1";
            usr.Password = "login1";
            usr.UserLoggedIn = false;

            HatCharacter chr = new HatCharacter();

            chr.CharacterID = 1;
            chr.CharacterData = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            chr.ParentUser = usr;
            chr.Code = "testing1";

            factory.Save( usr );
            factory.Save( chr );

            HatUser getUsr = factory.LookupFirst<HatUser>( new SelectByCodeCondition( "login1" ) );

            HatCharacter getChr = getUsr.CharacterList[0];

            Assert.AreEqual( chr.CharacterID, getChr.CharacterID );
            Assert.AreEqual( chr.CharacterData, getChr.CharacterData );
            Assert.AreEqual( chr.ParentUser, getChr.ParentUser );
        }

        [TearDown]
        public void Teardown() {
            factory.Dispose();
        }
    }
}
