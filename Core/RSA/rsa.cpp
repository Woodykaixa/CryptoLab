#include "rsa.h"
#include "RSAPrimes.h"
#include <numeric>

/**
 * 返回 pow(a, n) % m
 */
uint32_t QuickPowMod(uint32_t a, uint32_t n, uint32_t m) {
  uint64_t result = 1;
  auto mod = static_cast<uint64_t>(m);
  uint64_t base = a % mod;
  while (n) {
    if (n & 1u) {
      result = (result * base) % m;
    }
    base = (base * base) % m;
    n >>= 1u;
  }
  return result;
}

bool RabinMiller(uint16_t p, int b) {
  auto n = p - 1u;
  auto m = n;
  int a = 0;
  while ((m & 1u) == 0) {
    a++;
    m >>= 1u;
  }
  auto x = QuickPowMod(b, m, p);
  if (x == 1) {
    return true;
  }
  int i = 1;
  while (true) {
    if (x == n) {
      return true;
    }
    if (i == a) {
      return false;
    }
    x = QuickPowMod(x, 2, p);
    i++;
  }
}

bool IsPrime(uint16_t p) {

  for (int i = 0; i < 5; i++) {
    if (!RabinMiller(p, RandomGen() | 0x0000FFFFu)) {
      return false;
    }
  }
  return true;
}

uint16_t PrimeGen() {
gen:
  auto p = static_cast<uint16_t>(RandomGen() | 0x00000001u);
  for (int Prime : Primes) {
    if (p % Prime == 0) {
      p += 2;
    }
  }
  if (!IsPrime(p)) {
    goto gen;
  }
  if (p < Primes[PrimeCount - 1]) {
    goto gen;
  }
  return p;
}

uint32_t ExtendGCD(uint32_t a, uint32_t b, int64_t &x, int64_t &y) {
  if (!b) {
    x = 1;
    y = 0;
    return a;
  }
  auto result = ExtendGCD(b, a % b, x, y);
  auto tmp = x;
  x = y;
  y = tmp - a / b * y;
  return result;
}

uint32_t ModInverse(uint32_t a, uint32_t m) {
  int64_t x, y;
  ExtendGCD(a, m, x, y);
  while (x < 0) {
    x += m;
  }
  return x % m;
}

uint32_t *RsaKeyGen(uint32_t *a3key) {
keygen:
  auto p = static_cast<uint32_t>(PrimeGen());
  auto q = static_cast<uint32_t>(PrimeGen());
  auto n = p * q;
  auto phiN = (p - 1) * (q - 1);
  auto e = static_cast<uint32_t>(PrimeGen());
  auto d = ModInverse(e, phiN);
  if ((static_cast<uint64_t>(e) * static_cast<uint64_t>(d)) % phiN != 1) {
    goto keygen;
  }
  a3key[0] = n;
  a3key[1] = e;
  a3key[2] = d;
  return a3key;
}

char *RsaEncrypt(const char *plain, int len, uint32_t e, uint32_t n,
                 char *cipher) {
  if (std::gcd(e, n) != 1) {
    throw std::exception("gcd(e, n) != 1");
  }

  int i = 0;
  while (i < len / 3) {
    uint32_t plainByte = 0u | (static_cast<uint8_t>(plain[3 * i]) << 16u) |
                         (static_cast<uint8_t>(plain[3 * i + 1]) << 8u) |
                         static_cast<uint8_t>(plain[3 * i + 2]);
    if (n < plainByte) {
      throw std::exception("value too large");
    }
    auto cipherByte = QuickPowMod(plainByte, e, n);
    cipher[4 * i] = (cipherByte >> 24) & 0xFF;
    cipher[4 * i + 1] = (cipherByte >> 16) & 0xFF;
    cipher[4 * i + 2] = (cipherByte >> 8) & 0xFF;
    cipher[4 * i + 3] = cipherByte & 0xFF;
    i++;
  }
  auto rest = len % 3;
  if (rest == 0) {
    return cipher;
  }
  uint32_t m2;
  if (rest == 1) {
    m2 = plain[len - 1] << 16;
  } else {
    m2 = (plain[len - 2] << 16) | (plain[len - 1] << 8);
  }
  auto c2 = QuickPowMod(m2, e, n);
  cipher[4 * i] = (c2 >> 24) & 0xFF;
  cipher[4 * i + 1] = (c2 >> 16) & 0xFF;
  cipher[4 * i + 2] = (c2 >> 8) & 0xFF;
  cipher[4 * i + 3] = c2 & 0xFF;
  return cipher;
}

char *RsaDecrypt(const char *cipher, int len, uint32_t d, uint32_t n,
                 char *plain) {

  if (std::gcd(d, n) != 1) {
    throw std::exception("gcd(d, n) != 1");
  }
  if (len % 4 != 0) {
    throw std::exception("len % 4 != 0");
  }
  int i = 0;
  while (i < len / 4) {
    uint32_t c = (static_cast<uint8_t>(cipher[4 * i]) << 24u) |
                 (static_cast<uint8_t>(cipher[4 * i + 1]) << 16u) |
                 (static_cast<uint8_t>(cipher[4 * i + 2]) << 8u) |
                 static_cast<uint8_t>(cipher[4 * i + 3]);
    if (n < c) {
      throw std::exception("value too large");
    }
    auto p = QuickPowMod(c, d, n);
    plain[3 * i] = (p >> 16) & 0xFF;
    plain[3 * i + 1] = (p >> 8) & 0xFF;
    plain[3 * i + 2] = p & 0xFF;
    i++;
  }
  return plain;
}
