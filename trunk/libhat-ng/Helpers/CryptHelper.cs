using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace libhat {
    public static class CryptHelper {
        public static Stream CharacterStreamCrypt( Stream str, Int32 key) {
            Int32 nowKey = key, oldKey = key;
            byte[] ret = new byte[str.Length];

            nowKey &= 0xffff;
            nowKey = ( nowKey >> 0x10 ) | nowKey;

            str.Seek( 0, SeekOrigin.Begin );
            MemoryStream mem = new MemoryStream( ret );
            while ( str.CanRead ) {
                byte val = (byte)str.ReadByte();
                mem.WriteByte( (byte) ( ( nowKey >> 0x10 ) ^ val ) );
                nowKey <<= 1;

                if( (str.Position & 0xF) == 0xF) {
                    nowKey = nowKey | ( oldKey & 0xffff );
                }
            }
            
            mem.Seek( 0, SeekOrigin.Begin );

            return mem;
        }
    }
}
