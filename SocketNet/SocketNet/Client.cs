using System;
using System.Net;
using System.Net.Sockets;

namespace SocketNet
{
	public class Client
	{
		public event MessageHandler SendMessage;
		public event MessageHandler ReceiveMessage;
		public event UserHandler Joined;
		public event UserHandler Left;
		public event KickHandler Kicked;
		public event ClientErrorHandler ClientError;

		private class AsyncObject
		{
			public byte[] Buffer;
			public Socket WorkingSocket;

			public AsyncObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
			}
		}

		public ServerData Server
		{
			get;
			private set;
		} = new ServerData();

		public string Guid
		{
			get;
			set;
		}

		private bool _Connected = false;
		private Socket _ClientSocket = null;
		private AsyncCallback _fnReceive;
		private AsyncCallback _fnSend;

		public Client()
		{
			_fnReceive = new AsyncCallback(receive);
			_fnSend = new AsyncCallback(send);

			ReceiveMessage += Client_ReceiveMessage;
		}

		private void Client_ReceiveMessage(MessageEventArgs e)
		{
			if (Server.ServerName == string.Empty)
			{
				Send(new Message() { Type = MessageType.Info, Text = Guid });
				Server.ServerName = e.Message.Text;
				return;
			}

			if (Server.MaxUser == null)
			{
				Server.MaxUser = Convert.ToInt32(e.Message.Text);
				return;
			}
			
			if (e.Message.Type == MessageType.Kick)
			{
				_Connected = false;

				Kicked?.Invoke(new KickEventArgs(Guid, ((IPEndPoint)_ClientSocket.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_ClientSocket.RemoteEndPoint).Port, e.Message.Text));

				_ClientSocket.Close();
			}
		}

		private void receive(IAsyncResult ar)
		{
			AsyncObject ao = (AsyncObject)ar.AsyncState;

			int recvBytes = 0;

			try
			{
				recvBytes = ao.WorkingSocket.EndReceive(ar);
			}
			catch (Exception ex)
			{
				ClientError?.Invoke(new ClientErrorEventArgs(ex, DateTime.Now, this));

				return;
			}

			if (recvBytes > 0)
			{
				byte[] msgByte = new byte[recvBytes];
				Array.Copy(ao.Buffer, msgByte, recvBytes);

				Message msg = new Message(msgByte);

				ReceiveMessage?.Invoke(new MessageEventArgs(msg, ao.WorkingSocket));
			}

			try
			{
				ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnReceive, ao);
			}
			catch (Exception ex)
			{
				ClientError?.Invoke(new ClientErrorEventArgs(ex, DateTime.Now, this));

				return;
			}
		}

		private void send(IAsyncResult ar)
		{
			AsyncObject ao = (AsyncObject)ar.AsyncState;

			int sendBytes;

			try
			{
				sendBytes = ao.WorkingSocket.EndSend(ar);
			}
			catch (Exception ex)
			{
				ClientError?.Invoke(new ClientErrorEventArgs(ex, DateTime.Now, this));

				return;
			}

			if (sendBytes > 0)
			{
				byte[] msgByte = new byte[sendBytes];
				Array.Copy(ao.Buffer, msgByte, sendBytes);

				Message msg = new Message(msgByte);

				SendMessage?.Invoke(new MessageEventArgs(msg, ao.WorkingSocket));
			}
		}

		public bool isConnected
		{
			get
			{
				return _Connected;
			}
		}

		public void Start(string host_address, ushort host_port)
		{
			_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

			bool join = false;

			try
			{
				_ClientSocket.Connect(host_address, host_port);

				join = true;
			}
			catch (Exception ex)
			{
				ClientError?.Invoke(new ClientErrorEventArgs(ex, DateTime.Now, this));

				join = false;
			}

			_Connected = join;

			if(join)
			{
				AsyncObject ao = new AsyncObject(4096);
				ao.WorkingSocket = _ClientSocket;
				_ClientSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnReceive, ao);
			}
		}

		public void Stop()
		{
			if (!_Connected) return;

			_Connected = false;

			Send(new Message() { Type = MessageType.Left, Text = Guid });

			Left?.Invoke(new UserEventArgs(Guid, ((IPEndPoint)_ClientSocket.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_ClientSocket.RemoteEndPoint).Port));
			_ClientSocket.Close();
		}

		public void Send(Message message)
		{
			AsyncObject ao = new AsyncObject(1);
			ao.Buffer = message;
			ao.WorkingSocket = _ClientSocket;

			try
			{
				_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSend, ao);
			}
			catch (Exception ex)
			{
				ClientError?.Invoke(new ClientErrorEventArgs(ex, DateTime.Now, this));
			}
		}
	}
}