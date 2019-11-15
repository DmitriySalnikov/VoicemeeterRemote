using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Voicemeeter
{
	/// <summary>
	/// VB-AUDIO Callback is called for different task to Initialize, perform and end your process.
	/// VB-AUDIO Callback is part of single TIME CRITICAL Thread. 
	/// VB-AUDIO Callback is non re-entrant (cannot be called while in process)
	/// VB-AUDIO Callback is supposed to be REAL TIME when called to process buffer.
	/// (it means that the process has to be performed as fast as possible, waiting cycles are forbidden.
	/// do not use O/S synchronization object, even Critical_Section can generate waiting cycle. Do not use
	/// system functions that can generate waiting cycle like display, disk or communication functions for example).
	/// </summary>
	/// <param name="user">User pointer given on callback registration.</param>
	/// <param name="command">Reason why the callback is called.</param>
	/// <param name="data">Pointer on structure, pending on nCommand.</param>
	/// <param name="additional_data">Additional data, unused.</param>
	/// <returns>0: always 0 (unused).</returns>
	internal delegate int InternalAudioCallback(IntPtr user, AudioCommand command, IntPtr data, int additional_data);

	/// <summary>
	/// VB-AUDIO Callback is called for different task to Initialize, perform and end your process.
	/// VB-AUDIO Callback is part of single TIME CRITICAL Thread. 
	/// VB-AUDIO Callback is non re-entrant (cannot be called while in process)
	/// VB-AUDIO Callback is supposed to be REAL TIME when called to process buffer.
	/// (it means that the process has to be performed as fast as possible, waiting cycles are forbidden.
	/// do not use O/S synchronization object, even Critical_Section can generate waiting cycle. Do not use
	/// system functions that can generate waiting cycle like display, disk or communication functions for example).
	/// </summary>
	/// <param name="user">User data given on callback registration.</param>
	/// <param name="command">Reason why the callback is called.</param>
	/// <param name="data">Pointer on structure, pending on nCommand.</param>
	public delegate void AudioCallback(object user, AudioCommand command, object data);

	public enum RunError
	{
		OK = 0,
		NotInstalled = -1,
	}

	public enum VoicemeeterType
	{
		None = 0,
		Voicemeeter = 1,
		VoicemeeterBanana = 2,
		VoicemeeterPotato = 3,
	};

	public enum LevelType
	{
		PreFaderInput = 0,
		PostFaderInput = 1,
		PostMuteInput = 2,
		Output = 3
	};

	public enum LoginResponse
	{
		NotLaunched = 1,
		OK = 0,
		NoClient = -1,
		AlreadyLoggedIn = -2,
	}

	public enum VoicemeeterInfoError
	{
		OK = 0,
		CannotGetClient = -1,
		NoServer = -2
	}

	public enum VoicemeeterParametersState
	{
		NoNewParameters = 0,
		NewParameters = 1,
		Error = -1,
		NoServer = -2
	}

	public enum GetSetParameterError
	{
		OK = 0,
		Error = -1,
		NoServer = -2,
		UnknownParameter = -3,
		StructureMismatch = -5
	}

	public enum SetParametersByScriptError
	{
		OK = 0,
		Error = -1,
		NoServer = -2,
		UnexpectedError = -3,
		UnexpectedError2 = -4,
		ErrorInLine = -64
	}

	public enum LevelError
	{
		OK = 0,
		Error = -1,
		NoServer = -2,
		NoLevelAvailable = -3,
		OutOfRange = -4
	}

	public enum MidiMessageError
	{
		OK = 0,
		Error = -1,
		NoServer = -2,
		NoMIDIData = -5,
		NoMIDIData2 = -6
	}

	public enum VoicemeeterDeviceType
	{
		MME = 1,
		WDM = 3,
		KS = 4,
		ASIO = 5
	}

	public enum AudioCommand
	{
		/// <summary>
		/// Command to initialize data according SR and buffer size
		/// </summary>
		Starting = 1,
		/// <summary>
		/// Command to release data
		/// </summary>
		Ending = 2,
		/// <summary>
		/// If change in audio stream, you will have to restart audio 
		/// </summary>
		Change = 3,
		/// <summary>
		/// Input insert
		/// </summary>
		BufferIn = 10,
		/// <summary>
		/// Bus output insert
		/// </summary>
		BufferOut = 11,
		/// <summary>
		/// All i/o
		/// </summary>
		BufferMain = 20
	}

	public enum AudioCallbackMode
	{
		Input = 1,
		Output = 2,
		Main = 4
	}

	public enum InputOutputGetDeviceError
	{
		OK = 0,
		Error = -1
	}

	public enum AudioCallbackRegisterError
	{
		OK = 0,
		Error = -1,
		AlreadyRegistered = 1
	}

	public enum AudioCallbackStartStopError
	{
		OK = 0,
		Error = -1,
		NoCallbackRegistered = -1
	}

	public enum AudioCallbackUnregisterError
	{
		OK = 0,
		Error = -1,
		AlreadyUnregistered = 1
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct AudioInfo
	{
		[MarshalAs(UnmanagedType.I4)]
		public int SampleRate;
		[MarshalAs(UnmanagedType.I4)]
		public int SamplePerFrame;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	public struct AudioBuffer
	{
		[MarshalAs(UnmanagedType.I4)]
		int SampleRate;
		[MarshalAs(UnmanagedType.I4)]
		int SamplesPerFrame;
		[MarshalAs(UnmanagedType.I4)]
		int SizeOfInputBuffer;
		[MarshalAs(UnmanagedType.I4)]
		int SizeOfOutputBuffer;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		float[] InputBuffer;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		float[] OutputBuffer;
	}

	public struct VoicemeeterVersion
	{
		readonly int V1;
		readonly int V2;
		readonly int V3;
		readonly int V4;

		public VoicemeeterVersion(int v1, int v2, int v3, int v4)
		{
			V1 = v1;
			V2 = v2;
			V3 = v3;
			V4 = v4;
		}

		public override string ToString()
		{
			return $"{V1}.{V2}.{V3}.{V4}";
		}
	}

	public static class InputChannel
	{
		public const int Strip1Left = 0;
		public const int Strip1Right = 1;
		public const int Strip2Left = 2;
		public const int Strip2Right = 3;
		public const int Strip3Left = 4;
		public const int Strip3Right = 5;
		public const int VAIOLeft = 6;
		public const int VAIORight = 7;
		public const int AUXLeft = 8;
		public const int AUXRight = 9;
	}

	public static class OutputChannel
	{
		public const int A1Left = 0;
		public const int A1Right = 1;
		public const int A2Left = 2;
		public const int A2Right = 3;
		public const int A3Left = 4;
		public const int A3Right = 5;
		public const int Bus1Left = 6;
		public const int Bus1Right = 7;
		public const int Bus2Left = 8;
		public const int Bus2Right = 9;
	}

	public static class VoicemeeterCommand
	{
		public static string Shutdown = "Command.Shutdown";
		public static string Show = "Command.Show";
		public static string Restart = "Command.Restart";
		public static string Eject = "Command.Eject";
		public static string Reset = "Command.Reset";
		public static string Save = "Command.Save";
		public static string Load = "Command.Load";
	}
}
