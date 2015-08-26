// PhotoFilterLib.Win32.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "PhotoFilterLib.Win32.h"
#include <stdio.h>

// This is an example of an exported variable
PHOTOFILTERLIBWIN32_API int nPhotoFilterLibWin32=0;

// This is an example of an exported function.
extern "C" PHOTOFILTERLIBWIN32_API BYTE* ApplyAntiqueFilter(BYTE* pixelArray, int size)
{
	BYTE* newBytes = new BYTE[size];
	for (unsigned int x = 0; x < size; x += 4)
	{
		int rgincrease = 100;
		int red = pixelArray[x] + rgincrease;
		int green = pixelArray[x + 1] + rgincrease;
		int blue = pixelArray[x + 2];
		int alpha = pixelArray[x + 3];

		if (red > 255)
			red = 255;
		if (green > 255)
			green = 255;

		newBytes[x] = red;
		newBytes[x + 1] = green;
		newBytes[x + 2] = blue;
		newBytes[x + 3] = alpha;
	}
	return newBytes;
}

// This is the constructor of a class that has been exported.
// see PhotoFilterLib.Win32.h for the class definition
CPhotoFilterLibWin32::CPhotoFilterLibWin32()
{
	return;
}
