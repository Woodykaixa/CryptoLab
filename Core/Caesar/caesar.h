#pragma once
#include "../header/common.h"

char EXPORT *CaesarEncrypt(const char *plain, int textLen, char *cipher,
                           int key);
char EXPORT *CaesarDecrypt(const char *cipher, int cipherLen, char *plain,
                           int key);
