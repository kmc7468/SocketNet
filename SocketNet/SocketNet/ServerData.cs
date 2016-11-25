using System;
using System.Drawing;

namespace SocketNet
{
	public class ServerData
	{
		private string _ServerName = string.Empty;
		public string ServerName
		{
			get
			{
				return _ServerName;
			}
			set
			{
				if (value.Trim() == string.Empty)
					throw new ArgumentException("ServerName 속성은 비어있을 수 없습니다.");

				_ServerName = value;
			}
		}

		private int? _MaxUser = null;
		public int? MaxUser
		{
			get
			{
				return _MaxUser;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(MaxUser));
				else if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(MaxUser), "MaxUser 속성은 1 이상이여야 합니다.");

				_MaxUser = value;
			}
		}
	}
}
