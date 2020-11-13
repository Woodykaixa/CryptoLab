#include "monoalphabetic.h"
#include <iostream>
#include <string>
#include <string_view>
#include <unordered_set>

static const char Alphabet[] = "abcdefghijklmnopqrstuvwxyz";

char *UseKeyword(const char *keyword, const int len, char *oa26Key) {
  std::unordered_set<char> set;
  int okIndex = 0;
  std::string word = keyword;
  if (len < word.length()) {
    word = word.substr(0, len);
  }

  word += Alphabet;
  for (int i = 0; i < word.length(); i++) {
    if ('z' < word[i] || word[i] < 'a') {
      continue;
    }

    if (set.find(word[i]) == set.end()) {
      oa26Key[okIndex++] = word[i] + ('A' - 'a');
      set.insert(word[i]);
    }
  }
  return oa26Key;
}

char *MonoalphabeticEncrypt(const char *plain, const int pLen,
                            const char *keyword, const int kLen, char *cipher) {
  char key[26];
  UseKeyword(keyword, kLen, key);
  std::string_view alphabet = Alphabet;
  for (int i = 0; i < pLen; i++) {
    auto pos = alphabet.find(plain[i]);
    if (pos == -1) {
      cipher[i] = plain[i];
    } else {
      cipher[i] = key[pos];
    }
  }
  return cipher;
}

char *MonoalphabeticDecrypt(const char *cipher, const int cLen,
                            const char *keyword, const int kLen, char *plain) {
  char keyString[27];
  keyString[26] = '\0';
  UseKeyword(keyword, kLen, keyString);
  std::string_view cipherKey = keyString;
  for (int i = 0; i < cLen; i++) {
    auto pos = cipherKey.find(cipher[i]);
    if (pos == -1) {
      plain[i] = cipher[i];
    } else {
      plain[i] = Alphabet[pos];
    }
  }
  return plain;
}