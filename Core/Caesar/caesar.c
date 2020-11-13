#include "caesar.h"

#include <stdlib.h>

void CheckKey(const int key) {
  if (key < 0 || 25 < key) {
    perror("Invalid key");
    exit(1);
  }
}

char *CaesarEncrypt(const char *plain, int textLen, char *cipher, int key) {
  CheckKey(key);
  for (int i = 0; i < textLen; i++) {
    if (plain[i] < 'a' || 'z' < plain[i]) {
      cipher[i] = plain[i];
    } else {
      cipher[i] = (char)('A' + (plain[i] - 'a' + key) % 26);
    }
  }
  return cipher;
}

char *CaesarDecrypt(const char *cipher, int cipherLen, char *plain, int key) {
  CheckKey(key);
  for (int i = 0; i < cipherLen; i++) {
    if (cipher[i] < 'A' || 'Z' < cipher[i]) {
      plain[i] = cipher[i];
    } else {
      plain[i] = (char)('a' + (cipher[i] - 'A' - key + 26) % 26);
    }
  }
  return plain;
}