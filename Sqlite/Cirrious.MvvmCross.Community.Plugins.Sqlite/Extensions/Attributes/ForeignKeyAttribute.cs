using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace SQLiteNetExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : IndexedAttribute
    {
        public OnDeleteAction OnDeleteAction { get; set; }

        public ForeignKeyAttribute(Type foreignType)
        {
            ForeignType = foreignType;
        }

        public Type ForeignType { get; private set; }
    }
}