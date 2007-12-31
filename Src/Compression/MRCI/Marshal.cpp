#pragma once

#include "Shared.h"
#include "GrowBuf.h"
#include "MRCI.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace Microsoft
{
	namespace LearningComponents
	{
		public ref class MRCI
		{
		public:
			static long MRCIDecompress(IntPtr pb, int cb, IntPtr output, int cbOut)
			{
				const void* pbc = reinterpret_cast<const void *>(pb.ToPointer());
				const void* outputc = reinterpret_cast<const void*>(output.ToPointer());
				return MRCIDecompressWrapper(pbc, cb, outputc, cbOut);
			}	
		};
	}
}