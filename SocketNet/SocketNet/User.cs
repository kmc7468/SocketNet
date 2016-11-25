using System.Net.Sockets;

namespace SocketNet
{
	public class User
	{
		public Socket Socket
		{
			get;
			set;
		}

		public Server Server
		{
			get;
			set;
		}

		public string Guid
		{
			get;
			set;
		}

		public string Ip
		{
			get;
			set;
		}

		public int Port
		{
			get;
			set;
		}
	}
}
