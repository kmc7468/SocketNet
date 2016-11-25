using System;
using System.Text;

namespace SocketNet
{
	public enum MessageType
	{
		None,

		Joined,
		Left,
		Kick,
		Chat,
		Command,
		Info,
	}

	public class Message
	{
		public Message()
		{

		}

		public Message(string text)
		{
			Type = (MessageType)Convert.ToInt32(text.Substring(0, 1));
			Text = text.Substring(1);
		}

		public Message(byte[] bytes)
		{
			string text = Encoding.Unicode.GetString(bytes);

			Type = (MessageType)Convert.ToInt32(text.Substring(0, 1));
			Text = text.Substring(1);
		}

		public MessageType Type
		{
			get;
			set;
		}

		public string Text
		{
			get;
			set;
		}

		public override string ToString()
		{
			return (int)Type + Text;
		}

		public byte[] ToBytes()
		{
			return Encoding.Unicode.GetBytes(ToString());
		}

		public static bool operator==(Message a, Message b)
		{
			return (a.Text == b.Text && a.Type == b.Type);
		}

		public static bool operator!=(Message a, Message b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Message)) return false;

			Message b = (Message)obj;

			return (Text == b.Text && Type == b.Type);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static implicit operator string(Message msg)
		{
			return msg.ToString();
		}

		public static implicit operator byte[](Message msg)
		{
			return msg.ToBytes();
		}
	}
}
