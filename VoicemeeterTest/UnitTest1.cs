using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace VoiceMeeterTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public async Task TestRetrieveText()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Label";
				var test = Voicemeeter.Remote.GetTextParameter(parameter);
				await Task.Delay(1000);
				Assert.IsFalse(string.IsNullOrEmpty(test));
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestSetText()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Label";
				Voicemeeter.Remote.SetTextParameter(parameter, "Testing");
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestVolume()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Gain";
				float setValue = 2;

				Voicemeeter.Remote.SetParameter(parameter, setValue);
				await Task.Delay(5000);
				var result = Voicemeeter.Remote.GetParameter(parameter);
				await Task.Delay(1000);
				Voicemeeter.Remote.SetParameter(parameter, 0);

				Assert.AreEqual(setValue, result);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestMute()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Mute";

				Voicemeeter.Remote.SetParameter(parameter, 1);
				await Task.Delay(1000);
				var mute = Voicemeeter.Remote.GetParameter(parameter);

				Voicemeeter.Remote.SetParameter(parameter, 0);
				await Task.Delay(1000);
				Assert.AreEqual(mute, 1);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestShutdown()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Shutdown();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestShow()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Show();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestEject()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Eject();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestRestart()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Restart();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestLevel()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				var level = Voicemeeter.Remote.GetLevel(Voicemeeter.LevelType.Output, 0);
				Assert.AreNotEqual(level, 0);
			}
		}


		[TestMethod]
		[Ignore]
		public async Task TestLevelSubscribe()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.RunVoicemeeterParam.VoicemeeterPotato).ConfigureAwait(false))
			{
				var channels = new Voicemeeter.Levels.Channel[] {
					new Voicemeeter.Levels.Channel {
						LevelType = Voicemeeter.LevelType.Output,
						ChannelNumber = 0
					}
				};
				var results = new List<float>();
				var levels = new Voicemeeter.Levels(channels, 20);
				levels.OnRecieveNewLevels += (x => results.Add(x[0]));

				await Task.Delay(1000);
				Assert.AreNotEqual(results.Count, 0);
			}
		}
	}
}
