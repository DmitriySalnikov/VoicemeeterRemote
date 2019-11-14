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
		public async Task TestIsDirty()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var res = Voicemeeter.Remote.IsParametersDirty();

				Console.WriteLine(res);
				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestWaitDirty()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				int i = 1000;
				while (i > 0)
				{
					var res = Voicemeeter.Remote.IsParametersDirty();

					if (res == Voicemeeter.VoicemeeterParametersState.NoNewParameters)
						break;
					i--;

					await Task.Delay(1);
				}

				if (i == 0)
					throw new Exception("Error");

				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestRetrieveText()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Label";
				var code = Voicemeeter.Remote.GetParameterText(parameter, out string test);
				Console.WriteLine("Strip 0 name " + test);
				await Task.Delay(1000);
				Voicemeeter.Remote.Logout();
			}
		}

		[TestMethod]
		public async Task TestSetText()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Label";
				var codeR = Voicemeeter.Remote.GetParameterText(parameter, out string test);
				Console.WriteLine(Voicemeeter.Remote.IsParametersDirty());
				Console.WriteLine(test);
				await Task.Delay(1000);
				var codeS = Voicemeeter.Remote.SetParameterText(parameter, "Testing");
				Console.WriteLine(Voicemeeter.Remote.IsParametersDirty());
				await Task.Delay(1000);
				var codeR3 = Voicemeeter.Remote.GetParameterText(parameter, out string test3);
				Console.WriteLine(Voicemeeter.Remote.IsParametersDirty());
				Console.WriteLine(test3);
				await Task.Delay(1000);
				var codeS2 = Voicemeeter.Remote.SetParameterText(parameter, test);
				Console.WriteLine(Voicemeeter.Remote.IsParametersDirty());
				await Task.Delay(1000);
				var codeR2 = Voicemeeter.Remote.GetParameterText(parameter, out string test2);
				Console.WriteLine(Voicemeeter.Remote.IsParametersDirty());
				Console.WriteLine(test2);
				Voicemeeter.Remote.Logout();

				Console.WriteLine($"Start Name: {test}. Middle Name: {test3}. End Name: {test2}");
				Assert.AreEqual(test, test2);
				Assert.AreNotEqual(test3, test2);
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestGetOutputDeviceNumber()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Console.WriteLine($"Device count: {Voicemeeter.Remote.GetOutputDeviceNumber()}");

				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestGetInputDeviceNumber()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Console.WriteLine($"Device count: {Voicemeeter.Remote.GetInputDeviceNumber()}");

				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestGetOutputDevice()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.GetOutputDeviceDescription(1, out Voicemeeter.VoicemeeterDeviceType type, out string name, out string hardId);
				Console.WriteLine($"{type}: '{name}' {hardId}");

				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestGetInputDevice()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.GetInputDeviceDescription(1, out Voicemeeter.VoicemeeterDeviceType type, out string name, out string hardId);
				Console.WriteLine($"{type}: '{name}' {hardId}");

				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		public async Task TestShow()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Show();
				await Task.Delay(1000);
				Voicemeeter.Remote.Logout();
			}
		}

		[TestMethod]
		public async Task TestHide()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Hide();
				await Task.Delay(1000);
				Voicemeeter.Remote.Logout();
			}
		}

		[TestMethod]
		public async Task TestLevel()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var code = Voicemeeter.Remote.GetLevel(Voicemeeter.LevelType.Output, 0, out float level);
				Console.WriteLine(level);
				Voicemeeter.Remote.Logout();
				Assert.AreNotEqual(level, 0);
			}
		}

		[TestMethod]
		public async Task TestLevelSubscribe()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var channels = new Voicemeeter.LevelsWatcher.Channel[] {
					new Voicemeeter.LevelsWatcher.Channel {
						LevelType = Voicemeeter.LevelType.Output,
						ChannelNumber = 0
					}
				};
				var results = new List<float>();
				var levels = new Voicemeeter.LevelsWatcher(channels, 20);
				levels.OnRecieveNewLevels += (x => { results.Add(x[0]); Console.WriteLine(x[0]); });

				await Task.Delay(1000);
				Voicemeeter.Remote.Logout();
				Assert.AreNotEqual(results.Count, 0);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestVolume()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Gain";
				float setValue = 2;

				Voicemeeter.Remote.SetParameterFloat(parameter, setValue);
				await Task.Delay(5000);
				var code = Voicemeeter.Remote.GetParameterFloat(parameter, out float result);
				await Task.Delay(1000);
				Voicemeeter.Remote.SetParameterFloat(parameter, 0);

				Voicemeeter.Remote.Logout();
				Assert.AreEqual(setValue, result);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestMute()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				var parameter = "Strip[0].Mute";

				Voicemeeter.Remote.SetParameterFloat(parameter, 1);
				await Task.Delay(1000);
				var code = Voicemeeter.Remote.GetParameterFloat(parameter, out float mute);

				Voicemeeter.Remote.SetParameterFloat(parameter, 0);
				await Task.Delay(1000);
				Voicemeeter.Remote.Logout();
				Assert.AreEqual(mute, 1);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestShutdown()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Shutdown();
				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestEject()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Eject();
				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}

		[TestMethod]
		[Ignore]
		public async Task TestRestart()
		{
			using (var _ = await Voicemeeter.Remote.Initialize(Voicemeeter.VoicemeeterType.VoicemeeterPotato).ConfigureAwait(false))
			{
				Voicemeeter.Remote.Restart();
				Voicemeeter.Remote.Logout();
				await Task.Delay(1000);
			}
		}
	}
}
