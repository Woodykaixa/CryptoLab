#include "des.h"
#include <cstring>
#include <iostream>
static bit subkey[16][48];

static const char ipTable[64] = {
    58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4,
    62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
    57, 49, 41, 33, 25, 17, 9,  1, 59, 51, 43, 35, 27, 19, 11, 3,
    61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7};

static const char iprTable[64] = {
    40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
    38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
    36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
    34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9,  49, 17, 57, 25};

static const char expand48Table[] = {
    32, 1,  2,  3,  4,  5,  4,  5,  6,  7,  8,  9,  8,  9,  10, 11,
    12, 13, 12, 13, 14, 15, 16, 17, 16, 17, 18, 19, 20, 21, 20, 21,
    22, 23, 24, 25, 24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 01};

static const char pTable[] = {16, 7, 20, 21, 29, 12, 28, 17, 1,  15, 23,
                              26, 5, 18, 31, 10, 2,  8,  24, 14, 32, 27,
                              3,  9, 19, 13, 30, 6,  22, 11, 4,  25};

static const char keyTable1[] = {
    57, 49, 41, 33, 25, 17, 9,  1,  58, 50, 42, 34, 26, 18, 10, 2,  59, 51, 43,
    35, 27, 19, 11, 3,  60, 52, 44, 36, 63, 55, 47, 39, 31, 23, 15, 7,  62, 54,
    46, 38, 30, 22, 14, 6,  61, 53, 45, 37, 29, 21, 13, 5,  28, 20, 12, 4};

static const char keyTable2[] = {
    14, 17, 11, 24, 1,  5,  3,  28, 15, 6,  21, 10, 23, 19, 12, 4,
    26, 8,  16, 7,  27, 20, 13, 2,  41, 52, 31, 37, 47, 55, 30, 40,
    51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32};

static const int leftshifts[] = {1, 1, 2, 2, 2, 2, 2, 2,
                                 1, 2, 2, 2, 2, 2, 2, 1};

static const char s1[] = {14, 4,  13, 1, 2,  15, 11, 8,  3,  10, 6,  12, 5,
                          9,  0,  7,  0, 15, 7,  4,  14, 2,  13, 1,  10, 6,
                          12, 11, 9,  5, 3,  8,  4,  1,  14, 8,  13, 6,  2,
                          11, 15, 12, 9, 7,  3,  10, 5,  0,  15, 12, 8,  2,
                          4,  9,  1,  7, 5,  11, 3,  14, 10, 0,  6,  13};

static const char s2[] = {15, 1,  8,  14, 6,  11, 3, 4,  9,  7,  2,  13, 12,
                          0,  5,  10, 3,  13, 4,  7, 15, 2,  8,  14, 12, 0,
                          1,  10, 6,  9,  11, 5,  0, 14, 7,  11, 10, 4,  13,
                          1,  5,  8,  12, 6,  9,  3, 2,  15, 13, 8,  10, 1,
                          3,  15, 4,  2,  11, 6,  7, 12, 0,  5,  14, 9};

static const char s3[] = {10, 0,  9,  14, 6,  3,  15, 5,  1,  13, 12, 7,  11,
                          4,  2,  8,  13, 7,  0,  9,  3,  4,  6,  10, 2,  8,
                          5,  14, 12, 11, 15, 1,  13, 6,  4,  9,  8,  15, 3,
                          0,  11, 1,  2,  12, 5,  10, 14, 7,  1,  10, 13, 0,
                          6,  9,  8,  7,  4,  15, 14, 3,  11, 5,  2,  12};

static const char s4[] = {7,  13, 14, 3,  0,  6,  9,  10, 1,  2, 8,  5,  11,
                          12, 4,  15, 13, 8,  11, 5,  6,  15, 0, 3,  4,  7,
                          2,  12, 1,  10, 14, 9,  10, 6,  9,  0, 12, 11, 7,
                          13, 15, 1,  3,  14, 5,  2,  8,  4,  3, 15, 0,  6,
                          10, 1,  13, 8,  9,  4,  5,  11, 12, 7, 2,  14};

static const char s5[] = {2,  12, 4, 1,  7,  10, 11, 6, 8,  5,  3,  15, 13,
                          0,  14, 9, 14, 11, 2,  12, 4, 7,  13, 1,  5,  0,
                          15, 10, 3, 9,  8,  6,  4,  2, 1,  11, 10, 13, 7,
                          8,  15, 9, 12, 5,  6,  3,  0, 14, 11, 8,  12, 7,
                          1,  14, 2, 13, 6,  15, 0,  9, 10, 4,  5,  3};
static const char s6[] = {12, 1,  10, 15, 9,  2,  6,  8,  0,  13, 3, 4, 14,
                          7,  5,  11, 10, 15, 4,  2,  7,  12, 9,  5, 6, 1,
                          13, 14, 0,  11, 3,  8,  9,  14, 15, 5,  2, 8, 12,
                          3,  7,  0,  4,  10, 1,  13, 11, 6,  4,  3, 2, 12,
                          9,  5,  15, 10, 11, 14, 1,  7,  6,  0,  8, 13};

static const char s7[] = {4,  11, 2,  14, 15, 0,  8, 13, 3,  12, 9,  7,  5,
                          10, 6,  1,  13, 0,  11, 7, 4,  9,  1,  10, 14, 3,
                          5,  12, 2,  15, 8,  6,  1, 4,  11, 13, 12, 3,  7,
                          14, 10, 15, 6,  8,  0,  5, 9,  2,  6,  11, 13, 8,
                          1,  4,  10, 7,  9,  5,  0, 15, 14, 2,  3,  12};
