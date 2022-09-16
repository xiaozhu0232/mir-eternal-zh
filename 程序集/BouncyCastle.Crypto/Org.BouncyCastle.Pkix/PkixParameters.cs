using System;
using System.Collections;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.Date;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Pkix;

public class PkixParameters
{
	public const int PkixValidityModel = 0;

	public const int ChainValidityModel = 1;

	private ISet trustAnchors;

	private DateTimeObject date;

	private IList certPathCheckers;

	private bool revocationEnabled = true;

	private ISet initialPolicies;

	private bool explicitPolicyRequired = false;

	private bool anyPolicyInhibited = false;

	private bool policyMappingInhibited = false;

	private bool policyQualifiersRejected = true;

	private IX509Selector certSelector;

	private IList stores;

	private IX509Selector selector;

	private bool additionalLocationsEnabled;

	private IList additionalStores;

	private ISet trustedACIssuers;

	private ISet necessaryACAttributes;

	private ISet prohibitedACAttributes;

	private ISet attrCertCheckers;

	private int validityModel = 0;

	private bool useDeltas = false;

	public virtual bool IsRevocationEnabled
	{
		get
		{
			return revocationEnabled;
		}
		set
		{
			revocationEnabled = value;
		}
	}

	public virtual bool IsExplicitPolicyRequired
	{
		get
		{
			return explicitPolicyRequired;
		}
		set
		{
			explicitPolicyRequired = value;
		}
	}

	public virtual bool IsAnyPolicyInhibited
	{
		get
		{
			return anyPolicyInhibited;
		}
		set
		{
			anyPolicyInhibited = value;
		}
	}

	public virtual bool IsPolicyMappingInhibited
	{
		get
		{
			return policyMappingInhibited;
		}
		set
		{
			policyMappingInhibited = value;
		}
	}

	public virtual bool IsPolicyQualifiersRejected
	{
		get
		{
			return policyQualifiersRejected;
		}
		set
		{
			policyQualifiersRejected = value;
		}
	}

	public virtual DateTimeObject Date
	{
		get
		{
			return date;
		}
		set
		{
			date = value;
		}
	}

	public virtual bool IsUseDeltasEnabled
	{
		get
		{
			return useDeltas;
		}
		set
		{
			useDeltas = value;
		}
	}

	public virtual int ValidityModel
	{
		get
		{
			return validityModel;
		}
		set
		{
			validityModel = value;
		}
	}

	public virtual bool IsAdditionalLocationsEnabled => additionalLocationsEnabled;

	public PkixParameters(ISet trustAnchors)
	{
		SetTrustAnchors(trustAnchors);
		initialPolicies = new HashSet();
		certPathCheckers = Platform.CreateArrayList();
		stores = Platform.CreateArrayList();
		additionalStores = Platform.CreateArrayList();
		trustedACIssuers = new HashSet();
		necessaryACAttributes = new HashSet();
		prohibitedACAttributes = new HashSet();
		attrCertCheckers = new HashSet();
	}

	public virtual ISet GetTrustAnchors()
	{
		return new HashSet(trustAnchors);
	}

	public virtual void SetTrustAnchors(ISet tas)
	{
		if (tas == null)
		{
			throw new ArgumentNullException("value");
		}
		if (tas.IsEmpty)
		{
			throw new ArgumentException("non-empty set required", "value");
		}
		trustAnchors = new HashSet();
		foreach (TrustAnchor ta in tas)
		{
			if (ta != null)
			{
				trustAnchors.Add(ta);
			}
		}
	}

	public virtual X509CertStoreSelector GetTargetCertConstraints()
	{
		if (certSelector == null)
		{
			return null;
		}
		return (X509CertStoreSelector)certSelector.Clone();
	}

	public virtual void SetTargetCertConstraints(IX509Selector selector)
	{
		if (selector == null)
		{
			certSelector = null;
		}
		else
		{
			certSelector = (IX509Selector)selector.Clone();
		}
	}

	public virtual ISet GetInitialPolicies()
	{
		ISet s = initialPolicies;
		if (initialPolicies == null)
		{
			s = new HashSet();
		}
		return new HashSet(s);
	}

	public virtual void SetInitialPolicies(ISet initialPolicies)
	{
		this.initialPolicies = new HashSet();
		if (initialPolicies == null)
		{
			return;
		}
		foreach (string initialPolicy in initialPolicies)
		{
			if (initialPolicy != null)
			{
				this.initialPolicies.Add(initialPolicy);
			}
		}
	}

	public virtual void SetCertPathCheckers(IList checkers)
	{
		certPathCheckers = Platform.CreateArrayList();
		if (checkers == null)
		{
			return;
		}
		foreach (PkixCertPathChecker checker in checkers)
		{
			certPathCheckers.Add(checker.Clone());
		}
	}

	public virtual IList GetCertPathCheckers()
	{
		IList list = Platform.CreateArrayList();
		foreach (PkixCertPathChecker certPathChecker in certPathCheckers)
		{
			list.Add(certPathChecker.Clone());
		}
		return list;
	}

	public virtual void AddCertPathChecker(PkixCertPathChecker checker)
	{
		if (checker != null)
		{
			certPathCheckers.Add(checker.Clone());
		}
	}

	public virtual object Clone()
	{
		PkixParameters pkixParameters = new PkixParameters(GetTrustAnchors());
		pkixParameters.SetParams(this);
		return pkixParameters;
	}

