using System.Collections;
using System.Text;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Pkix;

public class PkixPolicyNode
{
	protected IList mChildren;

	protected int mDepth;

	protected ISet mExpectedPolicies;

	protected PkixPolicyNode mParent;

	protected ISet mPolicyQualifiers;

	protected string mValidPolicy;

	protected bool mCritical;

	public virtual int Depth => mDepth;

	public virtual IEnumerable Children => new EnumerableProxy(mChildren);

	public virtual bool IsCritical
	{
		get
		{
			return mCritical;
		}
		set
		{
			mCritical = value;
		}
	}

	public virtual ISet PolicyQualifiers => new HashSet(mPolicyQualifiers);

	public virtual string ValidPolicy => mValidPolicy;

	public virtual bool HasChildren => mChildren.Count != 0;

	public virtual ISet ExpectedPolicies
	{
		get
		{
			return new HashSet(mExpectedPolicies);
		}
		set
		{
			mExpectedPolicies = new HashSet(value);
		}
	}

	public virtual PkixPolicyNode Parent
	{
		get
		{
			return mParent;
		}
		set
		{
			mParent = value;
		}
	}

	public PkixPolicyNode(IList children, int depth, ISet expectedPolicies, PkixPolicyNode parent, ISet policyQualifiers, string validPolicy, bool critical)
	{
		if (children == null)
		{
			mChildren = Platform.CreateArrayList();
		}
		else
		{
			mChildren = Platform.CreateArrayList(children);
		}
		mDepth = depth;
		mExpectedPolicies = expectedPolicies;
		mParent = parent;
		mPolicyQualifiers = policyQualifiers;
		mValidPolicy = validPolicy;
		mCritical = critical;
	}

	public virtual void AddChild(PkixPolicyNode child)
	{
		child.Parent = this;
		mChildren.Add(child);
	}

	public virtual void RemoveChild(PkixPolicyNode child)
	{
		mChildren.Remove(child);
	}

	public override string ToString()
	{
		return ToString("");
	}

	public virtual string ToString(string indent)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(indent);
		stringBuilder.Append(mValidPolicy);
		stringBuilder.Append(" {");
		stringBuilder.Append(Platform.NewLine);
		foreach (PkixPolicyNode mChild in mChildren)
		{
			stringBuilder.Append(mChild.ToString(indent + "    "));
		}
		stringBuilder.Append(indent);
		stringBuilder.Append("}");
		stringBuilder.Append(Platform.NewLine);
		return stringBuilder.ToString();
	}

	public virtual object Clone()
	{
		return Copy();
	}

	public virtual PkixPolicyNode Copy()
	{
		PkixPolicyNode pkixPolicyNode = new PkixPolicyNode(Platform.CreateArrayList(), mDepth, new HashSet(mExpectedPolicies), null, new HashSet(mPolicyQualifiers), mValidPolicy, mCritical);
		foreach (PkixPolicyNode mChild in mChildren)
		{
			PkixPolicyNode pkixPolicyNode3 = mChild.Copy();
			pkixPolicyNode3.Parent = pkixPolicyNode;
			pkixPolicyNode.AddChild(pkixPolicyNode3);
		}
		return pkixPolicyNode;
	}
}
