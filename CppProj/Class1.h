#pragma once

namespace CppProj
{
    public ref class Class1 sealed
    {
    public:
        Class1();
		Windows::Foundation::IAsyncOperation<Windows::Devices::Geolocation::Geoposition ^>^ ReturnPosition();

    };
}
