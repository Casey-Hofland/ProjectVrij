using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace Extensions
	{
		public enum Difference
		{
			HigherThan,
			AtLeast,
			Equals,
			AtMost,
			LessThan
		}

		public static class Vector3Extensions
		{
			public static bool MagnitudeDiff(this Vector3 vector3, Vector3 comparer, Difference difference)
			{
				bool magnitudeDiff = false;

				switch (difference)
				{
					case Difference.HigherThan:
						magnitudeDiff = vector3.sqrMagnitude > comparer.sqrMagnitude;
						break;
					case Difference.AtLeast:
						magnitudeDiff = vector3.sqrMagnitude >= comparer.sqrMagnitude;
						break;
					case Difference.Equals:
						magnitudeDiff = vector3.sqrMagnitude == comparer.sqrMagnitude;
						break;
					case Difference.AtMost:
						magnitudeDiff = vector3.sqrMagnitude <= comparer.sqrMagnitude;
						break;
					case Difference.LessThan:
						magnitudeDiff = vector3.sqrMagnitude < comparer.sqrMagnitude;
						break;
					default:
						break;
				}

				return magnitudeDiff;
			}

			// INCLUDE SUMMARY THAT SAYS COMPARER SHOULD NOT HAVE TO BE CHANGED!
			public static bool MagnitudeDiff(this Vector3 vector3, float comparer, Difference difference)
			{
				bool magnitudeDiff = false;

				switch (difference)
				{
					case Difference.HigherThan:
						magnitudeDiff = vector3.sqrMagnitude > comparer * comparer;
						break;
					case Difference.AtLeast:
						magnitudeDiff = vector3.sqrMagnitude >= comparer * comparer;
						break;
					case Difference.Equals:
						magnitudeDiff = vector3.sqrMagnitude == comparer * comparer;
						break;
					case Difference.AtMost:
						magnitudeDiff = vector3.sqrMagnitude <= comparer * comparer;
						break;
					case Difference.LessThan:
						magnitudeDiff = vector3.sqrMagnitude < comparer * comparer;
						break;
					default:
						break;
				}

				return magnitudeDiff;
			}
		}
	}
}
