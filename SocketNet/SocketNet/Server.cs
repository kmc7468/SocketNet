using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SocketNet
{
	public class Server
	{
		public event UserHandler JoinedUser;
		public event UserHandler LeftUser;
		public event KickHandler KickedUser;
		public event ServerErrorHandler ServerError;
		public event MessageHandler SendMessage;
		public event MessageHandler ReceiveMessage;

		private class AsyncObject
		{
			public byte[] Buffer;
			public Socket WorkingSocket;

			public AsyncObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
			}
		}

		public List<User> Users
		{
			get;
			private set;
		} = new List<User>();

		public ServerData Data
		{
			get;
			set;
		} = new ServerData();

		private Socket _ServerSocket = null;
		private AsyncCallback _fnReceive;
		private AsyncCallback _fnSend;
		private AsyncCallback _fnAccept;

		public Server()
		{
			_fnReceive = new AsyncCallback(receive);
			_fnSend = new AsyncCallback(send);
			_fnAccept = new AsyncCallback(connect);

			ReceiveMessage += Server_ReceiveMessage;
		}

		private void Server_ReceiveMessage(MessageEventArgs e)
		{
			User sender = GetUserBySocket(e.Sender);
			if (sender != null)
			{
				if (sender.Socket == null)
				{
					sender.Guid = e.Message.Text;

					JoinedUser?.Invoke(new UserEventArgs(sender.Guid, sender.Ip, sender.Port));

					return;
				}
			}

			if (e.Message.Type == MessageType.Left)
			{
				User u = Users.Find((ev) => { return ev.Guid == e.Message.Text; });
				Users.Remove(u);
				LeftUser?.Invoke(new UserEventArgs(u.Guid, u.Ip, u.Port));
				return;
			}
		}

		public void Start(ushort port)
		{
			_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			_ServerSocket.Listen(5);
			_ServerSocket.BeginAccept(_fnAccept, null);
		}

		public void Stop()
		{
			_ServerSocket.Close();
		}

		public User GetUserByGuid(string guid)
		{
			return Users.Find((e) => { return e.Guid == guid; });
		}

		public User GetUserByIp(string ip)
		{
			return Users.Find((e) => { return e.Ip == ip; });
		}

		public User GetUserBySocket(Socket sock)
		{
			return Users.Find((e) => { return e.Socket == sock; });
		}

		public bool Kick(User user, string reason)
		{
			try
			{
				SendToUser(user, new Message() { Type = MessageType.Kick, Text = reason });

				Users.Remove(user);

				KickedUser?.Invoke(new KickEventArgs(user.Guid, user.Ip, user.Port, reason));

				return true;
			}
			catch (Exception ex)
			{
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return false;
			}
		}

		public bool SendToAll(Message message)
		{
			foreach (var it in Users)
			{
				AsyncObject ao = new AsyncObject(1);
				ao.Buffer = message;
				ao.WorkingSocket = it.Socket;

				try
				{
					it.Socket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSend, ao);
				}
				catch (Exception ex)
				{
					ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

					return false;
				}
			}

			return true;
		}

		public bool SendToUser(User user, Message message)
		{
			AsyncObject ao = new AsyncObject(1);
			ao.Buffer = message;
			ao.WorkingSocket = user.Socket;
			try
			{
				user.Socket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSend, ao);
			}
			catch (Exception ex)
			{
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return false;
			}

			return true;
		}

		public bool SendToOther(User other, Message message)
		{
			foreach (var it in Users)
			{
				if (it == other) continue;

				AsyncObject ao = new AsyncObject(1);
				ao.Buffer = message;
				ao.WorkingSocket = it.Socket;
				try
				{
					it.Socket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSend, ao);
				}
				catch (Exception ex)
				{
					ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

					return false;
				}
			}

			return true;
		}

		private void connect(IAsyncResult ar)
		{
			Socket client;
			try
			{
				client = _ServerSocket.EndAccept(ar);
			}
			catch (Exception ex)
			{
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}

			AsyncObject ao = new AsyncObject(4096);
			ao.WorkingSocket = client;

			User user = new User();
			user.Socket = client;
			user.Ip = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
			user.Port = ((IPEndPoint)client.RemoteEndPoint).Port;
			user.Server = this;
			user.Guid = null;
			Users.Add(user);

			Message info = new Message() { Type = MessageType.Info, Text = Data.ServerName };
			SendToUser(user, info);

			info = new Message() { Type = MessageType.Info, Text = Data.MaxUser.GetValueOrDefault().ToString() };
			SendToUser(user, info);

			try
			{
				client.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnReceive, ao);
			}
			catch (Exception ex)
			{
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}
		}

		private void receive(IAsyncResult ar)
		{
			AsyncObject ao = (AsyncObject)ar.AsyncState;

			int recvBytes;

			try
			{
				recvBytes = ao.WorkingSocket.EndReceive(ar);
			}
			catch (Exception ex)
			{
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

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
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

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
				ServerError?.Invoke(new ServerErrorEventArgs(ex, DateTime.Now, this));

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
	}
}
