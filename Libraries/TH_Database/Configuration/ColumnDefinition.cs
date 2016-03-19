
namespace TH_Database
{
    public enum DataType
    {
        // Boolean 0 - 10
        Boolean = 0,

        // Numeric 10 - 100
        Short = 10,
        Long = 20,
        Double = 30,

        // Text 100 - 200
        SmallText = 100,
        MediumText = 110,
        LargeText = 120,

        // Date 200 - 300
        DateTime = 200
    }

    public class ColumnDefinition
    {
        public ColumnDefinition() { }

        public ColumnDefinition(string columnName, DataType dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }

        public ColumnDefinition(string columnName, DataType dataType, bool primaryKey)
        {
            ColumnName = columnName;
            DataType = dataType;
            PrimaryKey = primaryKey;
        }

        public ColumnDefinition(string columnName, DataType dataType, bool primaryKey, bool notNull)
        {
            ColumnName = columnName;
            DataType = dataType;
            PrimaryKey = primaryKey;
            NotNull = notNull;
        }


        public string ColumnName { get; set; }

        public DataType DataType { get; set; }

        public bool NotNull { get; set; }
        public string Default { get; set; }

        public bool RowId { get; set; }

        public bool PrimaryKey { get; set; }
        public bool Unique { get; set; }

    }
}
