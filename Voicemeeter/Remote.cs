﻿using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Voicemeeter
{
	public static class Remote
	{
		// Don't keep loading the DLL
		private static IntPtr? handle;

		/// <summary>
		/// Logs into the Voicemeeter application.  Starts the given application (Voicemeeter, Bananna, Potato) if it is not already runnning.
		/// </summary>
		/// <param name="voicemeeterType">The Voicemeeter program to run</param>
		/// <returns>IDisposable class to dispose when finished with the remote.</returns>
		public static async Task<IDisposable> Initialize(VoicemeeterType voicemeeterType)
		{
			if (handle.HasValue == false)
			{
				// Find current version from the registry
				const string key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
				const string uninstKey = "VB:Voicemeeter {17359A74-1236-5467}";
				var voicemeeter = Registry.GetValue($"{key}\\{uninstKey}", "UninstallString", null);
				if (voicemeeter == null)
				{
					throw new Exception("Voicemeeter not installed");
				}

				handle = Wrapper.LoadLibrary(
					System.IO.Path.Combine(System.IO.Path.GetDirectoryName(voicemeeter.ToString()), RemoteWrapper.VMRDLLName));
			}

			var startVoiceMeeter = voicemeeterType != VoicemeeterType.None;

			await Login(voicemeeterType, startVoiceMeeter).ConfigureAwait(false);

			return null;
		}

		#region Login

		/// <summary>
		/// Login to remote Voicemeeter interface
		/// </summary>
		/// <param name="retry">Try to Run Voicemeeter and login again</param>
		/// <returns></returns>
		public static async Task<bool> Login(VoicemeeterType voicemeeterType, bool retry = true)
		{
			switch ((LoginResponse)RemoteWrapper.VBVMR_Login())
			{
				case LoginResponse.OK:
				case LoginResponse.AlreadyLoggedIn:
					return true;

				case LoginResponse.NotLaunched:
					if (retry)
					{
						// Run voicemeeter program 
						RunVoicemeeter(voicemeeterType);

						await Task.Delay(2000).ConfigureAwait(false);
						return await Login(voicemeeterType, false).ConfigureAwait(false);
					}
					break;
			}
			return false;
		}

		/// <summary>
		/// Logout from remote Voicemeeter interface
		/// </summary>
		/// <returns></returns>
		public static bool Logout()
		{
			switch ((LoginResponse)RemoteWrapper.VBVMR_Logout())
			{
				case LoginResponse.OK:
					return true;
			}
			return false;
		}

		/// <summary>
		/// Start the VoiceMeeter program
		/// </summary>
		/// <param name="voicemeterType"></param>
		public static RunError RunVoicemeeter(VoicemeeterType voicemeterType)
		{
			return (RunError)RemoteWrapper.VBVMR_RunVoicemeeter((int)voicemeterType);
		}

		#endregion

		#region Parameters

		/// <summary>
		/// Check if parameters have changed.
		/// Call this function periodically (typically every 10 or 20ms).
		/// </summary>
		/// <returns></returns>
		public static VoicemeeterParametersState IsParametersDirty()
		{
			return (VoicemeeterParametersState)RemoteWrapper.VBVMR_IsParametersDirty();

		}

		/// <summary>
		/// Gets a text value
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static GetSetParameterError GetParameterText(string parameter, out string result)
		{
			var buffer = new StringBuilder(512);
			var code = RemoteWrapper.VBVMR_GetParameterStringW(parameter, buffer);
			if (code == (int)GetSetParameterError.OK)
				result = buffer.ToString();
			else
				result = string.Empty;

			buffer.Clear();
			return (GetSetParameterError)code;
		}

		/// <summary>
		/// Set a text value
		/// </summary>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static GetSetParameterError SetParameterText(string parameter, string value)
		{
			return (GetSetParameterError)RemoteWrapper.VBVMR_SetParameterStringW(parameter, value);
		}

		/// <summary>
		/// Get a named parameter
		/// </summary>
		/// <param name="parameter">Parameter name</param>
		/// <returns>float value</returns>
		public static GetSetParameterError GetParameterFloat(string parameter, out float result)
		{
			float value = 0;
			var code = RemoteWrapper.VBVMR_GetParameterFloat(parameter, ref value);

			if (code == (int)GetSetParameterError.OK)
				result = value;
			else
				result = 0;

			return (GetSetParameterError)code;
		}

		/// <summary>
		/// Set a named parameter
		/// </summary>
		/// <param name="parameter">Parameter name</param>
		/// <param name="value">float value</param>
		public static GetSetParameterError SetParameterFloat(string parameter, float value)
		{
			return (GetSetParameterError)RemoteWrapper.VBVMR_SetParameterFloat(parameter, value);
		}

		/// <summary>
		/// Set one or several parameters by a script ( < 48 kB ).
		/// 
		/// EXAMPLE:
		///		SetParameters( "Strip[0].gain = -6.0;
		///						Strip[0].A1 = 0;
		///						Strip[0].B1 = 1;
		///						Strip[1].gain = -6.0;
		///						Strip[2].gain = 0.0;
		///						Strip[3].name = \"Skype Caller\";", out int error_line);
		/// </summary>
		/// <param name="parameters">Text of script</param>
		/// <param name="error_line">Line where error occurred</param>
		/// <returns></returns>
		public static SetParametersByScriptError SetParameters(string parameters, out int error_line)
		{
			var code = RemoteWrapper.VBVMR_SetParametersW(parameters);
			if (code > 0)
			{
				error_line = code;
				return SetParametersByScriptError.ErrorInLine;
			}

			error_line = -1;
			return (SetParametersByScriptError)code;
		}

		#endregion

		#region General Information

		/// <summary>
		/// Get Voicemeeter Type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static VoicemeeterInfoError GetVoicemeeterType(out VoicemeeterType type)
		{

			int _type = 0;
			int code = RemoteWrapper.VBVMR_GetVoicemeeterType(ref _type);

			if (code == (int)VoicemeeterInfoError.OK)
			{
				if (_type > 0 && _type <= 3)
				{
					type = (VoicemeeterType)_type;
				}
				else
				{
					type = VoicemeeterType.None;
				}
			}
			else
			{
				type = VoicemeeterType.None;
			}

			return (VoicemeeterInfoError)code;
		}

		/// <summary>
		/// Get version of current launched Voicemeeter
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public static VoicemeeterInfoError GetVoicemeeterVersion(out VoicemeeterVersion version)
		{
			long _ver = 0;
			int code = RemoteWrapper.VBVMR_GetVoicemeeterVersion(ref _ver);

			if (code == (int)VoicemeeterInfoError.OK)
				version = new VoicemeeterVersion(
					(int)(_ver & 0xFF000000) >> 24,
					(int)(_ver & 0x00FF0000) >> 16,
					(int)(_ver & 0x0000FF00) >> 8,
					(int)(_ver & 0x000000FF)
					);
			else
				version = new VoicemeeterVersion();

			return (VoicemeeterInfoError)code;
		}

		#endregion

		#region Level

		/// <summary>
		/// Get Current levels. 
		/// (this function must be called from one thread only)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="channel"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static LevelError GetLevel(LevelType type, int channel, out float result)
		{
			float value = 0;
			var code = (RemoteWrapper.VBVMR_GetLevel((int)type, channel, ref value));

			if (code == (int)LevelError.OK)
			{
				result = value;
			}
			else
			{
				result = 0;
			}

			return (LevelError)code;
		}

		/// <summary>
		/// Get MIDI message from M.I.D.I. input device used by Voicemeeter M.I.D.I. mapping.
		/// (this function must be called from one thread only)
		/// </summary>
		/// <returns></returns>
		public static MidiMessageError GetMidiMessage(out byte[] buffer, int maxBytes)
		{
			var tmp_buffer = new byte[maxBytes];
			var code = RemoteWrapper.VBVMR_GetMidiMessage(tmp_buffer, maxBytes);

			if (code > 0)
			{
				buffer = new byte[code];
				Array.Copy(tmp_buffer, 0, buffer, 0, code);

				return MidiMessageError.OK;
			}
			else
			{
				buffer = new byte[0];
			}

			return (MidiMessageError)code;
		}

		#endregion

		#region Devices

		/// <summary>
		/// Get number of Audio Output Device available on the system
		/// </summary>
		/// <returns></returns>
		public static int GetOutputDeviceNumber()
		{
			return RemoteWrapper.VBVMR_Output_GetDeviceNumber();
		}

		/// <summary>
		/// Get the description of Audio Output Device 
		/// </summary>
		/// <param name="index">Device index</param>
		/// <param name="type">Out Type of Device</param>
		/// <param name="name">Out Name of Device</param>
		/// <param name="hardwareId">Out HardwareID of Device</param>
		/// <returns></returns>
		public static InputOutputGetDeviceError GetOutputDeviceDescription(int index, out VoicemeeterDeviceType type, out string name, out string hardwareId)
		{
			return _Internal_GetInputOutputDeviceDescription(false, index, out type, out name, out hardwareId);
		}

		/// <summary>
		/// Get number of Audio Input Device available on the system
		/// </summary>
		/// <returns></returns>
		public static int GetInputDeviceNumber()
		{
			return RemoteWrapper.VBVMR_Input_GetDeviceNumber();
		}

		/// <summary>
		/// Get the description of Audio Input Device 
		/// </summary>
		/// <param name="index">Device index</param>
		/// <param name="type">Out Type of Device</param>
		/// <param name="name">Out Name of Device</param>
		/// <param name="hardwareId">Out HardwareID of Device</param>
		/// <returns></returns>
		public static InputOutputGetDeviceError GetInputDeviceDescription(int index, out VoicemeeterDeviceType type, out string name, out string hardwareId)
		{
			return _Internal_GetInputOutputDeviceDescription(true, index, out type, out name, out hardwareId);
		}

		internal static InputOutputGetDeviceError _Internal_GetInputOutputDeviceDescription(bool isInput, int index, out VoicemeeterDeviceType type, out string name, out string hardwareId)
		{
			int _type = 0;
			StringBuilder _name = new StringBuilder(512);
			StringBuilder _id = new StringBuilder(512);

			try
			{
				var code = -1;
				if (isInput)
					code = RemoteWrapper.VBVMR_Input_GetDeviceDescW(index, ref _type, _name, _id);
				else
					code = RemoteWrapper.VBVMR_Output_GetDeviceDescW(index, ref _type, _name, _id);

				if (code == (int)InputOutputGetDeviceError.OK)
				{
					type = (VoicemeeterDeviceType)_type;
					name = _name.ToString();
					hardwareId = _id.ToString();

					_name.Clear();
					_id.Clear();
					return InputOutputGetDeviceError.OK;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message + "\n" + e.StackTrace);
			}

			type = VoicemeeterDeviceType.WDM;
			name = "";
			hardwareId = "";

			_name.Clear();
			_id.Clear();

			return InputOutputGetDeviceError.Error;
		}

		#endregion

		#region Commands

		/// <summary>
		/// Shutdown the VoiceMeeter program
		/// </summary>
		/// <param name="voicemeeterType">The Voicemeeter program to run</param>
		public static GetSetParameterError Shutdown()
		{
			return SetParameterFloat(VoicemeeterCommand.Shutdown, 1);
		}

		/// <summary>
		/// Restart the audio engine
		/// </summary>
		public static GetSetParameterError Restart()
		{
			return SetParameterFloat(VoicemeeterCommand.Restart, 1);
		}

		/// <summary>
		/// Shows the running Voicemeeter application if minimized.
		/// </summary>
		public static GetSetParameterError Show()
		{
			return SetParameterFloat(VoicemeeterCommand.Show, 1);
		}

		/// <summary>
		/// Hides the running Voicemeeter application if minimized.
		/// </summary>
		public static GetSetParameterError Hide()
		{
			return SetParameterFloat(VoicemeeterCommand.Show, 0);
		}

		/// <summary>
		/// Eject Cassette 
		/// </summary>
		public static GetSetParameterError Eject()
		{
			return SetParameterFloat(VoicemeeterCommand.Eject, 1);
		}

		/// <summary>
		/// Load a configuation file name
		/// </summary>
		/// <param name="configurationFileName">Full path to file</param>
		public static GetSetParameterError Load(string configurationFileName)
		{
			return SetParameterText(VoicemeeterCommand.Load, configurationFileName);
		}

		/// <summary>
		/// Save a configuration to the given file name
		/// </summary>
		/// <param name="configurationFileName">Full path to file</param>
		public static GetSetParameterError Save(string configurationFileName)
		{
			return SetParameterText(VoicemeeterCommand.Load, configurationFileName);
		}

		#endregion

		#region VB-Audio

		/// <summary>
		/// Register your audio callback function to receive real time audio buffer
		/// it's possible to register up to 3x different Audio Callback in the same application or in 
		/// 3x different applications.In the same application, this is possible because Voicemeeter
		/// provides 3 kind of audio Streams:
		///			- AUDIO INPUT INSERT(to process all Voicemeeter inputs as insert)
		///			- AUDIO OUTPUT INSERT(to process all Voicemeeter BUS outputs as insert)
		///			- ALL AUDIO I/O(to process all Voicemeeter i/o).
		/// Note: a single callback can be used to receive the 3 possible audio streams.
		/// </summary>
		/// <param name="mode">Callback mode</param>
		/// <param name="callback"></param>
		/// <param name="user">Custom user data</param>
		/// <param name="clientName">Client name with maximum length 64</param>
		/// <param name="alreadyRegisteredName">Name of already registered client</param>
		/// <returns></returns>
		public static AudioCallbackRegisterError AudioCallbackRegister(AudioCallbackMode mode, AudioCallback callback, object user, string clientName, out string alreadyRegisteredName)
		{
			const int maxLength = 64;
			clientName = clientName.Substring(0, Math.Min(clientName.Length, maxLength));
			var nameBytes = Enumerable.Repeat<byte>(0, maxLength).ToArray();
			Encoding.Default.GetBytes(clientName, 0, clientName.Length, nameBytes, 0);

			int code = -1;
			switch (mode)
			{
				case AudioCallbackMode.Input:
					ACF_Input = (u, c, d, useless) => { Internal_ProcessAudiocallbackData(AudioCallbackMode.Input, u, c, d); return 0; };

					code = RemoteWrapper.VBVMR_AudioCallbackRegister((int)mode, ACF_Input, GCHandle.ToIntPtr(GCHandle.Alloc(user)), nameBytes);
					if (code == 0)
						audioCallbackInputInternal = callback;
					break;
				case AudioCallbackMode.Output:
					ACF_Output = (u, c, d, useless) => { Internal_ProcessAudiocallbackData(AudioCallbackMode.Output, u, c, d); return 0; };

					code = RemoteWrapper.VBVMR_AudioCallbackRegister((int)mode, ACF_Output, GCHandle.ToIntPtr(GCHandle.Alloc(user)), nameBytes);
					if (code == 0)
						audioCallbackOutputInternal = callback;
					break;
				case AudioCallbackMode.Main:
					ACF_Main = (u, c, d, useless) => { Internal_ProcessAudiocallbackData(AudioCallbackMode.Main, u, c, d); return 0; };

					code = RemoteWrapper.VBVMR_AudioCallbackRegister((int)mode, ACF_Main, GCHandle.ToIntPtr(GCHandle.Alloc(user)), nameBytes);
					if (code == 0)
						audioCallbackMainInternal = callback;
					break;
			}


			if (code == (int)AudioCallbackRegisterError.AlreadyRegistered)
			{
				int idx = maxLength - 1;
				for (int i = 0; i < maxLength; i++)
					if (nameBytes[i] == 0)
					{
						idx = i;
						break;
					}

				alreadyRegisteredName = Encoding.Default.GetString(nameBytes, 0, idx);
				Console.WriteLine(alreadyRegisteredName);
				return AudioCallbackRegisterError.AlreadyRegistered;
			}
			alreadyRegisteredName = "";

			return (AudioCallbackRegisterError)code;
		}

		internal static AudioCallback audioCallbackInputInternal;
		internal static AudioCallback audioCallbackOutputInternal;
		internal static AudioCallback audioCallbackMainInternal;
		//                                    AudioCallbackFucntion
		internal static InternalAudioCallback ACF_Input;
		internal static InternalAudioCallback ACF_Output;
		internal static InternalAudioCallback ACF_Main;

		// Maybe that's a bad solution, but at this moment i dont known better
		internal static void Internal_ProcessAudiocallbackData(AudioCallbackMode mode, IntPtr user, AudioCommand command, IntPtr data)
		{
			object managed_user = ((GCHandle)user).Target;
			object managed_data = null;

			switch (command)
			{
				case AudioCommand.Starting:
					managed_data = Marshal.PtrToStructure(data, typeof(AudioInfo));
					break;
				case AudioCommand.BufferIn:
				case AudioCommand.BufferOut:
				case AudioCommand.BufferMain:
					managed_data = Marshal.PtrToStructure(data, typeof(AudioBuffer));
					break;
			}

			switch (mode)
			{
				case AudioCallbackMode.Input:
					audioCallbackInputInternal.Invoke(managed_user, command, managed_data);
					break;
				case AudioCallbackMode.Output:
					audioCallbackOutputInternal.Invoke(managed_user, command, managed_data);
					break;
				case AudioCallbackMode.Main:
					audioCallbackMainInternal.Invoke(managed_user, command, managed_data);
					break;
			}
		}

		/// <summary>
		/// Start Audio processing the Callback will be called with
		/// </summary>
		/// <returns></returns>
		public static AudioCallbackStartStopError AudioCallbackStart()
		{
			return (AudioCallbackStartStopError)RemoteWrapper.VBVMR_AudioCallbackStart();
		}

		/// <summary>
		/// Stop Audio processing the Callback will be called with
		/// </summary>
		/// <returns></returns>
		public static AudioCallbackStartStopError AudioCallbackStop()
		{
			return (AudioCallbackStartStopError)RemoteWrapper.VBVMR_AudioCallbackStop();
		}

		/// <summary>
		/// Unregister your callback to release voicemeeter virtual driver
		/// (this function will automatically call VBVMR_AudioCallbackStop() function)
		/// </summary>
		/// <returns></returns>
		public static AudioCallbackUnregisterError AudioCallbackUnregister()
		{
			return (AudioCallbackUnregisterError)RemoteWrapper.VBVMR_AudioCallbackUnregister();
		}

		#endregion
	}
}