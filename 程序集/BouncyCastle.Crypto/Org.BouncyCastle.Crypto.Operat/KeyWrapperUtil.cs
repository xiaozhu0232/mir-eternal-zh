using System;
using System.Collections;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Operators;

internal class KeyWrapperUtil
{
	private static readonly IDictionary providerMap;

	static KeyWrapperUtil()
	{
		providerMap = Platform.CreateHashtable();
		providerMap.Add("RSA/NONE/OAEPWITHSHA1ANDMGF1PADDING", new RsaOaepWrapperProvider(OiwObjectIdentifiers.IdSha1));
		providerMap.Add("RSA/NONE/OAEPWITHSHA224ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha224));
		providerMap.Add("RSA/NONE/OAEPWITHSHA256ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha256));
		providerMap.Add("RSA/NONE/OAEPWITHSHA384ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha384));
		providerMap.Add("RSA/NONE/OAEPWITHSHA512ANDMGF1PADDING", new RsaOaepWrapperProvider(NistObjectIdentifiers.IdSha512));
	}

	public static IKeyWrapper WrapperForName(string algorithm, ICipherParameters parameters)
	{
		WrapperProvider wrapperProvider = (WrapperProvider)providerMap[Strings.ToUpperCase(algorithm)];
		if (wrapperProvider == null)
		{
			throw new ArgumentException("could not resolve " + algorithm + " to a KeyWrapper");
		}
		return (IKeyWrapper)wrapperProvider.CreateWrapper(forWrapping: true, parameters);
	}

	public static IKeyUnwrapper UnwrapperForName(string algorithm, ICipherParameters parameters)
	{
		WrapperProvider wrapperProvider = (WrapperProvider)providerMap[Strings.ToUpperCase(algorithm)];
		if (wrapperProvider == null)
		{
			throw new ArgumentException("could not resolve " + algorithm + " to a KeyUnwrapper");
		}
		return (IKeyUnwrapper)wrapperProvider.CreateWrapper(forWrapping: false, parameters);
	}
}
