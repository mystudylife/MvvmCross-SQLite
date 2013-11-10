namespace SQLiteNetExtensions.Attributes
{
    public enum OnDeleteAction
    {
        None,
        /// <summary>
        ///     Don't allow deleting if the relationship
        ///     is set.
        /// </summary>
        Deny,       // Don't allow deleting if relationship is set
        /// <summary>
        ///     Set foreign key to null.
        /// </summary>
        Nullify,
        /// <summary>
        ///     Delete the child entities.
        /// </summary>
        Cascade
    }

    public static class OnDeleteActionExtensions {
        public static string ToSql(this OnDeleteAction action) {
            switch (action) {
                case OnDeleteAction.Deny:
                    return "RESTRICT";
                case OnDeleteAction.Nullify:
                    return "SET NULL";
                case OnDeleteAction.Cascade:
                    return "CASCADE";
                default:
                    return "NO ACTION";
            }
        }
    }
}