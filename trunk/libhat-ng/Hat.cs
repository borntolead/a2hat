using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using libhat_ng.Entity;
using libhat_ng.Helpers;

namespace libhat_ng
{
    public class Hat
    {
        public static ManualResetEvent done = new ManualResetEvent(false);
        public EndPoint Address{ get; set; }
        public static HatConfiguration configuration = new HatConfiguration(); 
        public void Start()
        {
            try
            {
                var listener = NetworkHelper.GetNewIPv4Socket(Address);
                listener.Listen(10);
                
                while( true )
                {
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    done.WaitOne();
                }
            }
            catch (Exception ex)
            {
                // some logging actions
            }
        }

        protected virtual void AcceptCallback( IAsyncResult args)
        {
            done.Set();
            
            var listener = args.AsyncState as Socket;

            if( listener == null)
            {
                throw new InvalidAsynchronousStateException( "AsyncResult argument isn't socket" );
            }

            var handler = listener.EndAccept(args);

            var state = new StateObject();
            state.workSocket = handler;

            handler.BeginReceive(state.buffer,0, StateObject.BufferSize,0,new AsyncCallback(ReadCallback), state );
        }

        public virtual void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            var handler = state.workSocket;

            // Read data from the client socket. 
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                try
                {
                    var decoded = NetworkHelper.PacketDecoding(state.buffer);

                    var command = PacketParser.Parse(decoded);

                    var response = command.Execute();
                } catch( InvalidPacketException ex)
                {
                    
                }
            }
        }
    }

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        
    }

}
