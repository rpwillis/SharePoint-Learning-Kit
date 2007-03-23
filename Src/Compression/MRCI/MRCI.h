/* Copyright (c) Microsoft Corporation. All rights reserved. */

// MRCI.h
//
// Declare MRCIDecompress (which uses MRCI = Microsoft
// Realtime Compression Interface compression code, based on original code from
// MSliger, to decompress buffers).
//
// Microsoft Confidential
// Copyright (c) 1994-2000 Microsoft Corporation
// All Rights Reserved.
//

// prototypes from MRCI.cpp
HRESULT MRCIDecompress(const void *pb, unsigned cb, TGrowBuf<BYTE> *pbufOut,
	unsigned cbOut);

HRESULT MRCIDecompressWrapper(const void *pb, int cb, const void *pOut,
    int cbOut);

