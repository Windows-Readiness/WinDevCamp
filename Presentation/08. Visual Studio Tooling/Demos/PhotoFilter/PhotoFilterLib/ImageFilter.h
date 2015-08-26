#pragma once

#include <collection.h>

using namespace Windows::Storage::Streams;


namespace PhotoFilterLib_WinRT
{
	public delegate void PrimeFoundHandler(int result);
	public delegate void PixelArrayHandler(IBuffer^ result, int height, int width);
	public delegate void PixelStreamHandler(IRandomAccessStream^ result);



	public ref class ImageFilter sealed
	{
	public:
		ImageFilter();

		Windows::Foundation::IAsyncActionWithProgress<double>^ GetImage();
		Platform::Array<unsigned char>^ ImageFilter::AntiqueImage(const Platform::Array<unsigned char>^ buffer);
		Platform::Array<unsigned char>^ ImageFilter::AntiqueImageParallel(const Platform::Array<unsigned char>^ buffer);
		Platform::Array<unsigned char>^ ImageFilter::AntiqueImageParallelExplicit(const Platform::Array<unsigned char>^ buffer);

		event PixelArrayHandler^ pixelArrayEvent;
		event PixelStreamHandler^ pixelStreamEvent;

	private:
		bool is_prime(int n);
		Windows::UI::Core::CoreDispatcher^ m_dispatcher;
		void ThrowIfFailed(HRESULT hr);

	};
}