#pragma once

typedef int                 BOOL;
typedef unsigned char       BYTE;
typedef long				HRESULT;

#define TRUE                1
#define NULL				0

#define _HRESULT_TYPEDEF_(_sc)			((HRESULT)_sc)
#define S_OK                            ((HRESULT)0L)
#define E_FAIL                          _HRESULT_TYPEDEF_(0x80004005L)
#define E_OUTOFMEMORY                   _HRESULT_TYPEDEF_(0x8007000EL)