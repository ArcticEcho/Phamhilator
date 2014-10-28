using System.Text.RegularExpressions;
using System;



namespace Phamhilator
{
	public class Term
	{
		public Regex Regex { get; private set; }
		public bool IsAuto { get; private set; }
		public string Site { get; private set; }
		public float Score { get; private set; }



		public Term(Regex regex, float score, string site = "", bool isAuto = false)
		{
			if (regex == null) { throw new ArgumentNullException("regex"); }

			Regex = regex;
			Score = score;
			Site = site;
			IsAuto = isAuto;
		}



		public static bool operator ==(Term a, Term b)
		{
			if (ReferenceEquals(a, b)) { return true; }

			if ((object)a == null || (object)b == null) { return false; } // Box args to avoid recursion.

			return a.GetHashCode() == b.GetHashCode();
		}

		public static bool operator !=(Term a, Term b)
		{
			return !(a == b);
		}

		public bool Equals(Term term)
		{
			if (term == null) { return false; }

			return term.GetHashCode() == GetHashCode();
		}

		public bool Equals(Regex regex, string site = "")
		{
			if (String.IsNullOrEmpty(regex.ToString())) { return false; }

			return regex.ToString() == Regex.ToString() && site == Site;
		}

		public override bool Equals(object obj)
		{
			if (obj == null) { return false; }

			if (!(obj is Term)) { return false; }

			return obj.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return Regex.ToString().GetHashCode() + Site.GetHashCode();
			}
		}
	}
}
