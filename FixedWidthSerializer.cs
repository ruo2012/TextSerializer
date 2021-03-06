using System.Collections.Generic;
using System.Text;

namespace TheCodingMonkey.Serialization
{
    /// <summary>Used to serialize a TargetType object to a CSV file.</summary>
    /// <typeparam name="TTargetType">The type of object that will be serialized.  TargetType must have the 
    /// <see cref="TextSerializableAttribute">TextSerializable attribute</see> applied, and any fields contained must have the 
    /// <see cref="FixedWidthFieldAttribute">TextField attribute</see> applied to them.</typeparam>
    public class FixedWidthSerializer<TTargetType> : TextSerializer<TTargetType>
        where TTargetType : new()
    {
        /// <summary>Parses a line of text as a record and returns the fields.</summary>
        /// <param name="text">A single line of text for the entire record out of the file.</param>
        /// <returns>A list of strings, where each string represents a field in the record.</returns>
        /// <exception cref="TextSerializationException">Thrown if the record length does not match the calculated length for the 
        /// TargetType object.  Also thrown if any one field's length is not positive.</exception>
        protected override List<string> Parse( string text )
        {
            List<string> returnList = new List<string>();
            foreach (FixedWidthFieldAttribute field in _textFields.Values)
            {
                int fieldLen = field.Size;
                int startPos = field.Position;

                // Double check that the field length attribute on the property or field is positive.
                if ( fieldLen < 0 )
                    throw new TextSerializationException( "TextField Size must be specified for Fixed Width" );

                // Double check that the field length for the property won't make us go over the total record length.
                if (startPos + fieldLen > text.Length)
                {
                    if (field.Optional)    // If we're going to go over, and this field is optional, then we can bail
                        break;
                    else                   // Otherwise, this is an error condition
                        throw new TextSerializationException("Fixed width field length mismatch");
                }

                // Trim out any padding characters that may exist
                string strField = text.Substring( startPos, fieldLen );
                returnList.Add( strField.Trim( field.Padding ) );
            }

            return returnList;
        }

        /// <summary>Write out a list of fields in the correct fixed width format.</summary>
        /// <param name="fieldList">List of strings where each string represents one field in the record.</param>
        /// <returns>A single record.</returns>
        protected override string FormatOutput( List<string> fieldList )
        {
            StringBuilder sb = new StringBuilder();
            for ( int i = 0; i < fieldList.Count; i++ )
            {
                // Compare the length of the field with what the fixed width file is expecting
                int paddingCount = _textFields[i].Size - fieldList[i].Length;

                // Pad the string if required
                if ( paddingCount > 0 )
                {
                    FixedWidthFieldAttribute fieldAttr = (FixedWidthFieldAttribute)_textFields[i];
                    sb.Append( fieldAttr.Padding, paddingCount );
                }

                // Add the string to the record
                sb.Append( fieldList[i] );
            }

            return sb.ToString();
        }
    }
}