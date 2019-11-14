using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voicemeeter
{
	/// <summary>
	/// Observable levels monitor. Use events to subscribe to the levels of the selected Channels.
	/// Usage:
	///  var levels = new Voicemeeter.Levels(channels, 20);
	///  levels.OnRecieveNewLevels += (x => DoSomethingWithFloatArray(x));
	/// </summary>
	public class LevelsWatcher : IDisposable
	{
		public class Channel
		{
			public LevelType LevelType { get; set; }
			public int ChannelNumber { get; set; }
		};

		public delegate void VoidFloatArray(float[] levels);
		public delegate void VoidLevelError(LevelError error);
		public event VoidFloatArray OnRecieveNewLevels;
		public event VoidLevelError OnError;

		private readonly List<Channel> channels = new List<Channel>();
		private bool isWorking = false;
		private int msecs = 16;

		public LevelsWatcher(Channel[] _channels, int milliseconds = 16)
		{

			msecs = milliseconds;
			channels = new List<Channel>(_channels);
			isWorking = true;
			Watch();
		}

		private async void Watch()
		{
			while (isWorking)
			{
				var values = new List<float>(channels.Count);
				foreach (var channel in channels)
				{
					var error = Remote.GetLevel(channel.LevelType, channel.ChannelNumber, out float res);
					if (error != LevelError.OK)
					{
						OnError?.Invoke(error);
						isWorking = false;
						break;
					}
					values.Add(res);
				}

				if (!isWorking)
					break;

				OnRecieveNewLevels?.Invoke(values.ToArray());

				await Task.Delay(msecs);
			}
		}

		public void Dispose()
		{
			isWorking = false;
		}
	}
}

