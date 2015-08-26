// Class1.cpp
#include "pch.h"
#include "ImageFilter.h"
#include <ppltasks.h>
#include <concurrent_vector.h>
#include <agents.h>
#include <robuffer.h>
#include <wrl.h>


using namespace PhotoFilterLib_WinRT;
using namespace concurrency;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Foundation::Collections;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Pickers;
using namespace Windows::Storage;
using namespace Windows::Storage::Streams;



ImageFilter::ImageFilter()
{
}

//Public API
Windows::Foundation::IAsyncActionWithProgress<double>^ ImageFilter::GetImage()
{
	auto window = Windows::UI::Core::CoreWindow::GetForCurrentThread();
	m_dispatcher = window->Dispatcher;

	//  Initialize file picker

	// Open a stream for the selected file
	auto open = ref new FileOpenPicker();
	open->SuggestedStartLocation = PickerLocationId::PicturesLibrary;
	open->ViewMode = PickerViewMode::Thumbnail;

	// Filter to include a sample subset of file types
	open->FileTypeFilter->Clear();
	open->FileTypeFilter->Append(".bmp");
	open->FileTypeFilter->Append(".png");
	open->FileTypeFilter->Append(".jpeg");
	open->FileTypeFilter->Append(".jpg");

	return create_async([this, open](progress_reporter<double> reporter) {
		m_dispatcher->RunAsync(CoreDispatcherPriority::Normal,
			ref new DispatchedHandler([this, open]()
		{

			create_task(open->PickSingleFileAsync()).then([this](StorageFile^ file)
			{
				if (file)
				{
					// Ensure the stream is disposed once the image is loaded
					create_task(file->OpenAsync(Windows::Storage::FileAccessMode::Read)).then([this](IRandomAccessStream^ fileStream)
					{
						/*this->pixelStreamEvent(fileStream);
						return;*/
						int decodePixelHeight = 600;
						int decodePixelWidth = 400;

						WriteableBitmap^ wbmp = ref new WriteableBitmap(decodePixelWidth, decodePixelHeight);
						wbmp->SetSource(fileStream);
						this->pixelArrayEvent(wbmp->PixelBuffer, wbmp->PixelHeight, wbmp->PixelWidth);
					});

				}
			});

		}, Platform::CallbackContext::Any));
	});
}


Platform::Array<unsigned char>^ ImageFilter::AntiqueImage(const Platform::Array<unsigned char>^ buffer)
{
	auto pixels = ref new Platform::Array<unsigned char>(buffer->Length);
	byte* pixelBuffer = new byte[buffer->Length];

	// make workload bigger so that it is easier to see the effects of parallelization
	for (unsigned int x = 0; x < buffer->Length; x += 4)
	{
		int rgincrease = 100;
		int red = buffer[x] + rgincrease;
		int green = buffer[x + 1] + rgincrease;
		int blue = buffer[x + 2];
		int alpha = buffer[x + 3];

		if (red > 255)
			red = 255;
		if (green > 255)
			green = 255;

		pixels[x] = red;
		pixels[x + 1] = green;
		pixels[x + 2] = blue;
		pixels[x + 3] = alpha;
	}
	return pixels;
}


Platform::Array<unsigned char>^ ImageFilter::AntiqueImageParallel(const Platform::Array<unsigned char>^ buffer)
{
	auto pixels = ref new Platform::Array<unsigned char>(buffer->Length);

	size_t iterations = 1000;
	size_t chunkSize = buffer->Length / iterations;
	parallel_for(size_t(0), iterations, [&](size_t i)
	{
		for (int n = 0; n < 8; n++)
		{
			int lower = i*chunkSize;
			int upper = lower + chunkSize;
			int startOffset = i*4;
			int endOffset = iterations*4 - startOffset;
			for (int x = lower; x < upper; x += 4)
			{
				int rgincrease = 100;
				int red = buffer[x] + rgincrease;
				int green = buffer[x + 1] + rgincrease;
				int blue = buffer[x + 2];
				int alpha = buffer[x + 3];

				if (red > 255)
					red = 255;
				if (green > 255)
					green = 255;

				pixels[x] = red;
				pixels[x + 1] = green;
				pixels[x + 2] = blue;
				pixels[x + 3] = alpha;
			}
		}
	});
	return pixels;
}

Platform::Array<unsigned char>^ ImageFilter::AntiqueImageParallelExplicit(const Platform::Array<unsigned char>^ buffer)
{
	auto pixels = ref new Platform::Array<unsigned char>(buffer->Length);

	parallel_invoke(
		[&pixels, &buffer]{
				for (int n = 0; n < 8; n++)
				{
					for (int x = 0; x < buffer->Length / 2; x += 4)
					{
						int rgincrease = 100;
						int red = buffer[x] + rgincrease;
						int green = buffer[x + 1] + rgincrease;
						int blue = buffer[x + 2];
						int alpha = buffer[x + 3];

						if (red > 255)
							red = 255;
						if (green > 255)
							green = 255;

						pixels[x] = red;
						pixels[x + 1] = green;
						pixels[x + 2] = blue;
						pixels[x + 3] = alpha;
					}
				}
			},
		[&pixels, &buffer]{
				for (int n = 0; n < 8; n++)
				{
					for (int x = buffer->Length / 2; x < buffer->Length; x += 4)
					{
						int rgincrease = 100;
						int red = buffer[x] + rgincrease;
						int green = buffer[x + 1] + rgincrease;
						int blue = buffer[x + 2];
						int alpha = buffer[x + 3];

						if (red > 255)
							red = 255;
						if (green > 255)
							green = 255;

						pixels[x] = red;
						pixels[x + 1] = green;
						pixels[x + 2] = blue;
						pixels[x + 3] = alpha;
					}
				}
		}
	);

	return pixels;
}

inline void ImageFilter::ThrowIfFailed(HRESULT hr)
{
	if (FAILED(hr))
	{
		throw Exception::CreateException(hr);
	}
}
