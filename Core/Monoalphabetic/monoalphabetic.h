#include "../header/common.h"

extern "C" {
char EXPORT *UseKeyword(const char *keyword, const int len, char *oa26Key);
char EXPORT *MonoalphabeticEncrypt(const char *plain, const int pLen,
                                   const char *keyword, const int kLen,
                                   char *cipher);
char EXPORT *MonoalphabeticDecrypt(const char *cipher, const int cLen,
                                   const char *keyword, const int kLen,
                                   char *plain);
}