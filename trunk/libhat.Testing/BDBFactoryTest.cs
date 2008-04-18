using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using libhat.DBFactory;
using NUnit.Framework;

namespace libhat.Testing {
    [TestFixture]
    public class BDBFactoryTest {
        private IDBFactory factory;

        [SetUp]
        public void Setup() {
            BDBFactory.Configure( "c:\\", "test.db" );

            factory = BDBFactory.GetInstance();
        }

        [Test]
        public void CharacterSaveTest() {
            HatCharacter chr = new HatCharacter();

            chr.CharacterID = 1;
            chr.CharacterData = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            chr.ParentUserID = 0xfa;

            factory.Save( chr );

            BinaryFormatter frm = new BinaryFormatter( );
            using( MemoryStream mem = new MemoryStream( )) {
                frm.Serialize( mem, chr);

                mem.Seek( 0, SeekOrigin.Begin);

                HatCharacter c = (HatCharacter)frm.Deserialize( mem );
            }
            HatCharacter getChr = factory.LookupFirst<HatCharacter>( new SelectByCodeCondition( 1 ) );

            Assert.AreEqual( chr.CharacterID, getChr.CharacterID);
            Assert.AreEqual( chr.CharacterData, getChr.CharacterData );
            Assert.AreEqual( chr.ParentUserID, getChr.ParentUserID );
        }

        [TearDown]
        public void Teardown() {
            factory.Dispose();
        }
    }
}
