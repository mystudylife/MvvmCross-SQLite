﻿using System;
using IgnoreAttribute = Cirrious.MvvmCross.Community.Plugins.Sqlite.IgnoreAttribute;

namespace SQLiteNetExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class RelationshipAttribute : IgnoreAttribute
    {
        protected RelationshipAttribute(string foreignKey, string inverseForeignKey, string inverseProperty, OnDeleteAction onDeleteAction)
        {
            InverseForeignKey = inverseForeignKey;
            InverseProperty = inverseProperty;
            ForeignKey = foreignKey;
            OnDeleteAction = onDeleteAction;
        }

        public string ForeignKey { get; private set; }
        public string InverseProperty { get; private set; }
        public string InverseForeignKey { get; private set; }
        public OnDeleteAction OnDeleteAction { get; private set; }
    }
}
