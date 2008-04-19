using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace libhat.Testing {
    [TestFixture]
    public class SmallTestings {
        [Test]
        public void EndPointTest() {
            string str = "127.0.0.1:8001";
            string[] arr = str.Split( ':' );

            EndPoint ep = new IPEndPoint(IPAddress.Parse( arr[0] ), Int32.Parse( arr[1]) );

            Debug.Print( ep.ToString() );
        }
    }
}
