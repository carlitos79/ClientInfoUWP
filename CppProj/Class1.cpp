#include "pch.h"
#include "Class1.h"
#include<vector>

using namespace CppProj;
using namespace Platform;
using namespace Windows::Foundation::Collections;
using namespace concurrency;
using namespace Platform::Collections;
using namespace Windows::Devices::Geolocation;

Class1::Class1()
{	
}

Windows::Foundation::IAsyncOperation<Windows::Devices::Geolocation::Geoposition^>^ Class1::ReturnPosition(){

	auto geolocator = ref new Geolocator();
	auto pos = geolocator->GetGeopositionAsync();

	return pos;
}