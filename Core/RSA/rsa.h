#include "../header/common.h"

#include <ctime>
#include <random>

std::minstd_rand0 RandomGen(time(nullptr));

extern "C" {
uint32_t EXPORT *RsaKeyGen(uint32_t *a3key);
char EXPORT *RsaEncrypt(const char *plain, int len, uint32_t e, uint32_t n,
                        char *cipher);
char EXPORT *RsaDecrypt(const char *cipher, int len, uint32_t d, uint32_t n,
                        char *plain);
}