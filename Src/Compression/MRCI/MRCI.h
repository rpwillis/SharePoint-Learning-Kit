// MRCI.h
//
// Declare MRCIDecompress (which uses MRCI = Microsoft Realtime Compression Interface compression code to decompress buffers).

HRESULT MRCIDecompress(const void *pb, unsigned cb, TGrowBuf<BYTE> *pbufOut,
	unsigned cbOut);

HRESULT MRCIDecompressWrapper(const void *pb, int cb, const void *pOut,
    int cbOut);