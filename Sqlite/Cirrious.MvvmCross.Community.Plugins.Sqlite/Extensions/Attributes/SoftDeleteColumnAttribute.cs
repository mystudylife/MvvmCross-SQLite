using System;

namespace Cirrious.MvvmCross.Community.Plugins.Sqlite.Extensions.Attributes {
	[AttributeUsage(AttributeTargets.Property)]
	public class SoftDeleteAttribute : Attribute {
		public SoftDeleteAttribute() { }
	}
}
