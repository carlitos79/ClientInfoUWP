//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"

namespace Localizer
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public ref class MainPage sealed
	{
	private:
		Windows::Devices::Geolocation::Geolocator^ geolocator;
		Windows::Foundation::EventRegistrationToken positionToken;
		void OnPositionChanged(Windows::Devices::Geolocation::Geolocator^ sender, Windows::Devices::Geolocation::PositionChangedEventArgs^ e);

	public:
		MainPage();
	};
}
