using System;

namespace geomdata.vector.io
{
    public static class VectorFileReaderExtension
    {
        public static int GetOrdinalExt(this VectorFileReader inst, string colName)
        {
            try
            {
                return inst.GetOrdinal(colName);
            }
            catch (IndexOutOfRangeException)
            {
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static int GetInt32Ext(this VectorFileReader inst, int colIndex)
        {
            if (colIndex == -1)
            {
                return default;
            }

            return inst.GetInt32(colIndex);
        }
        
        public static double GetDoubleExt(this VectorFileReader inst, int colIndex)
        {
            if (colIndex == -1)
            {
                return default;
            }

            return inst.GetDouble(colIndex);
        }
        
        public static string GetStringExt(this VectorFileReader inst, int colIndex)
        {
            if (colIndex == -1)
            {
                return string.Empty;
            }

            return inst.GetString(colIndex);
        }
        
        public static DateTime GetDateTimeExt(this VectorFileReader inst, int colIndex)
        {
            if (colIndex == -1)
            {
                return default;
            }

            return inst.GetDateTime(colIndex);
        }
    }
}