static const char s8[] = {13, 2,  8, 4,  6,  15, 11, 1,  10, 9, 3, 14, 5,
                          0,  12, 7, 1,  15, 13, 8,  10, 3,  7, 4, 12, 5,
                          6,  11, 0, 14, 9,  2,  7,  11, 4,  1, 9, 12, 14,
                          2,  0,  6, 10, 13, 15, 3,  5,  8,  2, 1, 14, 7,
                          4,  10, 8, 13, 15, 12, 9,  0,  3,  5, 6, 1};

const static char *s_box[] = {s1, s2, s3, s4, s5, s6, s7, s8};

// In cpp, xor is an key word
void Xor(bit *a, const bit *b, int len) {
  for (int i = 0; i < len; i++) {
    a[i] ^= b[i];
  }
}

void TransformByTable(const bit *in, const char *table, int len, bit *out) {
  static bit tmp[256];
  for (int i = 0; i < len; i++) {
    tmp[i] = in[table[i] - 1];
  }
  memcpy(out, tmp, len);
}

void Substitution(const bit *a48, bit *oa32) {
  int row, col;
  for (int i = 0; i < 8; i++) {
    row = (a48[i * 6] << 1) + a48[i * 6 + 5];
    col = (a48[i * 6 + 1] << 3) + (a48[i * 6 + 2] << 2) +
          (a48[i * 6 + 3] << 1) + a48[i * 6 + 4];
    byte val = s_box[i][row * 16 + col];
    oa32[i * 4] = (val >> 3) & 1;
    oa32[i * 4 + 1] = (val >> 2) & 1;
    oa32[i * 4 + 2] = (val >> 1) & 1;
    oa32[i * 4 + 3] = val & 1;
  }
}

void DesRound(const bit *a48Key, bit *oa32Cipher) {
  bit tmp[48];
  TransformByTable(oa32Cipher, expand48Table, 48, tmp);
  Xor(tmp, a48Key, 48);
  Substitution(tmp, oa32Cipher);
  TransformByTable(oa32Cipher, pTable, 32, oa32Cipher);
}

void KeyShift(bit *arr, int len, int shiftBitCount) {
  bit *tmp = new bit[shiftBitCount];
  memcpy(tmp, arr, shiftBitCount);
  memcpy(arr, arr + shiftBitCount, len - shiftBitCount);
  memcpy(arr + len - shiftBitCount, tmp, shiftBitCount);
  delete[] tmp;
}

void ByteToBit(const byte *in, int byteCount, bit *out) {
  memset(out, 0, byteCount * 8);
  for (int i = 0; i < byteCount * 8; i++) {
    out[i] = (in[i / 8] >> (7 - i % 8)) & 1;
  }
}

void BitToByte(const bit *in, int byteCount, char *out) {
  memset(out, 0, byteCount);
  for (int i = 0; i < byteCount * 8; i++) {
    out[i / 8] |= in[i] << (7 - i % 8);
  }
}

void DesEncryptRound(bit *a64Plain, bit *oa64Cipher) {
  bit m[64], tmp[32];
  bit *li = m, *ri = m + 32;
  memcpy(m, a64Plain, 64);
  TransformByTable(m, ipTable, 64, m);
  for (auto &sk : subkey) {
    memcpy(tmp, ri, 32);
    DesRound(sk, ri);
    Xor(ri, li, 32);
    memcpy(li, tmp, 32);
  }
  memcpy(tmp, ri, 32);
  memcpy(ri, li, 32);
  memcpy(li, tmp, 32);
  TransformByTable(m, iprTable, 64, m);
  memcpy(oa64Cipher, m, 64);
}

void DesDecryptRound(bit *oa64Plain, bit *a64Cipher) {
  bit m[64], tmp[32];
  bit *li = m, *ri = m + 32;
  memcpy(m, a64Cipher, 64);
  TransformByTable(m, ipTable, 64, m);
  memcpy(tmp, ri, 32);
  memcpy(ri, li, 32);
  memcpy(li, tmp, 32);
  for (int i = 15; i >= 0; i--) {
    memcpy(tmp, li, 32);
    DesRound(subkey[i], li);
    Xor(li, ri, 32);
    memcpy(ri, tmp, 32);
  }
  TransformByTable(m, iprTable, 64, m);
  memcpy(oa64Plain, m, 64);
}

void ImportKey(const bit *a64Key) {
  bit k[64];
  bit *left = k, *right = k + 28;
  memcpy(k, a64Key, 64);
  TransformByTable(k, keyTable1, 56, k);
  for (int i = 0; i < 16; i++) {
    int shiftCount = 2;
    if (i == 0 || i == 1 || i == 8 || i == 15) {
      shiftCount = 1;
    }
    KeyShift(left, 28, shiftCount);
    KeyShift(right, 28, shiftCount);
    TransformByTable(k, keyTable2, 48, subkey[i]);
  }
}

byte *DesEncrypt(byte *a8Plain, const byte *a8Key, byte *oa8Cipher) {
  bit a64Plain[64], a64Key[64], a64Cipher[64];
  memset(a64Cipher, 0, 64);
  ByteToBit(a8Plain, 8, a64Plain);
  ByteToBit(a8Key, 8, a64Key);
  ImportKey(a64Key);
  DesEncryptRound(a64Plain, a64Cipher);
  BitToByte(a64Cipher, 8, oa8Cipher);
  return oa8Cipher;
}

byte *DesDecrypt(byte *a8Cipher, const byte *a8Key, byte *oa8Plain) {
  bit a64Plain[64], a64Key[64], a64Cipher[64];
  ByteToBit(a8Cipher, 8, a64Cipher);
  memset(a64Plain, 0, 64);
  ByteToBit(a8Key, 8, a64Key);
  ImportKey(a64Key);
  DesDecryptRound(a64Plain, a64Cipher);
  BitToByte(a64Plain, 8, oa8Plain);
  return oa8Plain;
}