	protected virtual void SetParams(PkixParameters parameters)
	{
		Date = parameters.Date;
		SetCertPathCheckers(parameters.GetCertPathCheckers());
		IsAnyPolicyInhibited = parameters.IsAnyPolicyInhibited;
		IsExplicitPolicyRequired = parameters.IsExplicitPolicyRequired;
		IsPolicyMappingInhibited = parameters.IsPolicyMappingInhibited;
		IsRevocationEnabled = parameters.IsRevocationEnabled;
		SetInitialPolicies(parameters.GetInitialPolicies());
		IsPolicyQualifiersRejected = parameters.IsPolicyQualifiersRejected;
		SetTargetCertConstraints(parameters.GetTargetCertConstraints());
		SetTrustAnchors(parameters.GetTrustAnchors());
		validityModel = parameters.validityModel;
		useDeltas = parameters.useDeltas;
		additionalLocationsEnabled = parameters.additionalLocationsEnabled;
		selector = ((parameters.selector == null) ? null : ((IX509Selector)parameters.selector.Clone()));
		stores = Platform.CreateArrayList(parameters.stores);
		additionalStores = Platform.CreateArrayList(parameters.additionalStores);
		trustedACIssuers = new HashSet(parameters.trustedACIssuers);
		prohibitedACAttributes = new HashSet(parameters.prohibitedACAttributes);
		necessaryACAttributes = new HashSet(parameters.necessaryACAttributes);
		attrCertCheckers = new HashSet(parameters.attrCertCheckers);
	}

	public virtual void SetStores(IList stores)
	{
		if (stores == null)
		{
			this.stores = Platform.CreateArrayList();
			return;
		}
		foreach (object store in stores)
		{
			if (!(store is IX509Store))
			{
				throw new InvalidCastException("All elements of list must be of type " + typeof(IX509Store).FullName);
			}
		}
		this.stores = Platform.CreateArrayList(stores);
	}

	public virtual void AddStore(IX509Store store)
	{
		if (store != null)
		{
			stores.Add(store);
		}
	}

	public virtual void AddAdditionalStore(IX509Store store)
	{
		if (store != null)
		{
			additionalStores.Add(store);
		}
	}

	public virtual IList GetAdditionalStores()
	{
		return Platform.CreateArrayList(additionalStores);
	}

	public virtual IList GetStores()
	{
		return Platform.CreateArrayList(stores);
	}

	public virtual void SetAdditionalLocationsEnabled(bool enabled)
	{
		additionalLocationsEnabled = enabled;
	}

	public virtual IX509Selector GetTargetConstraints()
	{
		if (selector != null)
		{
			return (IX509Selector)selector.Clone();
		}
		return null;
	}

	public virtual void SetTargetConstraints(IX509Selector selector)
	{
		if (selector != null)
		{
			this.selector = (IX509Selector)selector.Clone();
		}
		else
		{
			this.selector = null;
		}
	}

	public virtual ISet GetTrustedACIssuers()
	{
		return new HashSet(trustedACIssuers);
	}

	public virtual void SetTrustedACIssuers(ISet trustedACIssuers)
	{
		if (trustedACIssuers == null)
		{
			this.trustedACIssuers = new HashSet();
			return;
		}
		foreach (object trustedACIssuer in trustedACIssuers)
		{
			if (!(trustedACIssuer is TrustAnchor))
			{
				throw new InvalidCastException("All elements of set must be of type " + typeof(TrustAnchor).FullName + ".");
			}
		}
		this.trustedACIssuers = new HashSet(trustedACIssuers);
	}

	public virtual ISet GetNecessaryACAttributes()
	{
		return new HashSet(necessaryACAttributes);
	}

	public virtual void SetNecessaryACAttributes(ISet necessaryACAttributes)
	{
		if (necessaryACAttributes == null)
		{
			this.necessaryACAttributes = new HashSet();
			return;
		}
		foreach (object necessaryACAttribute in necessaryACAttributes)
		{
			if (!(necessaryACAttribute is string))
			{
				throw new InvalidCastException("All elements of set must be of type string.");
			}
		}
		this.necessaryACAttributes = new HashSet(necessaryACAttributes);
	}

	public virtual ISet GetProhibitedACAttributes()
	{
		return new HashSet(prohibitedACAttributes);
	}

	public virtual void SetProhibitedACAttributes(ISet prohibitedACAttributes)
	{
		if (prohibitedACAttributes == null)
		{
			this.prohibitedACAttributes = new HashSet();
			return;
		}
		foreach (object prohibitedACAttribute in prohibitedACAttributes)
		{
			if (!(prohibitedACAttribute is string))
			{
				throw new InvalidCastException("All elements of set must be of type string.");
			}
		}
		this.prohibitedACAttributes = new HashSet(prohibitedACAttributes);
	}

	public virtual ISet GetAttrCertCheckers()
	{
		return new HashSet(attrCertCheckers);
	}

	public virtual void SetAttrCertCheckers(ISet attrCertCheckers)
	{
		if (attrCertCheckers == null)
		{
			this.attrCertCheckers = new HashSet();
			return;
		}
		foreach (object attrCertChecker in attrCertCheckers)
		{
			if (!(attrCertChecker is PkixAttrCertChecker))
			{
				throw new InvalidCastException("All elements of set must be of type " + typeof(PkixAttrCertChecker).FullName + ".");
			}
		}
		this.attrCertCheckers = new HashSet(attrCertCheckers);
	}
}
