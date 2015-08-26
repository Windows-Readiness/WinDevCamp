// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the PHOTOFILTERLIBWIN32_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// PHOTOFILTERLIBWIN32_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef PHOTOFILTERLIBWIN32_EXPORTS
#define PHOTOFILTERLIBWIN32_API __declspec(dllexport)
#else
#define PHOTOFILTERLIBWIN32_API __declspec(dllimport)
#endif

// This class is exported from the PhotoFilterLib.Win32.dll
class PHOTOFILTERLIBWIN32_API CPhotoFilterLibWin32 {
public:
	CPhotoFilterLibWin32(void);
	// TODO: add your methods here.
};

extern "C"  PHOTOFILTERLIBWIN32_API int nPhotoFilterLibWin32;

extern "C"  PHOTOFILTERLIBWIN32_API BYTE* ApplyAntiqueFilter(BYTE*, int size);
