//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace Localizer;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::Devices::Geolocation;
using namespace Windows::UI::Core;
using namespace Bing::Maps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{
	InitializeComponent();

	geolocator = ref new Geolocator();
	positionToken = geolocator->PositionChanged::add(
		ref new TypedEventHandler<Geolocator^, PositionChangedEventArgs^>(
			this, &MainPage::OnPositionChanged));
}

void MainPage::OnPositionChanged(Geolocator^ sender, PositionChangedEventArgs^ e)
{
	Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this, e]()
	{
		auto coordinate = e->Position->Coordinate;
	},
			CallbackContext::Any
		)
	);
}