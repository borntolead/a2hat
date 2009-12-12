using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using libhat.BE;

namespace libhat {
    public static class ChecksumHelper {
        public static long AddValue(long checksum, object add) {
            long result = 0;
            if( add is byte) {
                result += addByte( checksum, (Byte) add );
            } else if ( add is Int16 ) {
                result += addInt16( checksum, (Int16)add );
            } else if ( add is Int32 ) {
                result += addInt32( checksum, (Int32)add );
            } else {
                throw new ArgumentException( );
            }

            return result;
        }
        
        private static long addByte(long checksum, Byte add) {
            long result;

            result = ( add & 0xff ) << 1;

            return result;
        }

        private static long addInt32( long checksum, Int32 add ) {
            long result=0;

            result += ( ( add & 0x000000ff ) >> 0 ) << 1;
            result += ( ( add & 0x0000ff00 ) >> 8 ) << 1;
            result += ( ( add & 0x00ff0000 ) >> 16 ) << 1;
            result += ( ( add & 0xff000000 ) >> 24 ) << 1;

            return result;
        }

        private static long addInt16( long checksum, Int16 add ) {
            long result = 0;

            result += ( ( add & 0x000000ff ) >> 0 ) << 1;
            result += ( ( add & 0x0000ff00 ) >> 8 ) << 1;
            result += ( ( add & 0x00ff0000 ) >> 16 ) << 1;

            return result;
        }

        public static Int32 GetChecksum(HatCharacter chr) {
            Int32 sum = 0;
            /*AddValue( sum, chr.MonsterKills );
            AddValue( sum, chr.PlayerKills );
            AddValue( sum, chr.TotalKills );
            AddValue( sum, chr.DeathCount );
            AddValue( sum, (int)chr.Money );
            AddValue( sum, chr.MonsterKills );
            AddValue( sum, chr.Body );
            AddValue( sum, chr.React );
            AddValue( sum, chr.Mind );
            AddValue( sum, chr.Spirit );
            AddValue( sum, chr.Spells );
            AddValue( sum, chr.ActiveSpell );
            AddValue( sum, chr.FireExp );
            AddValue( sum, chr.WaterExp );
            AddValue( sum, chr.AirExp );
            AddValue( sum, chr.EarthEx );

            sum += ( ( chr.AstralEx & 0x000000ff ) >> 0 ) << 1;
            sum += ( ( chr.AstralEx & 0x0000ff00 ) >> 8 ) << 1;
            sum += ( ( chr.AstralEx & 0x00ff0000 ) >> 16 );*/

            return sum;
        }

        public static Int32 GetStreamChecksum(Stream str) {
            Int32 result = 0;
            str.Seek( 0, SeekOrigin.Begin );

            while( str.CanRead ) {
                byte val = (byte)str.ReadByte();
                result = ( result << 1 ) + val;
            }

            return result;
        }
    }
}
