using System;
using System.Net.Sockets;

namespace SocketNet
{
	public class UserEventArgs : EventArgs
	{
		public UserEventArgs(string guid, string ip, int port)
		{
			Guid = guid;
			Ip = ip;
			Port = port;
		}

		public string Guid
		{
			get;
			private set;
		}

		public string Ip
		{
			get;
			private set;
		}

		public int Port
		{
			get;
			private set;
		}
	}

	public class KickEventArgs : UserEventArgs
	{
		public KickEventArgs(string guid, string ip, int port, string reason)
			: base(guid, ip, port)
		{
			Reason = reason;
		}

		public string Reason
		{
			get;
			private set;
		}
	}

	public class ErrorEventArgs : EventArgs
	{
		public ErrorEventArgs(Exception ex, DateTime time)
		{
			Exception = ex;
			Time = time;
		}

		public Exception Exception
		{
			get;
			private set;
		}

		public DateTime Time
		{
			get;
			private set;
		}
	}

	public class ServerErrorEventArgs : ErrorEventArgs
	{
		public ServerErrorEventArgs(Exception ex, DateTime time, Server server)
			: base(ex, time)
		{
			Server = server;
		}

		public Server Server
		{
			get;
			private set;
		}
	}

	public class ClientErrorEventArgs : ErrorEventArgs
	{
		public ClientErrorEventArgs(Exception ex, DateTime time, Client client) 
			: base(ex, time)
		{
			Client = client;
		}

		public Client Client
		{
			get;
			private set;
		}
	}

	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(Message msg, Socket sock)
		{
			Message = msg;
			Sender = sock;
		}

		public Message Message
		{
			get;
			private set;
		}

		public Socket Sender
		{
			get;
			private set;
		}
	}
}
