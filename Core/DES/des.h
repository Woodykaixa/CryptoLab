#pragma once
#include "../header/common.h"

extern "C" {
byte EXPORT *DesEncrypt(byte *a8Plain, const byte *a8Key, byte *oa8Cipher);
byte EXPORT *DesDecrypt(byte *a8Cipher, const byte *a8Key, byte *oa8Plain);
}