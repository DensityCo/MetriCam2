// OrbbecOpenNI.h

#include <msclr/marshal.h>
#include <OpenNI.h>
#include "cmd.h"
#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Threading;
using namespace Metrilus::Util;
using namespace Metrilus::Logging;

namespace MetriCam2 
{
	namespace Cameras 
	{

		struct OrbbecNativeCameraData
		{
			cmd* openNICam;

			openni::VideoStream* depth;
			int depthWidth;
			int depthHeight;

			openni::VideoStream* ir;
			int irWidth;
			int irHeight;
		};

		public ref class AstraOpenNI : Camera
		{
		public:
			AstraOpenNI();
			~AstraOpenNI();

			property bool EmitterEnabled 
			{
				bool get(void) 
				{
					//Reader the emitter status via the "cmd" class does not yet work. Check in future version of experimental SDK.
					return emitterEnabled;
				}
				void set(bool value) 
				{
					emitterEnabled = value;
					SetEmitter(emitterEnabled);
					log->InfoFormat("Emitter state set to: {0}", emitterEnabled.ToString());
				}
			}

			property ParamDesc<bool>^ EmitterEnabledDesc
			{
				inline ParamDesc<bool> ^get()
				{
					ParamDesc<bool> ^res = gcnew ParamDesc<bool>();
					res->Unit = "";
					res->Description = "Emitter is enabled";
					res->ReadableWhen = ParamDesc::ConnectionStates::Connected | ParamDesc::ConnectionStates::Disconnected;
					res->WritableWhen = ParamDesc::ConnectionStates::Connected;
					return res;
				}
			}

			static System::Collections::Generic::Dictionary<String^, String^>^ GetSerialToUriMappingOfAttachedCameras();

		protected:
			/// <summary>
			/// Resets list of available channels (<see cref="Channels"/>) to union of all cameras supported by the implementing class.
			/// </summary>
			virtual void LoadAllAvailableChannels() override;

			/// <summary>
			/// Connects the camera.
			/// </summary>
			/// <remarks>This method is implicitely called by <see cref="Camera.Connect"/> inside a camera lock.</remarks>
			/// <seealso cref="Camera.Connect"/>
			virtual void ConnectImpl() override;

			/// <summary>
			/// Disconnects the camera.
			/// </summary>
			/// <remarks>This method is implicitely called by <see cref="Camera.Disconnect"/> inside a camera lock.</remarks>
			/// <seealso cref="Camera.Disconnect"/>
			virtual void DisconnectImpl() override;

			/// <summary>
			/// Updates data buffers of all active channels with data of current frame.
			/// </summary>
			/// <remarks>This method is implicitely called by <see cref="Camera.Update"/> inside a camera lock.</remarks>
			/// <seealso cref="Camera.Update"/>
			virtual void UpdateImpl() override;

			/// <summary>Computes (image) data for a given channel.</summary>
			/// <param name="channelName">Channel name.</param>
			/// <returns>(Image) Data.</returns>
			/// <seealso cref="Camera.CalcChannel"/>
			virtual Metrilus::Util::CameraImage^ CalcChannelImpl(String^ channelName) override;

			/// <summary>
			/// Activate a channel.
			/// </summary>
			/// <param name="channelName">Channel name.</param>
			virtual void ActivateChannelImpl(String^ channelName) override;

			/// <summary>
			/// Deactivate a channel.
			/// </summary>
			/// <param name="channelName">Channel name.</param>
			virtual void DeactivateChannelImpl(String^ channelName) override;
		private:
			FloatCameraImage^ CalcZImage();
			ColorCameraImage^ CalcColor();
			Point3fCameraImage^ CalcPoint3fImage();
			FloatCameraImage^ CalcIRImage();

			static bool OpenNIInit();
			static bool OpenNIShutdown();
			static void LogOpenNIError(String^ status);
			static int openNIInitCounter = 0;

			void SetEmitter(bool on);

			bool emitterEnabled;
			OrbbecNativeCameraData* camData;

			// for converting managed strings to const char*
			msclr::interop::marshal_context oMarshalContext;
			System::Collections::Generic::Dictionary<String^, String^>^ serialToUriDictionary;
		};
	}
}