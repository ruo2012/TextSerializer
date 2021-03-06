using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TheCodingMonkey.Serialization
{
    /// <summary>Used to serialize a TargetType object to a CSV file.</summary>
    /// <typeparam name="TTargetType">The type of object that will be serialized.  TargetType must have the 
    /// <see cref="TextSerializableAttribute">TextSerializable attribute</see> applied, and any fields contained must have the 
    /// <see cref="TextFieldAttribute">TextField attribute</see> applied to them.</typeparam>
    public class CsvSerializer<TTargetType> : TextSerializer<TTargetType>
        where TTargetType : new()
    {
        /// <summary>Initializes a new instance of the CSVSerializer class.</summary>
        public CsvSerializer()
        {
            AlwaysWriteQualifier = true;
            Delimiter = ',';
            Qualifier = '"';
        }

        /// <summary>Initializes a new instance of the CSVSerializer class.</summary>
        public CsvSerializer(bool alwaysWriteQualifier, char delimiter, char qualifier)
        {
            AlwaysWriteQualifier = alwaysWriteQualifier;
            Delimiter = delimiter;
            Qualifier = qualifier;
        }

        /// <summary>True if should wrap every field in the <see cref="Qualifier">Qualifier</see> during serialization.  If false, then
        /// the qualifier is only written if the field contains the <see cref="Delimiter">Delimiter</see>.</summary>
        public bool AlwaysWriteQualifier { get; set; }

        /// <summary>Character which is used to delimit fields in the record.</summary>
        public char Delimiter { get; set; }

        /// <summary>Character used to wrap a field if the field contins the <see cref="Delimiter">Delimiter</see>.</summary>
        public char Qualifier { get; set; }

        /// <summary>Serializes an array of objects to CSV Format</summary>
        /// <param name="writer">TextWriter where CSV text should go</param>
        /// <param name="records">Array of objects to serialize</param>
        /// <param name="writeHeader">True if should write a header record first, false otherwise</param>
        public void SerializeArray( TextWriter writer, ICollection<TTargetType> records, bool writeHeader )
        {
            if ( writeHeader )
            {
                var headerList = new List<string>();
                for ( int i = 0; i < _textFields.Count; i++ )
                    headerList.Add( _textFields[i].Name );

                writer.WriteLine( FormatOutput( headerList ) );
            }

            base.SerializeArray( writer, records );
        }

        /// <summary>Creates a collection of TargetType objects from a stream of text containing CSV.</summary>
        /// <param name="reader">TextReader to read in from.</param>
        /// <param name="readHeader">True if a header row is expected, false otherwise.</param>
        /// <returns>An array of objects containing the records in the file.</returns>
        public ICollection<TTargetType> DeserializeArray( TextReader reader, bool readHeader )
        {
            return DeserializeArray( reader, 0, readHeader );
        }

        /// <summary>Creates a collection of TargetType objects from a stream of text containing CSV.</summary>
        /// <param name="reader">TextReader to read in from.</param>
        /// <param name="count">Number of records to read.</param>
        /// <param name="readHeader">True if a header row is expected, false otherwise.</param>
        /// <returns>An array of objects containing the records in the file.</returns>
        /// <exception cref="TextSerializationException">Thrown if the header record does not match the 
        /// names of the parameters in the TargetType class.</exception>
        public ICollection<TTargetType> DeserializeArray( TextReader reader, int count, bool readHeader )
        {
            if ( readHeader )
            {
                string strHeader = reader.ReadLine();
                if ( string.IsNullOrEmpty( strHeader ) )
                {
                    List<string> headerList = Parse( strHeader );
                    for ( int i = 0; i < headerList.Count; i++ )
                    {
                        if ( _textFields[i].Name != headerList[i] )
                            throw new TextSerializationException( "Header row does not match structure" );
                    }
                }
            }
            return base.DeserializeArray( reader, count );
        }

        /// <summary>Parses a line of text as a record and returns the fields.</summary>
        /// <param name="text">A single line of text for the entire record out of the file.</param>
        /// <returns>A list of strings, where each string represents a field in the record.</returns>
        protected override List<string> Parse( string text )
        {
            List<string> returnList = new List<string>();       // Return value
            bool countDelimiter = true;                         // If we hit the delimiter character, should it be treated as a delimiter?
            StringBuilder currentField = new StringBuilder();   // Current field we're parsing through

            foreach ( char ch in text )
            {
                if ( ch == Qualifier )
                {
                    // We found a Qualifier character (usually a quote), so should treat a delimiter character as part of the field
                    countDelimiter = !countDelimiter;
                }
                else if ( ch == Delimiter )
                {
                    if ( countDelimiter )
                    {
                        // Found a delimiter, so end the field we're building up, add to our return list and clear the current field
                        returnList.Add( currentField.ToString() );
                        currentField = new StringBuilder();
                    }
                    else
                    {
                        // Inside of a qualified field, so just add this to the field string as usual
                        currentField.Append( ch );
                    }
                }
                else
                {
                    // Add this to the field string... just a normal character
                    currentField.Append( ch );
                }
            }

            // End of record, so add whatever we have to the list
            if ( countDelimiter )
                returnList.Add( currentField.ToString() );

            return returnList;
        }

        /// <summary>Write out a list of fields in CSV format.</summary>
        /// <param name="fieldList">List of strings where each string represents one field in the record.</param>
        /// <returns>A single CSV record.</returns>
        protected override string FormatOutput( List<string> fieldList )
        {
            StringBuilder sb = new StringBuilder();
            foreach ( string field in fieldList )
            {
                string qual = Qualifier.ToString();

                // Don't use the qualifier if AlwaysWriteQualifier is false, or if the field doesn't contain the delimiter
                if ( !AlwaysWriteQualifier && !field.Contains( Delimiter.ToString() ) )
                    qual = "";

                // Add the field to the record string
                sb.AppendFormat( "{0}{1}{0}{2}", qual, field, Delimiter );
            }

            // Remove the last delimiter from the end of the record
            if ( sb.Length > 0 )
                sb.Remove( sb.Length - 1, 1 );

            return sb.ToString();
        }
    }
}