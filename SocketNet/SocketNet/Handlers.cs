namespace SocketNet
{
	public delegate void MessageHandler(MessageEventArgs e);
	public delegate void UserHandler(UserEventArgs e);
	public delegate void KickHandler(KickEventArgs e);
	public delegate void ServerErrorHandler(ServerErrorEventArgs e);
	public delegate void ClientErrorHandler(ClientErrorEventArgs e);
}
