using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Voicemeeter
{
	internal static class RemoteWrapper
	{
		/******************************************************************************/
		/*                                                                            */
		/*                                    Login                                   */
		/*                                                                            */
		/******************************************************************************/

		#region Login

		public const string VMRDLLName = "VoicemeeterRemote.dll";

		// Open Communication Pipe With Voicemeeter(typically called on software startup).
		// returns:  0: OK(no error).
		//			 1: OK but Voicemeeter Application not launched.
		//			-1: cannot get client (unexpected)
		//			-2: unexpected login (logout was expected before).
		[DllImport(VMRDLLName, CallingConvention = CallingConvention.StdCall)]
		internal static extern int VBVMR_Login();

		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_Logout();

		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_RunVoicemeeter(int voicemeterType);

		#endregion

		/******************************************************************************/
		/*                                                                            */
		/*                             General Information                            */
		/*                                                                            */
		/******************************************************************************/

		#region General Information
		// Get Voicemeeter Type
		// param pType : Pointer on 32bit long receiving the type(1 = Voicemeeter, 2= Voicemeeter Banana).
		//
		//				 VOICEMEETER STRIP/BUS INDEX ASSIGNMENT
		//
		//				 | Strip 1 | Strip 2 |Virtual Input|  BUS A  |  BUS B  |
		//				 +---------+---------+-------------+---------+---------+
		//				 |    0    |    1    |      2      |    0    |    1    |
		//
		//				 VOICEMEETER BANANA STRIP/BUS INDEX ASSIGNMENT
		//
		//				 | Strip 1 | Strip 2 | Strip 2 |Virtual Input|Virtual AUX|BUS A1|BUS A2|BUS A3|BUS B1|BUS B2|
		//				 +---------+---------+---------+-------------+-----------+------+------+------+------+------+
		//				 |    0    |    1    |    2    |       3     |     4     |   0  |   1  |   2  |   3  |   4  |
		//
		//				 VOICEMEETER POTATO STRIP/BUS INDEX ASSIGNMENT
		//
		//				 | Strip 1 | Strip 2 | Strip 2 | Strip 2 | Strip 2 |Virtual Input|Virtual AUX|   VAIO3   |BUS A1|BUS A2|BUS A3|BUS A4|BUS A5|BUS B1|BUS B2|BUS B3|
		//				 +---------+---------+---------+---------+---------+-------------+-----------+-----------+------+------+------+------+------+------+------+------+
		//				 |    0    |    1    |    2    |    3    |    4    |      5      |     6     |     7     |   0  |   1  |   2  |   3  |   4  |   5  |   6  |   7  |
		//
		//
		// returns:  0: OK(no error).
		//			-1: cannot get client(unexpected)
		//			-2: no server.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_GetVoicemeeterType(ref int type);

		// Get Voicemeeter Version
		// param pType : Pointer on 32bit integer receiving the version(v1.v2.v3.v4)
		//					v1 = (version & 0xFF000000)>>24;
		//					v2 = (version & 0x00FF0000)>>16;
		//					v3 = (version & 0x0000FF00)>>8;
		//					v4 = version & 0x000000FF;
		//
		// return:	 0: OK(no error).
		//			-1: cannot get client(unexpected)
		//			-2: no server.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_GetVoicemeeterVersion(ref long version);

		#endregion

		/******************************************************************************/
		/*                                                                            */
		/*                               Get Parameters                               */
		/*                                                                            */
		/******************************************************************************/

		#region Get Parameters
		// Check if parameters have changed.
		// Call this function periodically (typically every 10 or 20ms).
		// (this function must be called from one thread only)
		// returns:	 0: no new paramters.
		//  		 1: New parameters -> update your display.
		//			-1: error(unexpected)
		//			-2: no server.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_IsParametersDirty();

		// Get/Set Parameters return codes
		// return: 0: OK (no error).
		//        -1: error
		//        -2: no server.
		//        -3: unknown parameter
		//        -5: structure mismatch
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_GetParameterFloat(string szParamName, ref float value);

		// Get parameter value.
		// param szParamName : Null Terminal ASCII String giving the name of the parameter (see parameters name table)
		// param pValue : Pointer on String(512 char or wchar) receiving the wanted value.
		// return:	 0: OK (no error).
		//			-1: error
		//			-2: no server.
		//			-3: unknown parameter
		//			-5: structure mismatch

		[DllImport(VMRDLLName, CallingConvention = CallingConvention.StdCall)]
		internal static extern int VBVMR_GetParameterStringW(
			 [MarshalAs(UnmanagedType.LPStr)] string szParamName,      // char*
			 [MarshalAs(UnmanagedType.LPWStr)] StringBuilder value);   // unsigned short*

		#endregion

		/******************************************************************************/
		/*                                                                            */
		/*                               Set Parameters                               */
		/*                                                                            */
		/******************************************************************************/

		#region Set Parameters
		// Set a single float 32 bits parameters.
		// param szParamName : Null Terminal ASCII String giving the name of the parameter (see parameters name table)
		//					example:
		//					Strip[1].gain
		//					Strip[0].mute
		//					Bus[0].gain
		//					Bus[0].eq.channel[0].cell[0].gain
		//
		// param pValue : float 32bit containing the new value.
		// return :	 0: OK(no error).
		//			-1: error
		//			-2: no server.
		//			-3: unknown parameter
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_SetParameterFloat(string szParamName, float value);

		// Set a single string parameters.
		// param szParamName : Null Terminal ASCII String giving the name of the parameter (see parameters name table)
		//					example:
		//					Strip[1].name
		//					Strip[0].device.mme
		//					Bus[0].device.asio
		// param szString : zero terminal string.
		// return :	 0: OK(no error).
		//			-1: error
		//			-2: no server.
		//			-3: unknown parameter
		[DllImport(VMRDLLName, CallingConvention = CallingConvention.StdCall)]
		internal static extern int VBVMR_SetParameterStringW(
			[MarshalAs(UnmanagedType.LPStr)] string szParamName,        // char*
			[MarshalAs(UnmanagedType.LPWStr)] string value);            // unsigned short*

		// Set one or several parameters by a script ( < 48 kB ).
		// param szParamName:	Null Terminal ASCII String giving the script
		//						(script allows to change several parameters in the same time - SYNCHRO).
		//						Possible Instuction separators: ',' ';' or '\n'(CR)
		//						EXAMPLE:
		//						"Strip[0].gain = -6.0
		//						Strip[0].A1 = 0
		//						Strip[0].B1 = 1
		//						Strip[1].gain = -6.0
		//						Strip[2].gain = 0.0
		//						Strip[3].name = "Skype Caller" "
		// return :	 0: OK(no error).
		//			>0: number of line causing script error.
		//			-1: error
		//			-2: no server.
		//			-3: unexpected error
		//			-4: unexpected error
		[DllImport(VMRDLLName, CallingConvention = CallingConvention.StdCall)]
		internal static extern int VBVMR_SetParametersW(
			[MarshalAs(UnmanagedType.LPWStr)] string szParamScript);    // unsigned short*

		#endregion

		/******************************************************************************/
		/*                                                                            */
		/*                                Get Levels                                  */
		/*                                                                            */
		/******************************************************************************/

		#region Get Levels
		// Get Current levels. (this function must be called from one thread only)
		//
		// param nType:	0= pre fader input levels.
		//				1= post fader input levels.
		//				2= post Mute input levels.
		//				3= output levels.
		//
		// param nuChannel: audio channel zero based index 
		//					for input 0 = in#1 left, 1= in#1 Right, etc...
		//					for output 0 = busA ch1, 1 = busA ch2...
		//
		//		VOICEMEETER CHANNEL ASSIGNMENT
		//
		//		| Strip 1 | Strip 2 |             Virtual Input             |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 10 | 11 |
		//
		//		|             Output A1 / A2            |             Virtual Output            |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 10 | 11 | 12 | 13 | 14 | 15 |
		//
		//		VOICEMEETER BANANA CHANNEL ASSIGNMENT
		//
		//		| Strip 1 | Strip 2 | Strip 3 |             Virtual Input             |            Virtual Input AUX          |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 |
		//
		//		|             Output A1                 |                Output A2              |                Output A3              |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 |
		//
		//		|            Virtual Output B1          |             Virtual Output B2         |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 24 | 25 | 26 | 27 | 28 | 29 | 30 | 31 | 32 | 33 | 34 | 35 | 36 | 37 | 38 | 39 |
		//
		//		VOICEMEETER POTATO CHANNEL ASSIGNMENT
		//
		//		| Strip 1 | Strip 2 | Strip 3 | Strip 4 | Strip 5 |             Virtual Input             |            Virtual Input AUX          |                 VAIO3                 |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 25 | 25 | 26 | 27 | 28 | 29 | 30 | 31 | 32 | 33 |
		//
		//		|             Output A1                 |                Output A2              |                Output A3              |                Output A4              |                Output A5              |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 00 | 01 | 02 | 03 | 04 | 05 | 06 | 07 | 08 | 09 | 10 | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 | 19 | 20 | 21 | 22 | 23 | 24 | 25 | 26 | 27 | 28 | 29 | 30 | 31 | 32 | 33 | 34 | 35 | 36 | 37 | 38 | 39 |
		//
		//		|            Virtual Output B1          |             Virtual Output B2         |             Virtual Output B3         |
		//		+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+----+
		//		| 40 | 41 | 42 | 43 | 44 | 45 | 46 | 47 | 48 | 49 | 50 | 51 | 52 | 53 | 54 | 55 | 56 | 57 | 58 | 59 | 60 | 61 | 62 | 63 |
		//
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_GetLevel(int nType, int nuChannel, ref float value);

		// Get MIDI message from M.I.D.I. input device used by Voicemeeter M.I.D.I. mapping. (this function must be called from one thread only)
		// param pMIDIBuffer:	pointer on MIDI Buffer.Expected message size is below 4 bytes,
		//						but it's recommended to use 1024 Bytes local buffer to receive 
		//						possible multiple M.I.D.I. event message in optimal way: 
		//						unsigned char pBuffer[1024];
		// return :	>0: number of bytes placed in buffer(2 or 3 byte for usual M.I.D.I.message)
		//			-1: error
		//			-2: no server.
		//			-5: no MIDI data
		//			-6: no MIDI data
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_GetMidiMessage(byte[] pMIDIBuffer, long nbByteMax);

		#endregion

		/******************************************************************************/
		/*                                                                            */
		/*                                   Devices                                  */
		/*                                                                            */
		/******************************************************************************/

		#region Devices

		// Get number of Audio Output Device available on the system
		// return : return number of device found.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_Output_GetDeviceNumber();

		// Get the description of Audio Output Device 
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_Output_GetDeviceDescW(int zindex, ref int nType,
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder wszDeviceName,      // unsigned short*
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder wszHardwareId);    // unsigned short*


		// Get number of Audio Input Device available on the system
		// return : return number of device found.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_Input_GetDeviceNumber();

		// Get the description of Audio Input Device 
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_Input_GetDeviceDescW(int zindex, ref int nType, [
			MarshalAs(UnmanagedType.LPWStr)] StringBuilder wszDeviceName,      // unsigned short*
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder wszHardwareId);    // unsigned short*



		#endregion

		/******************************************************************************/
		/*                                                                            */
		/*                             VB-AUDIO CALLBACK                              */
		/*                                                                            */
		/******************************************************************************/

		#region VB-Audio

		// Register your audio callback function to receive real time audio buffer
		// it's possible to register up to 3x different Audio Callback in the same application or in 
		// 3x different applications.In the same application, this is possible because Voicemeeter
		// provides 3 kind of audio Streams:
		//			- AUDIO INPUT INSERT(to process all Voicemeeter inputs as insert)
		//			- AUDIO OUTPUT INSERT(to process all Voicemeeter BUS outputs as insert)
		//			- ALL AUDIO I/O(to process all Voicemeeter i/o).
		// Note: a single callback can be used to receive the 3 possible audio streams.
		//
		// param mode: callback type(main, input or bus output) see define below
		// param pCallback: Pointer on your callback function.
		// param lpUser: user pointer(pointer that will be passed in callback first argument).
		// param szClientName[64]:	IN: Name of the application registering the Callback.
		//							OUT: Name of the application already registered.
		// return:	 0: OK (no error).
		//			-1: error
		//			 1: callback already registered (by another application).
		[DllImport(VMRDLLName, CharSet = CharSet.Ansi)]
		internal static extern int VBVMR_AudioCallbackRegister(int mode, InternalAudioCallback callback, IntPtr user, byte[] ClientName);

		// Start / Stop Audio processing the Callback will be called with
		// return :	 0: OK(no error).
		//			-1: error
		//			-2: no callback registred.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_AudioCallbackStart();
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_AudioCallbackStop();

		// Unregister your callback to release voicemeeter virtual driver
		// (this function will automatically call VBVMR_AudioCallbackStop() function)
		// return :	 0: OK(no error).
		//			-1: error
		//			 1: callback already unregistered.
		[DllImport(VMRDLLName)]
		internal static extern int VBVMR_AudioCallbackUnregister();

		#endregion
	}
}
