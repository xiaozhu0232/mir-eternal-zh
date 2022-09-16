using System;

namespace ICSharpCode.SharpZipLib.Zip;

[Flags]
internal enum GeneralBitFlags
{
	Encrypted = 1,
	Method = 6,
	Descriptor = 8,
	Reserved = 0x10,
	Patched = 0x20,
	StrongEncryption = 0x40,
	EnhancedCompress = 0x1000,
	HeaderMasked = 0x2000
}
