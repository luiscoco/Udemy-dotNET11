# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual experimental System.Security.Cryptography.Hpke on the selected .NET 11 runtime. The explicit SYSLIB5009 opt-in is localized in source. A classical P-256/HKDF-SHA256/AES-128-GCM suite performs real encapsulation, encryption and authenticated decryption; no post-quantum or TLS interoperability claim is made. Modified ciphertext and mismatched HPKE application context are actually rejected. Hybrid Public Key Encryption is not synonymous with a post-quantum hybrid KEM.
