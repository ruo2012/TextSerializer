namespace TheCodingMonkey.Serialization
{
    /// <summary>This attribute is applied to Fields or Properties of a class to control where in the fixed width file
    /// this field belongs.</summary>
    public class FixedWidthFieldAttribute : TextFieldAttribute
    {
        /// <summary>Default constructor.</summary>
        /// <param name="position">Position (column) where this field is serialized in the fixed width file.</param>
        /// <param name="size">Number of characters in the fixed width file that this field takes up.</param>
        public FixedWidthFieldAttribute( int position, int size )
        : base( position )
        {
            Size = size;
            Padding = ' ';
        }

        /// <summary>Character to use to pad a text field if it doesn't meet the minimum size requirement for the
        /// fixed length field.</summary>
        public char Padding { get; set; }
    }
}